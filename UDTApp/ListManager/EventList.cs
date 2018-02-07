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
            // else get from dictionary
            UDTData dataItem = item as UDTData;
            if(dataItem != null)
            {
                if (itemDic.ContainsKey(dataItem.objId))
                {
                    List<UDTData> dataList = null;
                    if (itemDic.TryGetValue(dataItem.objId, out dataList))
                    {
                        if (!dataList.Contains(dataItem))
                            dataList.Add(dataItem);
                        dataItem = dataList[0];
                    }
                }
                else
                {
                    List<UDTData> dataList = new List<UDTData>();
                    dataList.Add(dataItem);
                    itemDic.Add(dataItem.objId, dataList);
                }
            }
            if (deletedTableList.Contains(dataItem.Name))
                deletedTableList.Remove(dataItem.Name);
            base.Add(dataItem);
        }

        public void Remove(T item)
        {
            // remove item only when last reference to the item is removed
            UDTData dataItem = item as UDTData;
            if (dataItem != null)
            {
                if (itemDic.ContainsKey(dataItem.objId))
                {
                    List<UDTData> dataList = null;
                    if (itemDic.TryGetValue(dataItem.objId, out dataList))
                    {
                        if(dataList.Count > 1)
                        {
                            dataList.Remove(dataList[Count - 1]);
                            return;
                        }
                        else
                        {
                            dataList.Clear();
                            deletedTableList.Add(dataItem.Name);
                        }
                    }
                }
            }
            base.Remove(dataItem);
        }

        private static Dictionary<Guid, List<UDTData>> itemDic = new Dictionary<Guid, List<UDTData>>();

        public static List<String> deletedTableList = new List<string>();
    
    }
}
