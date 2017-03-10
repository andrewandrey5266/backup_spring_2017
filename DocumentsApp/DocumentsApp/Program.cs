using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Hangfire;
using System.Configuration;
namespace DocumentsApp
{
    class Program
    { 
        static void Main(string[] args)
        {
            //GlobalConfiguration.Configuration.UseSqlServerStorage("Database");

            //using (var server = new BackgroundJobServer())
            //{
            //    Console.WriteLine("Hangfire Server started. Press any key to exit...");

            //    //BackgroundJob.Schedule(() => DoJob1() , TimeSpan.FromSeconds(1));
            //    RecurringJob.AddOrUpdate(() => DoJob1(), "0/3 * * * *");
            //    Console.ReadKey();
            //}
            DoJob1();

            Console.ReadKey();

        }
        public static void DoJob1()
        {      
            Info info = new Info
            {
                //S:\Statements_2.0\_WEBUPLOAD\_ClientPortal
                SourceFoler = ConfigurationManager.AppSettings["SourceFoler"],
                //C:\FTP Contents
                DestFolder = ConfigurationManager.AppSettings["DestFolder"],
                FileNames = new string[] { ".txt" },
                //S:\Statements_2.0\_WEBUPLOAD\_ClientPortal\Archive
                ArchiveFolder = ConfigurationManager.AppSettings["ArchiveFolder"]
            };
            var walker = new FileWalker(info, (a, b) => a.Contains(b));
            walker.Start();

        }
        public static void DoJob()

        {
            using (StreamWriter writer = new StreamWriter(@"E:\GAMES\Postal 2 Complete\log.txt",true))
            {
                writer.WriteLine(DateTime.Now);                
            }
        }
    }
   
}
