using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace AutoUpdater
{
    class Program
    {
        private static readonly string dir = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\GTAOBusinesses\";
        private static readonly string tmp = dir + "GTAOBusinesses.exe";
        static void Main(string[] args)
        {
            if (args.Length < 1)
            { 
                Console.WriteLine("No executable path was given!");
                Console.ReadLine();
                return;
            }

            Thread.Sleep(1000);

            Directory.CreateDirectory(dir);
            WebRequest req = WebRequest.CreateHttp("http://begus.ddns.net/gtaoupdate/GTAOBusinesses.exe");
            WebResponse resp = req.GetResponse();
            Stream r = resp.GetResponseStream();
            FileStream w = File.Open(tmp, FileMode.OpenOrCreate);

            for (long i = 0; i < resp.ContentLength; i++)
                w.WriteByte((byte)r.ReadByte());

            w.Close();

            if (File.Exists(args[0]))
                File.Delete(args[0]);

            File.Move(tmp, args[0]);

            r.Close();
            resp.Close();

            Process.Start(args[0]);
        }
    }
}
