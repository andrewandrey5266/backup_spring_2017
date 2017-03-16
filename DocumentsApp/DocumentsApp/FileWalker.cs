using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
namespace DocumentsApp
{
    public class Info
    {
        public string SourceFoler { get; set; }
        public string DestFolder { get; set; }
        public string ArchiveFolder { get; set; }
        public string[] FileNames { get; set; }        
    }
    public class FileWalker
    {
        public Info Info;
        public Func<string, string, bool> CompareFiles;
        public FileWalker(Info info, Func<string, string, bool> compareFiles)
        {
            this.Info = info;
            this.CompareFiles = compareFiles;
        }

        public void Start()
        {
            Info.DestFolder = Info.DestFolder + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;

            WalkFiles(new DirectoryInfo(Info.SourceFoler));
        }

        private void WalkFiles(DirectoryInfo dir)
        {
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo f in files)
            {
                if (CompareFiles(f.Name, Info.FileNames[0]))
                    SendViaFtp(f);
            }
        }
        private void SendViaFtp(FileInfo fi)
        { 
            // Get the object used to communicate with the server.  
            //////FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://www.contoso.com/test.htm");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://speedtest.tele2.net:21/upload/log.txt");

            request.Method = WebRequestMethods.Ftp.UploadFile;
        //ftp://speedtest.tele2.net/
            // This example assumes the FTP site uses anonymous logon.  
            //request.Credentials = new NetworkCredential("anonymous", "planetazemla1212@mail.ru");
            request.Credentials = new NetworkCredential("demo", "password");

            // Copy the contents of the file to the request stream.  
            StreamReader sourceStream = new StreamReader(fi.FullName);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
            
            // next 
            Task[] tasks = new Task[2];
            tasks[0] = Task.Factory.StartNew((i) => MoveToArchive(i), fi);
            tasks[1] = Task.Factory.StartNew((i) => AddDbRecord(i), fi);

            Console.WriteLine("Moving to archive...");
            Task.WaitAll(tasks);
            Console.WriteLine("Moved to archive");
        }
        private void MoveToArchive(object fileInfo)
        {
            FileInfo fi = (FileInfo)fileInfo;
            File.Copy(fi.FullName, Info.ArchiveFolder + "\\" + fi.Name,true);
            Console.WriteLine("Copy" + DateTime.Now);
        }
        private void AddDbRecord(object fileInfo)
        {
            FileInfo fi = (FileInfo)fileInfo;

            //string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=usersdb;Integrated Security=True; User id=DESKTOP-N9U7COP\SQLEXPRESS";
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
      
            string sqlExpression = "INSERT INTO Archive (Name, Date) VALUEs ('" + fi.Name + "', '" + DateTime.Now.ToString() + "');";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteReader();
            }
            Console.WriteLine("Moved at " + DateTime.Now);

        }
    }
}
