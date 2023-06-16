namespace ServertFtp
{
    using Application.Services;
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class Program
    {

        //TODO pushear issue patron de diseño

        public static void Main()
        {

            ServerServices server = new ServerServices(1);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 5000;
            TcpListener listener = new TcpListener(ipAddress, port); // Puerto de escucha del servidor
            listener.Start();

            Console.WriteLine($"Servidor escuchando en {ipAddress} {port}. Esperando conexiones...");


            try
            {
                while (true)
                {
                    // Esperar a que se reciba una nueva conexión de cliente
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Cliente conectado.");

                    // Simulación de la conexión del cliente al servidor
                    server.HandleClientConnection(client);
                }
            }
            finally
            {
                // Detener el servidor y liberar recursos al finalizar
                listener.Stop();
            }


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