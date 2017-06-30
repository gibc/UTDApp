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
                Sets = ModelBase.LoadRecords<DataSet>(typeof(DataSet));

                //ObservableCollection<DataItem> items = new ObservableCollection<DataItem>();
                //items.Add(new DataItem("ProductName", 1));
                //items.Add(new DataItem("Cost", 1));
                //Sets = new ObservableCollection<DataSet>();
                //ObservableCollection<DataSetRelation> ritems = new ObservableCollection<DataSetRelation>();
                //ritems.Add(new DataSetRelation("Product", "Order"));
                //ritems.Add(new DataSetRelation("Product", "Customer"));
                //Sets.Add(new DataSet("Product", "inventory item", items, ritems));

                //items = new ObservableCollection<DataItem>();
                //items.Add(new DataItem("Terms", 1));
                //items.Add(new DataItem("ShipDate", 1));

                //ritems = new ObservableCollection<DataSetRelation>();
                //ritems.Add(new DataSetRelation("Order", "Product"));
                //ritems.Add(new DataSetRelation("Order", "PayMethod"));
                //Sets.Add(new DataSet("Order", "list of ordered produts", items, ritems));

                //items = new ObservableCollection<DataItem>();
                //items.Add(new DataItem("CustomeName", 1));
                //items.Add(new DataItem("Phone", 1));

                //ritems = new ObservableCollection<DataSetRelation>();
                //ritems.Add(new DataSetRelation("Customer", "Product"));
                //ritems.Add(new DataSetRelation("Customer", "PayMethod"));
                //Sets.Add(new DataSet("Customer", "person who places orders", items, ritems));

                //items = new ObservableCollection<DataItem>();
                //items.Add(new DataItem("Code", 1));
                //items.Add(new DataItem("Key", 1));

                //ritems = new ObservableCollection<DataSetRelation>();
                //ritems.Add(new DataSetRelation("PayMethod", "Product"));
                //ritems.Add(new DataSetRelation("PayMethod", "Customer"));
                //Sets.Add(new DataSet("PayMethod", "person who places orders", items, ritems));

                //ReadDataSetList();

                SelectedIndex = -1;
            }
        }

        static public void SaveDataSetList()
        {
            foreach(DataSet dataSet in Sets)
            {
                dataSet.CreateTable();
                int recId = dataSet.CreateRecord();

                foreach(DataItem item in dataSet.DataItems)
                {
                    item.CreateTable();
                    item.ParentId = recId;
                    item.CreateRecord();
                }

                foreach (DataSetRelation relation in dataSet.DataSetRelations)
                {
                    relation.CreateTable();
                    relation.ParentId = recId;
                    relation.CreateRecord();
                }

            }
        }

        static public ObservableCollection<DataSet> Sets { get; set; }
        static public int SelectedIndex { get; set; }
        //public ObservableCollection<DataSet> Sets { get; set; }
        //public int SelectedIndex { get; set; }
    }
}
