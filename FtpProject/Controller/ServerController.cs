using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using FtpProject.Dto;
using Infraestructure.Implements;
using Infraestructure.Interfaces;
using System.IO;
using System.Net.Sockets;
using System.Security.AccessControl;

namespace FtpProject.Controller
{
    public class ServerController
    {
        private IEntityRepository<ClientConnection> _clientRepository;
        private IServerServices _serverServices;
        private string pathDocuments = "";
        public ServerController(int maxConections, string pathDocuments)
        {
            if (!Directory.Exists(pathDocuments))
            {
                // Crear el directorio con permisos adicionales
                Directory.CreateDirectory(pathDocuments);

                // Obtener los permisos actuales del directorio
                DirectoryInfo dirInfo = new DirectoryInfo(pathDocuments);
                DirectorySecurity directorioSeguridad = dirInfo.GetAccessControl();

                // Agregar permisos para el usuario actual
                string usuarioActual = Environment.UserName;
                FileSystemAccessRule accesoUsuarioActual = new FileSystemAccessRule(usuarioActual, FileSystemRights.FullControl, AccessControlType.Allow);
                directorioSeguridad.AddAccessRule(accesoUsuarioActual);

                // Establecer los nuevos permisos en el directorio
                dirInfo.SetAccessControl(directorioSeguridad);

            }


            _clientRepository = new EntityRepository<ClientConnection>();
            _serverServices = new ServerServices(_clientRepository, maxConections);
            this.pathDocuments = pathDocuments;
        }


        public ClientConnection newConnection(TcpClient client)
        {
            return _serverServices.newConnectionClient(client);
        }

        public void deleteConnection(ClientConnection client)
        {
            _serverServices.deleteConnectionClient(client);
        }

        public List<ClientConnection> listConnections()
        {
            return _serverServices.getConnectionClients();
        }

        public List<Document> listDocuments(string ipConnection) {
            List<Document> documents = new List<Document>();

            //string ipLocal = "127.0.0.1";

            try
            {
                string[] files = Directory.GetFiles(pathDocuments);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);


                    string nameDocument = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                    long fileSize = (long)fileInfo.Length / 1024;

                    Document document = new Document()
                    {
                        Name = Path.GetFileNameWithoutExtension(fileInfo.FullName),
                        Extension = fileInfo.Extension,
                        Size = fileSize > 0 ? fileSize : 1,
                    };

                    documents.Add(document);
                }

                return documents;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                return documents;
            }


        }

        public bool saveDocument(byte[] fileBytes, string nameFile, string ipAddress)
        {
            try
            {
                string fileName = nameFile.Substring(0, nameFile.LastIndexOf('.')) + "-" + ipAddress;
                string fileExtension = nameFile.Substring(nameFile.LastIndexOf('.') + 1);
                string filePath = this.pathDocuments + fileName + "." + fileExtension;

                File.WriteAllBytes(filePath, fileBytes);

                Console.WriteLine("Archivo recibido y guardado en: " + filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fallo al guardar el archivo");

                Console.WriteLine(ex);
                return false;
            }
        } 

        public FileInfo findDocument(string documentName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(pathDocuments);

            FileInfo[] documents = directoryInfo.GetFiles(documentName);

            if (documents.Length == 0) return null;
            return documents[0];
        }

    }
}
