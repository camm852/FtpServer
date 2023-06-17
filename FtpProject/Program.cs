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

    public class Program
    {
        public static void Main(string[] args)
        {

            ServerController serverController = new ServerController();

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


            if (connection == null)
            {
                NetworkStream stream = client.GetStream();
                string dataToSend = "Servidor lleno por el momento";
                byte[] data = System.Text.Encoding.ASCII.GetBytes(dataToSend);
                stream.Write(data, 0, data.Length);
                return;
            }


            // Tratar la conexión de cliente en un hilo de ejecución separado
            Thread thread = new Thread(() =>
            {
                try
                {
                    Console.WriteLine($"Cliente conectado desde {connection.IpAddress}");

                    NetworkStream stream = connection.TcpClient.GetStream();
                    byte[] buffer = new byte[4048];
                    int bytesRead = 0;
                    String requestJson = "";
                    RequestDto request = new RequestDto();
                    ResponseDto response = new ResponseDto();
                    while (true)
                    {
                        if ((bytesRead = stream.Read(buffer, 0, buffer.Length)) <= 0 && !connection.TcpClient.Connected)
                        {
                            Console.WriteLine("Desconectado");
                            break;
                        }

                        //Convertir el json a mi Dto
                        requestJson = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        request = JsonConvert.DeserializeObject<RequestDto>(requestJson);


                        string service = request.Service.ToLower();
                        string type = request.Type.ToLower();

                        if (type.Equals("get"))
                        {

                            if (service.Equals("list-users"))
                            {
                                String getAllConnections = ConnectionsMapper.fromConnectionsToString(serverController.listConnections());

                                response.Status = "200";
                                response.Data = getAllConnections;

                            }
                            else if (service.Equals("list-documents"))
                            {

                            }else if (service.Equals("get-document"))
                            {

                            }
                            else
                            {
                                response.Status = "404";
                                response.Data = "servicio no disponible";
                            }
                            string dataToSend = JsonConvert.SerializeObject(response);
                            buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                            stream.Write(buffer, 0, buffer.Length);
                            Array.Clear(buffer, 0, buffer.Length);

                        }
                        else if (type.Equals("post"))
                        {

                            if (service.Equals("send-document"))
                            {

                            }
                            else
                            {
                                response.Status = "404";
                                response.Data = "servicio no disponible";
                            }
                            string dataToSend = JsonConvert.SerializeObject(response);
                            buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                            stream.Write(buffer, 0, buffer.Length);
                            Array.Clear(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            response.Status = "400";
                            response.Data = "servicio no disponible";

                            string dataToSend = JsonConvert.SerializeObject(response);
                            buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                            stream.Write(buffer, 0, buffer.Length);
                            Array.Clear(buffer, 0, buffer.Length);
                        }

                    }

                }catch(Exception e)
                {

                }
                finally
                {
                    Console.WriteLine("Cliente desconectado");
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