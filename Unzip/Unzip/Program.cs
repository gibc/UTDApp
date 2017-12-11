using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
//using System.IO.Compression.FileSystem;

namespace Unzip
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting unzip.");
                Assembly assembly = Assembly.GetExecutingAssembly();
                string asnm = assembly.GetName().Name;
                List<string> names = new List<string>(assembly.GetManifestResourceNames());
                Console.WriteLine("Searching for embedded resource.");
                foreach (string nm in names)
                {
                    //if (nm == asnm + ".udtinstaller.zip")
                    if (nm == asnm + ".UtdAppPublish.zip")
                    {
                        Console.WriteLine(string.Format("Found: {0}.", nm));
                        string tempFolder = Path.GetTempPath();
                        tempFolder += "udtsetup";
                        Console.WriteLine(string.Format("Creating setup folder: {0}.", tempFolder));
                        if (Directory.Exists(tempFolder))
                            Directory.Delete(tempFolder, true);
                        Directory.CreateDirectory(tempFolder);
                        string zipFile = tempFolder + "\\zipfile.zip";
                        Console.WriteLine(string.Format("Creating zipfile: {0}.", zipFile));
                        var fileStream = File.Create(zipFile);
                        Stream stream = assembly.GetManifestResourceStream(nm);
                        stream.CopyTo(fileStream);
                        fileStream.Close();
                        Console.WriteLine(string.Format("Extracting zipfile to: {0}.", tempFolder));
                        ZipFile.ExtractToDirectory(zipFile, tempFolder);
                        Console.WriteLine(string.Format("Starting: {0}.", "setup.exe"));
                        Process.Start(tempFolder + "\\setup.exe");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("Setup failed: {0}.", ex.Message));
                Console.ReadLine();
            }
        }
    }
}
