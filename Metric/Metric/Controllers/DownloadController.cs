using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
                string baseUrl = ConfigurationManager.AppSettings["downloadurl"];
                string bolbUrl = string.Format("{0}/{1}", baseUrl, fileName);

                response.Content = new PushStreamContent((stream, content, context) =>
                {
                    var webReq = (HttpWebRequest)WebRequest.Create(bolbUrl);
                    webReq.Method = "GET";
                    WebResponse webResponse = webReq.GetResponse();
                    byte[] data = new byte[500000];
                    int count = data.Length;
                    Stream responseStream = webResponse.GetResponseStream();
                    while(count > 0)
                    {
                        count = responseStream.Read(data, 0, data.Length);
                        stream.Write(data, 0, count);
                        stream.Flush();
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
            string baseUrl = ConfigurationManager.AppSettings["downloadurl"];
            string bolbUrl = string.Format("{0}/{1}", baseUrl, fileName);

            long length = -1;
            var webRequest = HttpWebRequest.Create(bolbUrl);
            webRequest.Method = "HEAD";
            try
            {
                using (var webResponse = webRequest.GetResponse())
                {
                    length = Int64.Parse(webResponse.Headers.Get("Content-Length"));
                }
            }
            catch
            {
                return -1;
            }
            return length;
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