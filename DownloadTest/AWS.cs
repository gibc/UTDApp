using System;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;

namespace s3.amazon.com.docsamples
{
    class UploadObject
    {
        static string bucketName = "*** bucket name ***";
        static string keyName = "*** key name when object is created ***";
        static string filePath = "*** absolute path to a sample file to upload ***";

        static IAmazonS3 client;

        public static void AWStest(string[] args)
        {
            // AWSAccessKeyId = AKIAJTUF5W2B4ZDRZNEQ
            // AWSSecretKey=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
            //sqlncli64.msi
            //SqlLocalDB.msi
            //UtdAppPublish.zip
            //sqlncli32.msi
            using (client = new AmazonS3Client("AKIAIN2WU67HVCNF3UHA",
                "MBo7RJp+MwbH/Fy/NcolilC6Q7svoiLjJp5ZCRNP", Amazon.RegionEndpoint.USWest2))
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "udtdownloads",
                    Key = "UtdAppPublish.zip"
                };

                using (GetObjectResponse response = client.GetObject(request))
                {
                    int count = 500000;
                    int total = 0;
                    int offset = 0;
                    long lenght = response.ContentLength;
                    byte[] data = new byte[500000];
                    while (count > 0)
                    {
                        count = response.ResponseStream.Read(data, offset, 500000 - offset);
                        if(count > 0)
                        {
                            offset += count;
                            total += count;
                            if (offset < 500000  && total < lenght) continue;
                            else
                            {
                                offset = 0;
                                Console.WriteLine("downloaded: {0} bytes", total);
                            }
                        }
                    }
                }

            }


            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void WritingAnObject()
        {
            try
            {
                PutObjectRequest putRequest1 = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    ContentBody = "sample text"
                };

                PutObjectResponse response1 = client.PutObject(putRequest1);

                // 2. Put object-set ContentType and add metadata.
                PutObjectRequest putRequest2 = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = "text/plain"
                };
                putRequest2.Metadata.Add("x-amz-meta-title", "someTitle");

                PutObjectResponse response2 = client.PutObject(putRequest2);

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                        "For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when writing an object"
                        , amazonS3Exception.Message);
                }
            }
        }
    }
}