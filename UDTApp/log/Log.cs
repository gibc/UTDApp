using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Web.Script.Serialization;

namespace UDTApp.Log
{
    public class Log
    {
        public static void LogMessage(string msg)
        {
            //string url = "http://localhost:54946/api/log/post";
            string url = "http://metricresearch.org/api/log/post";
            using (WebClient client = new WebClient())
            {
                ApiLogMessage apiMsg = new ApiLogMessage();
                apiMsg.AppHost = Environment.MachineName;
                apiMsg.UserName = Environment.UserName;
                apiMsg.DateTime = DateTime.Now.ToString();
                apiMsg.Message = msg;

                // Set the header so it knows we are sending JSON.
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Serialise the data we are sending in to JSON
                JavaScriptSerializer jss = new JavaScriptSerializer();
                string serialisedData = jss.Serialize(apiMsg);
                // Make the request
                string response = client.UploadString(url, serialisedData);
                // Deserialise the response
                string result = (string)jss.DeserializeObject(response);
            }

        }
    }
}
