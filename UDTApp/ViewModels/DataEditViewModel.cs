using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class UDTDataButton
    {
        public DelegateCommand ButtonClickCommand { get; set; }

        public UDTDataButton(UDTData item, Action<UDTData> _buttonClick)
        {
            dataItem = item;
            buttonClick = _buttonClick;
            ButtonClickCommand = new DelegateCommand(btnClick);
        }
        public UDTData dataItem { get; set; }
        public Action<UDTData> buttonClick { get; set; }
        private void btnClick()
        {
            buttonClick(dataItem);
        }
    }

    public class DataEditViewModel : ValidatableBindableBase
    {
        public DelegateCommand WindowLoadedCommand { get; set; }
        public DelegateCommand UpdateDatasetCommand { get; set; }

        public DataEditViewModel()
        {
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            UpdateDatasetCommand = new DelegateCommand(updateDataset);
        }

        private void windowLoaded()
        {
            // load database from currently loaded schema
            UDTDataSet.udtDataSet.readDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
            DisplayTable(UDTXml.UDTXmlData.SchemaData[0] as UDTData);

        }

        private void updateDataset()
        {
            UDTDataSet.udtDataSet.saveDataset();
        }

        private void DisplayTable(UDTData dataItem)
        {
            gridData = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];

            List<UDTDataButton> childList = new List<UDTDataButton>();
            foreach (UDTBase item in dataItem.ChildData)
            {
                if (item.GetType() == typeof(UDTData))
                {
                    childList.Add(new UDTDataButton(item as UDTData, childBtnClick));
                }
            }
            if(dataItem.parentObj != null)
                childList.Add(new UDTDataButton(dataItem.parentObj as UDTData, childBtnClick));
            childTables = childList;
        }

        private void childBtnClick(UDTData name)
        {
            DisplayTable(name);
        }

        private DataTable _gridData = null;
        public DataTable gridData
        { 
            get
            {
                //if(UDTDataSet.udtDataSet.DataSet != null)
                //    return UDTDataSet.udtDataSet.DataSet.Tables["UDTMaster"];
                return _gridData;
            }
            set
            {
                SetProperty(ref _gridData, value); 
            }
        }

        private List<UDTDataButton> _childTables = null;
        public List<UDTDataButton> childTables
        {
            get { return _childTables; }
            set
            {
                SetProperty(ref _childTables, value);
            }
        }
    }
}
