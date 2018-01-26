using System;
using System.IO;
using System.Net;
using System.Windows;

namespace UDTApp.BlobStorage
{
    public class BlobLoader
    {
        static private string baseUrl = "http://metricresearch.org/api/Download";
        //static private string baseUrl = "http://localhost:54946/api/Download";

        static public async void downloadFile(string blobName, string filePath, 
            Action<int> doneCallBack, Action<long, long> downloadCountCallBack,
            Func<bool> isCancled)
        {
            long fileLength = -1;
            try
            {
                fileLength = GetFileLength(blobName);
                if (fileLength < 0) return;               
                downloadCountCallBack(0, fileLength);

                string url = string.Format("{0}/GetFile?fileName={1}", baseUrl, blobName);
                // Initialize an HttpWebRequest for the current URL.
                var webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Method = "GET";

                int count = 5000000;
                byte[] data = new byte[count];
                int total = 0;
                int chunk = 0;

                WebResponse response = await webReq.GetResponseAsync();
                // Get the data stream that is associated with the specified URL.
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (var fileStream = System.IO.File.OpenWrite(filePath))
                    {
                        while (count > 0)
                        {
                            if (isCancled()) break;

                            count = await responseStream.ReadAsync(data, 0, 5000000);
                            fileStream.Write(data, 0, count);
                            if (count > 0)
                            {
                                chunk += count;
                                total += count;
                                if (chunk >= 500000)
                                {
                                    //Console.WriteLine("Chunk size {0} total {1} KB.", chunk, total / 1000);
                                    downloadCountCallBack(total, fileLength);
                                    chunk = 0;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Insatll package download failed: {0}", ex.Message),
                    "Download Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                doneCallBack(-1);
                return ;
            }
            finally
            {
                if(isCancled() || fileLength < 0) doneCallBack(-1);
                else doneCallBack(1);
            }
        }

        private static long GetFileLength(string fileName)
        {
            //string url = "http://localhost:54946/api/Download/FileLength";
            string url = string.Format("{0}/FileLength?fileName={1}", baseUrl, fileName);
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.Method = "GET";
            long length = -1;
            try
            {
                WebResponse response = webReq.GetResponse();
                
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        length = Int64.Parse(sr.ReadToEnd());
                    }
                }
                //Console.WriteLine("File {0} length = {1}", fileName, length);
                return length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("BlobLoader::GetFileLength failed: {0}", ex.Message));
                return -1;
            }
        }
    }
}
