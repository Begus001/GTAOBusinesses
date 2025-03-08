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
            string exepath;
            if (args.Length < 1)
            {
                exepath = @".\GTAOBusinesses.exe";
            }
            else
            {
                exepath = args[0];
            }

            Thread.Sleep(200);

            Directory.CreateDirectory(dir);
            WebRequest req = WebRequest.CreateHttp("https://goisser.net/gtaoupdate/GTAOBusinesses.exe");
            WebResponse resp = req.GetResponse();
            Stream r = resp.GetResponseStream();
            FileStream w = File.Open(tmp, FileMode.OpenOrCreate);

            for (long i = 0; i < resp.ContentLength; i++)
                w.WriteByte((byte)r.ReadByte());

            w.Close();

            if (File.Exists(exepath))
                File.Delete(exepath);

            Thread.Sleep(500);

            File.Move(tmp, exepath);

            r.Close();
            resp.Close();

            Process.Start(exepath);
        }
    }
}
