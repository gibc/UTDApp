using Metric.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metric.DAL
{
    public class LogMessageInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<LogMessageContext>
    {
        protected override void Seed(LogMessageContext context)
        {
            var hosts = new List<AppHosts>
                {
                    new AppHosts{AppHostsID = 1, Name="AppHostA"},
                    new AppHosts{AppHostsID = 2, Name="AppHostB"},
                };

            hosts.ForEach(s => context.AppHosts.Add(s));
            context.SaveChanges();

            var messages = new List<LogMessage>
                {
                    new LogMessage{AppHostsID = 1, UserName ="UserA", Message = "AppHostA starup", DateTime = "1/1/2007" },
                    new LogMessage {AppHostsID = 2, UserName = "UserB", Message = "AppHostB starup", DateTime = "1/1/20017" },
                };

            messages.ForEach(s => context.LogMessage.Add(s));
            context.SaveChanges();
        }
    }
}