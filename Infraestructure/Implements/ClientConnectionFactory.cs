using Domain.Entities;
using Infraestructure.Interfaces;

namespace Infraestructure.Implements
{
    public class ClientConnectionFactory : IObjectFactory<ClientConnection>
    {
        public ClientConnection CreateObject()
        {
            return new ClientConnection();
        }
    }
}
