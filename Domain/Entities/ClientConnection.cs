using Domain.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace Domain.Entities
{
    public class ClientConnection : IPoolableObject
    {
        private string ipAddress;
        private string date;
        private string hour;
        private TcpClient tcpClient;


        public string IpAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        public string Hour
        {
            get { return hour; }
            set { hour = value; }
        }

        public TcpClient TcpClient
        {
            get { return tcpClient; }
            set {
                ipAddress = ((IPEndPoint)value.Client.RemoteEndPoint).Address.ToString();
                date = DateTime.Now.ToShortDateString();
                hour = DateTime.Now.ToShortTimeString();
                tcpClient = value; 
            }  
        }

        public ClientConnection() { 
            this.ipAddress = "";
            this.date = "";  
            this.hour = "";
            this.tcpClient = new TcpClient();
        }


        public string toString()
        {
            return $"{this.ipAddress}-{this.date}-{this.hour}";
        }

        public void Reset()
        {
            this.IpAddress = "";
            this.Date = "";
            this.Hour = "";
            this.tcpClient.Close();
            this.tcpClient.Dispose();
        }

    }
}
