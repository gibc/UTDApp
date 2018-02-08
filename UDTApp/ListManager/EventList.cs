using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

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
                        tableRef.refCount++;
                        dataItem = tableRef.item;
                    }
                }
                else
                {
                    TableRef tableRef = new TableRef { refCount = 1, item = dataItem };
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
        public UDTData item { get; set; }
    }
}
