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

        public UDTDataButton(UDTData item, Action<UDTData> _buttonClick, Func<bool> _canClick, string preFix = "")
        {
            dataItem = item;
            buttonName = preFix + item.Name;
            buttonClick = _buttonClick;
            canClick = _canClick;
            ButtonClickCommand = new DelegateCommand(btnClick, canClick);
        }
        public UDTData dataItem { get; set; }
        public string buttonName { get; set; }
        public Action<UDTData> buttonClick { get; set; }
        public Func<bool> canClick { get; set; }
        private void btnClick()
        {
            buttonClick(dataItem);
        }
        public void raiseCanExecuteChnaged()
        {
            ButtonClickCommand.RaiseCanExecuteChanged();
        }
    }

    public class DataEditViewModel : ValidatableBindableBase
    {
        public DelegateCommand WindowLoadedCommand { get; set; }
        public DelegateCommand UpdateDatasetCommand { get; set; }
        public DelegateCommand AddRowCommand { get; set; }
        public DelegateCommand DeleteRowCommand { get; set; }

        public DataEditViewModel()
        {
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            UpdateDatasetCommand = new DelegateCommand(updateDataset);
            AddRowCommand = new DelegateCommand(addRow);
            DeleteRowCommand = new DelegateCommand(deleteRow);
        }

        DataRowView _selectedItem = null;
        public DataRowView SelectedItem
        {
            get { return _selectedItem;  }
            set 
            { 
                _selectedItem = value;
                foreach (UDTDataButton btn in childTables)
                    btn.raiseCanExecuteChnaged();
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; }
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

        private void addRow()
        {
            //DataRow row = gridData.NewRow();
            //foreach(string colName in currentDataItem.ParentColumnNames)
            //{
            //    row[colName] = 1;
            //}
            //gridData.Rows.Add(row);
        }

        private void deleteRow()
        {

        }

        private UDTData currentDataItem = null;

        private void DisplayTable(UDTData dataItem)
        {

            DataTable tbl = new DataTable();
            if (dataItem.parentObj != null)
            {
                string filter = string.Format("{0} = {1}", currentDataItem.Name, SelectedItem["Id"]);
                DataView dv = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name],
                    filter, "", DataViewRowState.CurrentRows);
                gridData = dv;
            }
            else
                gridData = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);

            currentDataItem = dataItem;

            updateChildButtons(dataItem);
        }

        private void updateChildButtons(UDTData dataItem)
        {
            List<UDTDataButton> childList = new List<UDTDataButton>();
            foreach (UDTBase item in dataItem.ChildData)
            {
                if (item.GetType() == typeof(UDTData))
                {
                    childList.Add(new UDTDataButton(item as UDTData, childBtnClick, canClick));
                }
            }
            returnBtn = null;
            if (dataItem.parentObj != null)
                returnBtn = new UDTDataButton(dataItem.parentObj as UDTData, returnBtnClick, returnBtnClick, "Back to: ");
            childTables = childList;
        }

        private UDTDataButton _returnBtn = null;
        public UDTDataButton returnBtn
        {
            get { return _returnBtn; }
            set { SetProperty(ref _returnBtn, value); }
        }

        private bool canClick()
        {
            return SelectedItem != null;
        }

        private void childBtnClick(UDTData name)
        {
            DisplayTable(name);
        }

        private void returnBtnClick(UDTData dataItem)
        {
            gridData = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);
            currentDataItem = dataItem;
            updateChildButtons(dataItem);
        }

        private bool returnBtnClick()
        {
            return true;
        }

        //private DataTable _gridData = null;
        //public DataTable gridData
        private DataView _gridData = null;
        public DataView gridData
        { 
            get
            {
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
