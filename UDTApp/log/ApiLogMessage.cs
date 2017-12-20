using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.Log
{
    // dup code with same class in Metric web app project
    public class ApiLogMessage
    {
        public string AppHost { get; set; }
        public string DateTime { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}
