using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpProject.Dto
{
    public class Document
    {
        public string Name { get; set; }
        public long Size { get; set; }    
        public string Extension { get; set; }



        public string toString()
        {

            return $"{Name}{Extension} {Size}Kb";
        }

    }
}
