using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metric.Models
{
    public class AppHosts
    {
        public int AppHostsID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<LogMessage> LogMessage { get; set; }
    }
}