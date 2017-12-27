using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Metric.Models
{
    public class LogMessage
    {
        public int LogMessageID { get; set; }
        public int AppHostsID { get; set; }
        public string DateTime { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public virtual AppHosts AppHosts { get; set; }
    }
}