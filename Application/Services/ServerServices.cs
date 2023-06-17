using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Abstracts;
using Infraestructure.Implements;
using Infraestructure.Interfaces;
using System.Net.Sockets;

namespace Application.Services
{
    public class ServerServices : IServerServices
    {

        private readonly IEntityRepository<ClientConnection> _entityRepository;
        private readonly AbstractObjectPool<ClientConnection> clientConnectionPool;

        public ServerServices(IEntityRepository<ClientConnection> entityRepository, int maxClients)
        {
            IObjectFactory<ClientConnection> factory = new ClientConnectionFactory();
            clientConnectionPool = new ClientConnectionPool(factory, maxClients);
            _entityRepository = entityRepository;
            Console.WriteLine("Hola");
        }

        public bool deleteConnectionClient(ClientConnection client)
        {
            if(!_entityRepository.deleteEntity(client)) return false;

            clientConnectionPool.Release(client);
            return true;
        }


        public ClientConnection? newConnectionClient(TcpClient client)
        {

            ClientConnection connection = clientConnectionPool.Get();


            if (connection == null) return null;

            connection.TcpClient = client;

            if (!_entityRepository.newEntity(connection))
            {
                clientConnectionPool.Release(connection);
                return null;
            }


            return connection;
        }

        public List<ClientConnection> getConnectionClients()
        {
            return _entityRepository.getEntities();
        }
    }
}
