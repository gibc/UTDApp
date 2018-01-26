using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace Metric.Controllers
{
    public class DownloadController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        [ActionName("GetFile")]
        [Route("api/Download/GetFile")]
        public HttpResponseMessage GetFile(string fileName)
        {
            HttpResponseMessage response = Request.CreateResponse();

            try
            {
                response.Content = new PushStreamContent((stream, content, context) =>
                {
                    using (AmazonS3Client client = new AmazonS3Client("AKIAIN2WU67HVCNF3UHA",
                           "MBo7RJp+MwbH/Fy/NcolilC6Q7svoiLjJp5ZCRNP", Amazon.RegionEndpoint.USWest2))
                    {
                        GetObjectRequest request = new GetObjectRequest
                        {
                            BucketName = "udtdownloads",
                            Key = fileName
                        };
                        using (GetObjectResponse s3response = client.GetObject(request))
                        {
                            s3response.ResponseStream.CopyTo(stream, 5000000);
                        }
                    }
                    stream.Close();
                });

                return response;
            }
            catch(AmazonS3Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(ex.Message),
                    ReasonPhrase = ex.ErrorCode
                };
                throw new HttpResponseException(resp);
            }
        }

        // GET api/<controller>/5
        [HttpGet]
        [ActionName("FileLength")]
        [Route("api/Download/FileLength")]
        public long FileLength(string fileName)
        {
            using (AmazonS3Client client = new AmazonS3Client("AKIAIN2WU67HVCNF3UHA",
                   "MBo7RJp+MwbH/Fy/NcolilC6Q7svoiLjJp5ZCRNP", Amazon.RegionEndpoint.USWest2))
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = "udtdownloads",
                    Key = fileName
                };
                try
                {
                    using (GetObjectResponse s3response = client.GetObject(request))
                    {
                        return s3response.ContentLength;
                    }
                }
                catch(AmazonS3Exception ex)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(ex.Message),
                        ReasonPhrase = ex.ErrorCode
                    };
                    throw new HttpResponseException(resp);
                }
            }
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}