using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;
using UDTApp.SchemaModels;

namespace UDTApp.ListManager
{
    public class ManagedObservableCollection<T> : ObservableCollection<UDTData>
    {
        public void Add(T item)
        {
            // if not present, add to dictionary
            // else get from dictionary and inc ref count
            UDTData dataItem = item as UDTData;
            if(dataItem != null)
            {
                if (TableDictionary.itemDic.ContainsKey(dataItem.objId))
                {
                    TableRef tableRef = null;
                    if (TableDictionary.itemDic.TryGetValue(dataItem.objId, out tableRef))
                    {
                        if(tableRef.refCount == 1)
                        {
                            tableRef.sharedTable = new SharedUDTTable() { sharedTables = tableRef.tables };
                            dataItem.sharedTable = tableRef.sharedTable;
                            tableRef.tables[0].sharedTable = tableRef.sharedTable;
                        }
                        tableRef.refCount++;
                        tableRef.tables.Add(dataItem);
                        dataItem.sharedTable = tableRef.sharedTable;
                        dataItem.columnData = tableRef.tables[0].columnData;
                    }
                }
                else
                {
                    TableRef tableRef = new TableRef { refCount = 1 };
                    tableRef.tables.Add(dataItem);
                    TableDictionary.itemDic.Add(dataItem.objId, tableRef);
                }
            }
            base.Add(dataItem);
        }

        public void Remove(T item)
        {
            // remove item from dictionary only when last reference to the item is removed
            UDTData dataItem = item as UDTData;
            if (dataItem != null)
            {
                if (TableDictionary.itemDic.ContainsKey(dataItem.objId))
                {
                    TableRef tableRef = null;
                    if (TableDictionary.itemDic.TryGetValue(dataItem.objId, out tableRef))
                    {
                        tableRef.tables.Remove(dataItem);
                        tableRef.refCount--;
                        if(tableRef.refCount <= 0)
                        {
                           TableDictionary.itemDic.Remove(dataItem.objId);
                        }
                    }
                }
            }
            base.Remove(dataItem);
        }

    
    }

    public class TableDictionary
    {
        public static Dictionary<Guid, TableRef> itemDic = null;// = new Dictionary<Guid, List<UDTData>>();

    }

    public class TableRef
    {
        public int refCount { get; set; }
        private List<UDTData> _tables = new List<UDTData>();
        public List<UDTData> tables
        {
            get { return _tables; }
        }
        public SharedUDTTable sharedTable { get; set; }
    }
}
