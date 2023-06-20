using Domain.Entities;
using FtpProject.Dto;
using System.Text;

namespace FtpProject.Mappers
{
    public static class ServerMappers
    {
        public static String fromConnectionsToString(List<ClientConnection> connections)
        {
            StringBuilder listConnections = new StringBuilder();


            foreach (ClientConnection connection in connections)
            {
                listConnections.Append(connection.toString() + "\n");
            }

            return listConnections.ToString();
        }

        public static String fromDocumentsToString(List<Document> documents)
        {
            StringBuilder listDocuments = new StringBuilder();


            foreach (Document document in documents)
            {
                listDocuments.Append(document.toString() + "\n");
            }

            return listDocuments.ToString();
        }
    }
}
