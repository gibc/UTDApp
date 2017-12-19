using Metric.ApiMessages;
using Metric.DAL;
using Metric.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Metric.Controllers
{
    public class LogController : ApiController
    {
        // GET: api/Log
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Log/5
        public string Get(int id)
        {
            return "value";
        }

        private LogMessageContext db = new LogMessageContext();

        // POST: api/Log
        public string Post([FromBody]ApiLogMessage value)
        {
            AppHosts host = db.AppHosts.SingleOrDefault(apphost => apphost.Name == value.AppHost);
            if (host == null)
            {
                var hosts = new List<AppHosts>
                {
                    new AppHosts{Name=value.AppHost},
                };
                hosts.ForEach(s => db.AppHosts.Add(s));
                db.SaveChanges();
                host = db.AppHosts.SingleOrDefault(apphost => apphost.Name == value.AppHost);
            }

            var msgs = new List<LogMessage>
            {
                new LogMessage { UserName = value.UserName, AppHostsID = host.AppHostsID, Message = value.Message, DateTime= value.DateTime },
            };
            msgs.ForEach(s => db.LogMessage.Add(s));
            db.SaveChanges();

            return "a ok";
        }

        // PUT: api/Log/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Log/5
        public void Delete(int id)
        {
        }
    }
}
