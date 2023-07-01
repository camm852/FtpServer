namespace ServertFtp
{
    using Domain.Entities;
    using FtpProject.Controller;
    using FtpProject.Dto;
    using FtpProject.Mappers;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;

    public class Program
    {
        static Semaphore semaphore = new Semaphore(1, 1); // Crea un semáforo

        static int maxReadTimeOut = 300000;

        static int maxReadTimeOutDocument = 1000;

        public static void Main(string[] args)
        {

            int port = 5000;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            NetworkInterface wifiInterface = GetActiveWifiInterface();
            NetworkInterface ethernetInterface = GetActiveEthernetInterface();

            if (wifiInterface != null)
            {
                Console.WriteLine("Conectado a través de Wi-Fi");
                Console.WriteLine("Nombre de la interfaz: " + wifiInterface.Name);
                Console.WriteLine("Descripción: " + wifiInterface.Description);
                ipAddress = IPAddress.Parse(GetIPv4Address(wifiInterface));
            }
            else if (ethernetInterface != null)
            {
                Console.WriteLine("Conectado a través de Ethernet");
                Console.WriteLine("Nombre de la interfaz: " + ethernetInterface.Name);
                Console.WriteLine("Descripción: " + ethernetInterface.Description);
                ipAddress = IPAddress.Parse(GetIPv4Address(ethernetInterface));
            }


            ServerController serverController = new ServerController(2, @"C:\FtpDocuments\");


            //Socket para clientes
            
            TcpListener listener = new TcpListener(ipAddress, port);
            

            listener.Start(); //Inicio server

            //Console.WriteLine(Directory.GetCurrentDirectory());

            Console.WriteLine($"Servidor con Ip: {ipAddress} escuchando por el puerto: {port}. Esperando conexiones...");


            try
            {
                TcpClient client = new TcpClient();
                while (true)
                {
                    client = listener.AcceptTcpClient(); //Esperando conexiones
                    HandleClientConnection(serverController, client);
                }
            }
            finally
            {
                listener.Stop(); //Detener el server
            }


        }

        private static NetworkInterface GetActiveWifiInterface()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in interfaces)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic;
                }
            }

            return null;
        }

        private static NetworkInterface GetActiveEthernetInterface()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in interfaces)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic;
                }
            }

            return null;
        }

        private static string GetIPv4Address(NetworkInterface networkInterface)
        {
            IPInterfaceProperties properties = networkInterface.GetIPProperties();
            IPAddress ipAddress = properties
                .UnicastAddresses
                .FirstOrDefault(address =>
                    address.Address.AddressFamily == AddressFamily.InterNetwork
                )?.Address;

            return ipAddress?.ToString();
        }


        private static void HandleClientConnection(ServerController serverController, TcpClient client)
        {
            Thread thread = new Thread(() => //Lanzando hilo
            {

                ClientConnection connection = serverController.newConnection(client);

                if (connection == null) //Si no hay conexiones en el object pool
                {
                    ResponseDto responseNullConnection = new ResponseDto();

                    responseNullConnection.status = "503";
                    responseNullConnection.data = "Servidor lleno por el momento";
                    NetworkStream streamNullConnection = client.GetStream();
                    string data = JsonConvert.SerializeObject(responseNullConnection);
                    byte[] bufferNullConnection = System.Text.Encoding.ASCII.GetBytes(data);
                    streamNullConnection.Write(bufferNullConnection, 0, bufferNullConnection.Length);// Rechazar conexion
                    return;
                }

                RequestDto request = new RequestDto();
                ResponseDto response = new ResponseDto();

                int bytesRead = 0;
                string dataToSend = "";
                string requestJson = "";
                byte[] bufferGetMessages = new byte[1024];

                NetworkStream streamConnection = connection.TcpClient.GetStream(); // para leer lo enviado por los sockets
                streamConnection.ReadTimeout = maxReadTimeOut;


                response.status = "200";
                response.data = "Logeado";
                dataToSend = JsonConvert.SerializeObject(response);
                byte[] bufferAcceptConnection = System.Text.Encoding.ASCII.GetBytes(dataToSend);
                streamConnection.Write(bufferAcceptConnection, 0, bufferAcceptConnection.Length); // Confirmarle al usuario que si se acepto la conexion


                Console.WriteLine($"Cliente conectado desde {connection.IpAddress}");

                try
                {
                    while (true)
                    {
                        if ((bytesRead = streamConnection.Read(bufferGetMessages, 0, bufferGetMessages.Length)) <= 0 && !connection.TcpClient.Connected)
                        {
                            break;
                        }

                        requestJson = Encoding.UTF8.GetString(bufferGetMessages, 0, bufferGetMessages.Length);
                        request = JsonConvert.DeserializeObject<RequestDto>(requestJson);

                        string service = request.service.ToLower();
                        string type = request.type.ToLower();
                        string body = request.body;

                        if (type.Equals("get"))
                        {
                            if (service.Equals("list-users"))
                            {
                                semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                String getAllConnections = ServerMappers.fromConnectionsToString(serverController.listConnections());

                                semaphore.Release(); // El hilo libera el semáforo (recurso)

                                response.status = "200";
                                response.data = getAllConnections;
                            }
                            else if (service.Equals("list-documents"))
                            {
                                semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                String getAllDocuments = ServerMappers.fromDocumentsToString(serverController.listDocuments(connection.IpAddress));

                                semaphore.Release(); // El hilo libera el semáforo (recurso)

                                response.status = "200";
                                response.data = getAllDocuments;
                            }
                            else if (service.Equals("get-document"))
                            {
                                semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                FileInfo fileInfo = serverController.findDocument(body);
                                
                                semaphore.Release(); // El hilo libera el semáforo (recurso)

                                if (fileInfo == null)
                                {
                                    response.status = "404";
                                    response.data = "Archivo no encontrado";
                                }
                                else
                                {
                                    response.status = "200";
                                    response.data = "El archivo se enviara en breve";
                                    dataToSend = JsonConvert.SerializeObject(response);
                                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                                    streamConnection.Write(buffer, 0, buffer.Length);

                                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

                                    byte[] bufferReadDocument = new byte[1024];

                                    while ((bytesRead = fileStream.Read(bufferReadDocument, 0, bufferReadDocument.Length)) > 0)
                                    {

                                        streamConnection.Write(bufferReadDocument, 0, bytesRead);
                                    }

                                    Console.WriteLine($"Se envio archivo {fileInfo.Name} a {connection.IpAddress}");

                                    fileStream.Close();

                                    Thread.Sleep(1500); //Tiempo de espera para que no se concatene el buffer


                                    response.status = "200";
                                    response.data = "Archivo descargado correctamente";

                                    //continue;
                                }
                            }
                            else
                            {
                                response.status = "404";
                                response.data = "servicio no disponible";
                            }
                        }
                        else if (type.Equals("post"))
                        {
                            if (service.Equals("send-document"))
                            {
                                response.status = "200";
                                response.data = "Listo para recibir";
                                dataToSend = JsonConvert.SerializeObject(response);
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                                streamConnection.Write(buffer, 0, buffer.Length);

                                MemoryStream memoryStream = new MemoryStream(); //Leer los bytes del archivo

                                byte[] bufferDocument = new byte[1024];

                                streamConnection.ReadTimeout = maxReadTimeOutDocument; //Timeout en 1 seg

                                try
                                {
                                    while ((bytesRead = streamConnection.Read(bufferDocument, 0, bufferDocument.Length)) > 0) // leer los bits del documento
                                    {
                                        memoryStream.Write(bufferDocument, 0, bytesRead);

                                    }
                                } catch (System.IO.IOException ex)
                                {
                                    if (ex.InnerException is SocketException socketException && socketException.SocketErrorCode == SocketError.TimedOut)
                                    {

                                        semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                        bool isDocumentSaved = serverController.saveDocument(memoryStream.ToArray(), body, connection.IpAddress);

                                        semaphore.Release(); // El hilo libera el semáforo (recurso)

                                        memoryStream.Close();

                                        if (!isDocumentSaved)
                                        {
                                            response.status = "500";
                                            response.data = "Error al recibir el archivo";
                                        }
                                        else
                                        {
                                            response.status = "201";
                                            response.data = "Archivo recibido correctamente";
                                        }
                                    }
                                    else
                                    {
                                        response.status = "500";
                                        response.data = "Error al recibir el archivo";
                                    }
                                }
                                streamConnection.ReadTimeout = maxReadTimeOut; //Devolvemos el timeout a 5 min
                            }
                            else
                            {
                                response.status = "404";
                                response.data = "servicio no disponible";
                            }
                        }
                        else
                        {
                            response.status = "400";
                            response.data = "servicio no disponible";
                        }

                        dataToSend = JsonConvert.SerializeObject(response);
                        byte[] bufferSend = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                        streamConnection.Write(bufferSend, 0, bufferSend.Length);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    if (connection.TcpClient.Connected)
                    {
                        ResponseDto responseCloseConnection = new ResponseDto()
                        {
                            status = "503",
                            data = "Hubo un error en el servidor"
                        };

                        dataToSend = JsonConvert.SerializeObject(responseCloseConnection);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                        streamConnection.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        Console.WriteLine("Algo ha fallado en el servidor ");
                    }
                }
                finally
                {
                    Console.WriteLine($"Cliente {connection.IpAddress} desconectado");

                    // Devolver la conexión al pool cuando el cliente se desconecte
                    serverController.deleteConnection(connection);
                }
            });
            thread.Start();
        }

    }
}
