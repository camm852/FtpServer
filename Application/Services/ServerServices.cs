using Domain.Entities;
using Infraestructure.Abstracts;
using Infraestructure.Implements;
using Infraestructure.Interfaces;
using System.Net.Sockets;

namespace Application.Services
{
    public class ServerServices
    {


        private AbstractObjectPool<ClientConnection> clientConnectionPool;

        public ServerServices(int maxClients) { 
            IObjectFactory<ClientConnection> factory = new ClientConnectionFactory();
            clientConnectionPool = new ClientConnectionPool(factory, maxClients);
        }

        public void HandleClientConnection(TcpClient client)
        {
            // Obtener una conexión de cliente del pool
            ClientConnection connection = clientConnectionPool.Get();


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
                    while (true)
                    {
                        // Realizar operaciones con la conexión de cliente
                        Console.WriteLine("Handling client connection");
                        Thread.Sleep(2000); // Simulando operaciones con la conexión
                    }
                    
                }
                finally
                {
                    Console.WriteLine("Cliente desconectado");
                    // Devolver la conexión al pool cuando el cliente se desconecte
                    clientConnectionPool.Release(connection);
                }
            });

            thread.Start();
        }
    }
}
