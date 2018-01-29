using s3.amazon.com.docsamples;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DownloadTest
{
    //https://s3-us-west-2.amazonaws.com/udtdownloads/SqlLocalDB.msi
    class Program
    {
        private static long GetURLFileLength(string url, string fileName)
        {
            long length = -1;
            var webRequest = HttpWebRequest.Create(
                string.Format("https://s3-us-west-2.amazonaws.com/udtdownloads/{0}", fileName));
            webRequest.Method = "HEAD";

            using (var webResponse = webRequest.GetResponse())
            {
                length = Int64.Parse(webResponse.Headers.Get("Content-Length"));
            }
            Console.WriteLine("File {0} length = {1}", fileName, length);
            return length;
        }

            //url += string.Format("?fileName={0}", fileName);
            //var webReq = (HttpWebRequest)WebRequest.Create(url);
            //webReq.Method = "GET";
            //try
            //{
            //    WebResponse response = webReq.GetResponse();
            //    long length = 0;

            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        using (StreamReader sr = new StreamReader(stream))
            //        {
            //            length = Int64.Parse(sr.ReadToEnd());
            //        }
            //    }
            //    Console.WriteLine("File {0} length = {1}", fileName, length);

            //}
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("For file '{0}' {1}", fileName, ex.Message);
        //    }
        //}

        private static async Task<bool> GetURLContentsAsync(string url, string fileName)
        {
            // The downloaded resource ends up in the variable named content.
            var content = new MemoryStream();

            byte[] data = new byte[5000000];
            string baseUrl = ConfigurationManager.AppSettings["downloadurl"];
            //url += string.Format("?fileName={0}", fileName);
            string bolbUrl = string.Format("{0}/{1}", baseUrl, fileName);

            // Initialize an HttpWebRequest for the current URL.
            var webReq = (HttpWebRequest)WebRequest.Create(bolbUrl);
            webReq.Method = "GET";
            
            // **Call GetResponseAsync instead of GetResponse, and await the result.
            // GetResponseAsync returns a Task.
            int count = 5000000;
            int total = 0;
            int chunk = 0;
            Console.WriteLine("");
            Console.WriteLine("Loading file {0}", fileName);
            WebResponse response = null;
            try
            {
                response = await webReq.GetResponseAsync();
                // Get the data stream that is associated with the specified URL.
                using (Stream responseStream = response.GetResponseStream())
                {
                    while (count > 0)
                    {
                        count = await responseStream.ReadAsync(data, 0, 5000000);
                        // TBD: write to file stream
                        content.Write(data, 0, count);
                        if (count > 0)
                        {
                            chunk += count;
                            total += count;
                            if (chunk >= 500000)
                            {
                                Console.WriteLine("Chunk size {0} total {1} KB.", chunk, total / 1000);
                                chunk = 0;
                            }
                        }
                    }
                    Console.WriteLine("Chunk size {0} total {1} KB.", chunk, total / 1000);

                }
                return true;
            }
            catch(Exception ex)
            {
                //return response;
                return false;
            }
            // Return the result as a byte array.
            // content.ToArray();
        }

        static string url = "http://localhost:54946/api/Download/FileLength";
        static void Main(string[] args)
        {
            //UploadObject.AWStest(null);
            //return;

            //long length = GetURLFileLength(url, "UtdAppPublish.zip");
            //length = GetURLFileLength(url, "SqlLocalDB.msi");
            //length = GetURLFileLength(url, "sqlncli32.msi");
            //length = GetURLFileLength(url, "sqlncli64.msi");
            //GetURLFileLength(url, "missing");

            //url = "http://localhost:54946/api/Download/GetFile";
            Task<bool> t = GetURLContentsAsync(url, "SqlLocalDB.msi");
            t.Wait();
            if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "UtdAppPublish.zip");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "sqlncli32.msi");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "sqlncli64.msi");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "missing");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");


            //HttpWebResponse r = t.Result as HttpWebResponse;
            //Console.WriteLine("Request status: {0}", r.StatusCode);
            //r.Dispose();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
    }
}
