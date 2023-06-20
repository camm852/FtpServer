using Domain.Entities;
using System.Net.Sockets;

namespace Application.Interfaces
{
    public interface IServerServices
    {

        ClientConnection newConnectionClient(TcpClient client);
        bool deleteConnectionClient(ClientConnection client);
        List<ClientConnection> getConnectionClients();
        
    }
}
