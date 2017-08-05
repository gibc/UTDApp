using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class UDTDataTextBox : ValidatableBindableBase
    {
        public UDTDataTextBox(string _colName, UDTBase item)
        {
            colName = _colName;
            udtItem = item;
        }

        private string _editText = null;
        [Required]
        public string editText 
        {
            get { return _editText; }
            set 
            { 
                SetProperty(ref _editText, value);
                row[colName] = value;
            }
        }

        private DataRowView _row = null;
        public DataRowView row 
        {
            get { return _row; }
            set 
            {
                SetProperty(ref _row, value);
                if (udtItem.GetType() == typeof(UDTTxtItem))
                    editText = (string)_row[colName];
                else if (udtItem.GetType() == typeof(UDTDateItem))
                    editText = (string)_row[colName].ToString();
            }
        }

        public string colName { get; set; }
        private UDTBase udtItem = null;

    }

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
        private UDTData dataItem { get; set; }
        public string buttonName { get; set; }
        private Action<UDTData> buttonClick { get; set; }
        private Func<bool> canClick { get; set; }
        private void btnClick()
        {
            buttonClick(dataItem);
        }
        public void raiseCanExecuteChanged()
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
            DeleteRowCommand = new DelegateCommand(deleteRow, canDelete);
        }

        DataRowView _selectedItem = null;
        public DataRowView SelectedItem
        {
            get { return _selectedItem;  }
            set 
            { 
                _selectedItem = value;
                foreach (UDTDataButton btn in childTables)
                    btn.raiseCanExecuteChanged();

                if (editBoxes != null && _selectedItem != null) updateEditBoxes(_selectedItem);

                DeleteRowCommand.RaiseCanExecuteChanged();
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set 
            { 
                //_selectedIndex = value;
                SetProperty(ref _selectedIndex, value);
            }
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
            DataRowView row = gridData.AddNew();
            row["Id"] = Guid.NewGuid();
            foreach (string colName in currentDataItem.ParentColumnNames)
            {
                //row[colName] = parentId;
                if (colName == currentDataItem.parentObj.Name)
                    row[colName] = parentIds.Peek();
                else
                    row[colName] = DBNull.Value;
            }
            //gridData.Rows.Add(row);
        }

        private void deleteRow()
        {
            SelectedItem.Delete();
            //gridData.Table.Rows.Remove(SelectedItem.Delete());
        }

        private bool canDelete()
        {
            return SelectedItem != null;
        }

        private UDTData currentDataItem = null;

        private void DisplayTable(UDTData dataItem)
        {
            //if (SelectedItem != null) parentId = (Guid)SelectedItem["Id"];
            if (SelectedItem != null) parentIds.Push( (Guid)SelectedItem["Id"]);

            if (dataItem.parentObj != null)
            {
                DataTable childTbl = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];
                DataView dv;
                if (childTbl.Rows.Count > 0)
                {
                    string filter = string.Format("{0} = '{1}'", currentDataItem.Name, SelectedItem["Id"]);
                    dv = new DataView(childTbl,
                        filter, "", DataViewRowState.CurrentRows);
                }
                else
                {
                    dv = new DataView();
                    dv.Table = childTbl;
                }
                gridData = dv;
            }
            else
                gridData = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);

            currentDataItem = dataItem;

            updateChildButtons(dataItem);

        }

        //private Guid parentId = Guid.Empty;

        private Stack<Guid> parentIds = new Stack<Guid>();

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

            DataViewName = string.Format("Data Group: {0}", dataItem.Name);

            editBoxes = createEditBoxes(dataItem);

            SelectedIndex = 0;

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
            //parentId = Guid.Empty;
            parentIds.Pop();

            gridData = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);
            currentDataItem = dataItem;
            updateChildButtons(dataItem);
        }

        private bool returnBtnClick()
        {
            return true;
        }

        private string _dataViewName = ""; 
        public string DataViewName 
        { 
            get { return _dataViewName; }
            set
            {
                SetProperty(ref _dataViewName, value);
            }
        }
 
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

        private List<UDTDataTextBox> _editBoxes = null;
        public List<UDTDataTextBox> editBoxes 
        {
            get { return _editBoxes; }
            set
            {
                SetProperty(ref _editBoxes, value);
            }
        }

        List<UDTDataTextBox> createEditBoxes(UDTData dataItem)
        {
            List<UDTDataTextBox> editBoxes = new List<UDTDataTextBox>();
            
            foreach(UDTBase item in dataItem.ChildData)
            {
                if(item.GetType() == typeof(UDTTxtItem))
                    editBoxes.Add(new UDTDataTextBox(item.Name, item));
                else if (item.GetType() == typeof(UDTDateItem))
                    editBoxes.Add(new UDTDataTextBox(item.Name, item));

            }

            return editBoxes;
        }

        void updateEditBoxes(DataRowView selectedRow)
        {
            foreach (UDTDataTextBox editBox in editBoxes)
                editBox.row = selectedRow;
        }
    }
}
