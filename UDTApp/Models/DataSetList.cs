using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.Models
{
    public class DataSetList
    {
        public DataSetList()
        {
            if(Sets == null)
            { 
                ObservableCollection<DataItem> items = new ObservableCollection<DataItem>();
                items.Add(new DataItem("ProductName", 1));
                items.Add(new DataItem("Cost", 1));
                Sets = new ObservableCollection<DataSet>();
                Sets.Add(new DataSet("Product", "inventory item", items));
                items = new ObservableCollection<DataItem>();
                items.Add(new DataItem("Terms", 1));
                items.Add(new DataItem("ShipDate", 1));
                Sets.Add(new DataSet("Order", "list of ordered produts", items));
                items = new ObservableCollection<DataItem>();
                items.Add(new DataItem("CustomeName", 1));
                items.Add(new DataItem("Phone", 1));
                Sets.Add(new DataSet("Customer", "person who places orders", items));

                SelectedIndex = -1;
            }
        }

        static public ObservableCollection<DataSet> Sets { get; set; }
        static public int SelectedIndex { get; set; }
        //public ObservableCollection<DataSet> Sets { get; set; }
        //public int SelectedIndex { get; set; }
    }
}
