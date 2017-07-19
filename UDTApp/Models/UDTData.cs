using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.Models
{
    public class UDTData : UDTItem
    {
        public UDTData()
        {
            ChildData = new ObservableCollection<UDTData>();
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get { return "Item Group"; } set{} }
        // on write child data insert or update UDTRelation record
        //   where 
        //     parent and child names are parent and child colleciton name
        //     child column name is pareent collection name 
        // on read UDT data set do select * from DataRelation where ParentName = [Name]
        //  for ecah relation
        //     create UDTData collection
        //     select * from [ChildName] where [ChildColumnName] = [Id of this record]
        //        for each record add record to collection
        // To create CRUD UI
        //   find UDTData collectons with ChildData.count = 0;
        //     create display and edit page for each UDTData item
        public ObservableCollection<UDTData> ChildData { get; set; }
        public ObservableCollection<DataItem> DataItems { get; set; }
        // add this if we want children to have more than on parent
        public ObservableCollection<UDTParentColumn> ParentColumnNames { get; set; }
    }

    public class UDTRelation
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }
        public string ChildColumnName { get; set; }
    }

    public class UDTParentColumn
    {
        public string ParentColumnName { get; set; }
    }

    public interface UDTItem
    {
        string Name { get; set; }
        string Type { get; set; }
        string TypeName { get; set; }
    }

    public class UDTTxtItem : UDTItem
    {
        public UDTTxtItem(string name)
        { 
            Type = "[varchar](255) NULL ";
            TypeName = "Text Item";
            Name = name;
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
    }


    public class UDTIntItem : UDTItem
    {
        public UDTIntItem(string name)
        {
            Type = "[int] NULL ";
            TypeName = "Number Item";
            Name = name;
        }        
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }

    }
    public class UDTDecimalItem : UDTItem
    {
        public UDTDecimalItem(string name)
        {
            Type = "[decimal](10, 5) NULL ";
            TypeName = "Real Number Item";
            Name = name;
        }                
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }

    }
    public class UDTDateItem : UDTItem
    {
        public UDTDateItem(string name)
        {
            Type = "[datetime] NULL";
            TypeName = "DateTime Item";
            Name = name;
        }                
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
    }

    public class UDTItemList
    {
        private static Collection<UDTItem> _itemList = null;
        public static Collection<UDTItem> ItemList { 
            get
            {
                if (_itemList == null)
                {
                    _itemList = new Collection<UDTItem>();
                    _itemList.Add(new UDTData());
                    _itemList.Add(new UDTTxtItem(""));
                    _itemList.Add(new UDTIntItem(""));
                    _itemList.Add(new UDTDecimalItem(""));
                    _itemList.Add(new UDTDateItem(""));
                }
                return _itemList;
            }
        }
    }
}
