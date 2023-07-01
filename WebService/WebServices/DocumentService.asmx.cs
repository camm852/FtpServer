using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Xml.Linq;

namespace WebService.WebServices
{
    /// <summary>
    /// Descripción breve de DocumentService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class DocumentService : System.Web.Services.WebService
    {

        [WebMethod]
        public byte[] DownLoadDocument(string documentName)
        {
            string directory = @"C:\FtpDocuments\";

            FileStream fileStream = null;
            fileStream = System.IO.File.Open(directory+documentName, FileMode.Open, FileAccess.Read);
            byte[] bufferDocument = new byte[fileStream.Length];
            fileStream.Read(bufferDocument, 0, (int)fileStream.Length);
            fileStream.Close();
            return bufferDocument;
        }

        [WebMethod]
        public string HelloWord()
        {
            return "Hello word";
        }

    }
}
