using System.Text;

namespace FtpProject.Dto
{
    public class RequestDto
    {
        public string type { get; set; }
        public string service { get; set; }

        public string body { get; set; }    
    }
}
