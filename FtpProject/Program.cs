namespace ServertFtp
{
    using Application.Interfaces;
    using Application.Services;
    using Domain.Entities;
    using FtpProject.Controller;
    using FtpProject.Dto;
    using Infraestructure.Implements;
    using Infraestructure.Interfaces;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Newtonsoft.Json;
    using FtpProject.Mappers;
    using System.IO;

    public class Program
    {
        static Semaphore semaphore = new Semaphore(1, 1); // Crea un semáforo

        public static void Main(string[] args)
        {

            ServerController serverController = new ServerController("../../../Documents/");

            int port = 5000;
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(ipAddress, port); // Puerto de escucha del servidor
            

            listener.Start();

            Console.WriteLine($"Servidor escuchando en {ipAddress} {port}. Esperando conexiones...");


            try
            {
                TcpClient client = new TcpClient();
                while (true)
                {
                    // Esperar a que se reciba una nueva conexión de cliente
                    client = listener.AcceptTcpClient();
                    HandleClientConnection(serverController, client);
                }
            }
            finally
            {
                // Detener el servidor y liberar recursos al finalizar
                listener.Stop();
            }


        }

        private static void HandleClientConnection(ServerController serverController, TcpClient client)
        {

            ClientConnection connection = serverController.newConnection(client);

            
            int bytesRead = 0;

            string dataToSend = "";
            string requestJson = "";

            RequestDto request = new RequestDto();
            ResponseDto response = new ResponseDto();


            if (connection == null)
            {
                response.Status = "503";
                response.Data = "Servidor lleno por el momento";
                NetworkStream stream = client.GetStream();
                dataToSend = JsonConvert.SerializeObject(response);
                byte [] buffer = System.Text.Encoding.ASCII.GetBytes(dataToSend);
                stream.Write(buffer, 0, buffer.Length);
                return;
            }
            else
            {
                response.Status = "200";
                response.Data = "Logeado";
                NetworkStream stream = client.GetStream();
                dataToSend = JsonConvert.SerializeObject(response);
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(dataToSend);
                stream.Write(buffer, 0, buffer.Length);
            }

       
            // Tratar la conexión de cliente en un hilo de ejecución separado
            Thread thread = new Thread(() =>
            {
                Console.WriteLine($"Cliente conectado desde {connection.IpAddress}");

                NetworkStream stream = connection.TcpClient.GetStream(); // para leer lo enviado por los sockets
                byte[] bufferGetMessages = new byte[1024];

                try
                {
                    while (true)
                    {
                        if ((bytesRead = stream.Read(bufferGetMessages, 0, bufferGetMessages.Length)) <= 0 && !connection.TcpClient.Connected)
                        {
                            break;
                        }

                        requestJson = Encoding.UTF8.GetString(bufferGetMessages, 0, bufferGetMessages.Length);

                        request = JsonConvert.DeserializeObject<RequestDto>(requestJson); //Convertir el string en json


                        string service = request.Service.ToLower();
                        string type = request.Type.ToLower();
                        string body = request.Body;

                        if (type.Equals("get"))
                        {

                            if (service.Equals("list-users"))
                            {
                                semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                String getAllConnections = ServerMappers.fromConnectionsToString(serverController.listConnections());

                                semaphore.Release(); // El hilo libera el semáforo (recurso)


                                response.Status = "200";
                                response.Data = getAllConnections;


                            }
                            else if (service.Equals("list-documents"))
                            {

                                semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                String getAllConnections = ServerMappers.fromDocumentsToString(serverController.listDocuments(connection.IpAddress));

                                semaphore.Release(); // El hilo libera el semáforo (recurso)


                                response.Status = "200";
                                response.Data = getAllConnections;
                            }
                            else if (service.Equals("get-document"))
                            {

                            }
                            else
                            {
                                response.Status = "404";
                                response.Data = "servicio no disponible";
                            }
                        }
                        else if (type.Equals("post"))
                        {

                            if (service.Equals("send-document"))
                            {
                                response.Status = "200";
                                response.Data = "Listo para recibir";
                                dataToSend = JsonConvert.SerializeObject(response);
                                byte [] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                                stream.Write(buffer, 0, buffer.Length);

                                //Leer los bytes del archivo
                                MemoryStream memoryStream = new MemoryStream();

                                byte[] bufferDocument = new byte[5000];

                                while ((bytesRead = stream.Read(bufferDocument, 0, bufferDocument.Length)) > 0)
                                {
                                    memoryStream.Write(bufferDocument, 0, bufferDocument.Length);
                                }

                                semaphore.WaitOne(); // El hilo espera para tomar el semáforo (recurso)

                                bool isDocumentSaved = serverController.saveDocument(memoryStream.ToArray(), body, connection.IpAddress);

                                semaphore.Release(); // El hilo libera el semáforo (recurso)


                                if (!isDocumentSaved) {
                                    response.Status = "500";
                                    response.Data = "Error al recibir el archivo";
                                }
                                else
                                {
                                    response.Status = "201";
                                    response.Data = "Archivo recibido correctamente";
                                }
                            }
                            else
                            {
                                response.Status = "404";
                                response.Data = "servicio no disponible";
                            }
                            
                        }
                        else
                        {
                            response.Status = "400";
                            response.Data = "servicio no disponible";
                        }

                        dataToSend = JsonConvert.SerializeObject(response);
                        byte [] bufferSend = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                        stream.Write(bufferSend, 0, bufferSend.Length);
                    }
                }
                catch(Exception ex)
                {
                    if (connection.TcpClient.Connected)
                    {
                        ResponseDto response = new ResponseDto()
                        {
                            Status = "503",
                            Data = "Hubo un error en el servidor"
                        };

                        dataToSend = JsonConvert.SerializeObject(response);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        Console.WriteLine("Algo ha fallado en el servidor " + ex.Message);
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
//TimeSpan ts = new TimeSpan(0, 0, 1);

//IClientFtpRepository _clientRepository = new ClientFtpRepository();

//IClientFtpServices  _clientServices = new ClientFtpServices(_clientRepository);

//for (int i = 0; i < 10; i++)
//{
//    ClientFTP client = new ClientFTP(i.ToString(), DateTime.Now.ToString("HH:mm"), DateTime.Now.ToString("dd/MM/yyyy"));
//    _clientServices.newFtpClient(client);
//    Thread.Sleep(ts);
//}

//foreach(ClientFTP clients in _clientServices.getFtpClients())
//{
//    Console.WriteLine(clients.toString() + "\n");
//}