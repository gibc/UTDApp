using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.SchemaModels
{
    public class SharedUDTTable
    {
        public string Name
        {
            set
            {
                foreach(UDTData table in sharedTables)
                {
                    if (table.Name != value)
                    {
                        table.Name = value;
                    }
                }
            }
        }

        public List<UDTData> sharedTables
        { get; set; }
    }
}
