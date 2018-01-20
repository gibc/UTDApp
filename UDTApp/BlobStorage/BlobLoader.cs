using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UDTApp.BlobStorage
{
    public class BlobLoader
    {
 
        static public async void downloadFile(string blobName, string filePath, 
            Action<int> doneCallBack, Action<long, long> downloadCountCallBack,
            Func<bool> isCancled)
        {
            try
            {
                string blobCon = ConfigurationManager.AppSettings["StorageConnectionString"];
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobCon);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference("install-downloads");

                // Retrieve reference to a blob .
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
                //Thread.CurrentThread.IsBackground = true;
                blockBlob.FetchAttributes();
                long size = blockBlob.Properties.Length;
                downloadCountCallBack(0, size);

                int count = 50000;
                long downloadCount = 0;
                byte[] buff = new byte[count];
                using (var fileStream = System.IO.File.OpenWrite(filePath))
                {
                    using (Stream blobStream = blockBlob.OpenRead())
                    {
                        while (count > 0)
                        {
                            count = await blobStream.ReadAsync(buff, 0, 50000);
                            fileStream.Write(buff, 0, count);
                            downloadCount += count;
                            if (isCancled())
                            {
                                break;
                            }
                            downloadCountCallBack(downloadCount, size);
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
                if(isCancled()) doneCallBack(-1);
                else doneCallBack(1);
            }

            return ;
        }
    }
}
