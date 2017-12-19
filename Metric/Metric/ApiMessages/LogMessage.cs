using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Metric.ApiMessages
{
    public class ApiLogMessage
    {
        public string AppHost { get; set; }
        public string DateTime { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}