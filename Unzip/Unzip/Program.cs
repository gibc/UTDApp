using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using UDTApp.BlobStorage;

namespace Unzip
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting UDTApp insatll.");

                string tempFolder = Path.GetTempPath();
                tempFolder += "udtclickonce";
                Console.WriteLine(string.Format("Creating setup folder: {0}.", tempFolder));
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
                Directory.CreateDirectory(tempFolder);

                Console.WriteLine("Downloading install package.");
                string zipFile = tempFolder + "\\zipfile.zip";
                int exitVal = 0;
                long downloadCount = 1000;
                long length = 0;
                BlobLoader.downloadFile(
                   "UtdAppPublish.zip", zipFile,
                   (rv) => exitVal = rv,
                   (cnt, sz) => { downloadCount = cnt; length = sz; },
                   () => { return false; }
                   );

                StringBuilder spin = new StringBuilder(15);
                spin.Insert(0, new string(' ', 15));
                int progressCount = 0;
                while (exitVal == 0)
                {
                    if (progressCount < 15)
                    {
                        spin[progressCount] = '*';
                        progressCount++;
                    }
                    else
                    {
                        progressCount = 0;
                        spin.Replace('*', ' ');
                    }

                    string dlstr = "         0";
                    if (downloadCount > 1000)
                    { 
                        dlstr = (downloadCount / 1000).ToString("0000000000");
                        dlstr = Regex.Replace(dlstr, @"\b0+", m => "".PadLeft(m.Value.Length, ' '));
                    }
                    
                    string msg = string.Format("\r {0} of {1} KBytes {2}",
                        dlstr, length/1000, spin);
                    Console.Write(msg);
                    Thread.Sleep(500);
                }

                Console.WriteLine(string.Format("Extracting zipfile to: {0}.", tempFolder));
                ZipFile.ExtractToDirectory(zipFile, tempFolder);
                Console.WriteLine(string.Format("Starting: {0}.", "setup.exe"));
                Process.Start(tempFolder + "\\setup.exe");
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("Setup failed: {0}.", ex.Message));
                Console.ReadLine();
            }
        }
    }
}
