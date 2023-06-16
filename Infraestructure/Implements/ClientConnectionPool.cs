using Domain.Entities;
using Infraestructure.Abstracts;
using Infraestructure.Interfaces;

namespace Infraestructure.Implements
{
    public class ClientConnectionPool : AbstractObjectPool<ClientConnection>
    {
        public ClientConnectionPool(IObjectFactory<ClientConnection> factory, int maxConnections) : base(factory, maxConnections)
        {
        }
    }
}
