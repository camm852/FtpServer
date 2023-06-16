using Infraestructure.Interfaces;

namespace Infraestructure.Implements
{
    public class ClientFtpRepository : IClientFtpRepository
    {

        //private List<ClientFTP> clientsFtp; 


        //public ClientFtpRepository()
        //{
        //    clientsFtp = new List<ClientFTP>();
        //}

        //public bool newFtpClient(ClientFTP client)
        //{
        //    try
        //    {
        //        this.clientsFtp.Add(client);
        //        return true;
        //    }catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //public bool deleteFtpClient(ClientFTP client)
        //{
        //    try
        //    {
        //        if(getFtpClient(client) != null)
        //        {
        //            return this.clientsFtp.Remove(client);
        //        }
        //        return false;
        //    }catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //public ClientFTP getFtpClient(ClientFTP client)
        //{
        //    try
        //    {
        //        return this.clientsFtp.FirstOrDefault(client => client.IpAddress.Equals(client.IpAddress));
        //    }catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        //public List<ClientFTP> getFtpClients()
        //{
        //    return this.clientsFtp;
        //}

     
    }
}
