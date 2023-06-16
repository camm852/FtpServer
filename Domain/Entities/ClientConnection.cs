using Domain.Interfaces;
using System.Net.Sockets;

namespace Domain.Entities
{
    public class ClientConnection : TcpClient , IPoolableObject
    {
        private string ipAddress;
        private string date;
        private string hour;


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

        public ClientConnection() : base() { 
            this.ipAddress = "";
            this.date = "";  
            this.hour = "";
        }


        public string toString()
        {
            return $"Client ip: {this.ipAddress} - date: {this.date} - hour: {this.hour}";
        }

        public void Reset()
        {
            this.IpAddress = "";
            this.Date = "";
            this.Hour = "";
        }
    }
}
