using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class UDTDataGrid : ValidatableBindableBase
    {
        public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public UDTDataGrid(string _parentColName, UDTData _childDef)
        {
            parentColName = _parentColName;
            childDef = _childDef;
            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);

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

        private DataRowView _parentRow = null;
        public DataRowView parentRow
        {
            get { return _parentRow; }
            set
            {
                SetProperty(ref _parentRow, value);

                Guid parentId = (Guid)_parentRow["Id"];

                DataTable childTbl = UDTDataSet.udtDataSet.DataSet.Tables[childDef.Name];
                DataView dv;
                if (childTbl.Rows.Count > 0)
                {
                    string filter = string.Format("{0} = '{1}'", parentColName, parentId);
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
        
        }

        private string parentColName { get; set; }
        private UDTData childDef { get; set; }

        public void createColumns(System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(Guid))
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }
    }

    public class UDTDataTextBox : ValidatableBindableBase
    {
        public UDTDataTextBox(string _colName, UDTBase item)
        {
            colName = _colName;
            udtItem = item;
        }

        private string _editText = null;
        [Required]
        [CustomValidation(typeof(UDTDataTextBox), "CheckTextEntry")]
        public string editText 
        {
            get { return _editText; }
            set 
            { 
                SetProperty(ref _editText, value);
                if(!HasErrors)
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
                else if (udtItem.GetType() == typeof(UDTIntItem))
                    editText = (string)_row[colName].ToString();
                else if (udtItem.GetType() == typeof(UDTDecimalItem))
                    editText = (string)_row[colName].ToString();
            }
        }

        public string colName { get; set; }
        public UDTBase udtItem = null;

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckTextEntry(string name, ValidationContext context)
        {
            UDTDataTextBox dataObj = context.ObjectInstance as UDTDataTextBox;
            if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTDateItem))
            {
                DateTime val;
                if(!DateTime.TryParse(name, out val))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid date.");
                }
            }
            else if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTIntItem))
            {
                int val;
                if (!Int32.TryParse(name, out val))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid number.");
                }
            }
            else if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTDecimalItem))
            {
                decimal val;
                if (!Decimal.TryParse(name, out val))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid decimal number.");
                }
            }


            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

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
        public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public DataEditViewModel()
        {
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            UpdateDatasetCommand = new DelegateCommand(updateDataset);
            AddRowCommand = new DelegateCommand(addRow);
            DeleteRowCommand = new DelegateCommand(deleteRow, canDelete);
            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);

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
                if (childGrids != null && _selectedItem != null) updateChildGrids(_selectedItem);

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

            SelectedIndex = 0;


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
            childGrids = createChildGrids(dataItem);

            //SelectedIndex = 0;

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

            updateChildButtons(dataItem);
            gridData = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);
            currentDataItem = dataItem;
            //updateChildButtons(dataItem);
            SelectedIndex = 0;

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

        private List<UDTDataGrid> _childGrids = null;
        public List<UDTDataGrid> childGrids
        {
            get { return _childGrids; }
            set
            {
                SetProperty(ref _childGrids, value);
            }
        }

        List<UDTDataGrid> createChildGrids(UDTData dataItem)
        {
            List<UDTDataGrid> childGrids = new List<UDTDataGrid>();

            foreach (UDTBase item in dataItem.ChildData)
            {
                if (item.GetType() == typeof(UDTData))
                    childGrids.Add(new UDTDataGrid(dataItem.Name, item as UDTData));
            }

            return childGrids;
        }

        List<UDTDataTextBox> createEditBoxes(UDTData dataItem)
        {
            List<UDTDataTextBox> editBoxes = new List<UDTDataTextBox>();
            
            foreach(UDTBase item in dataItem.ChildData)
            {
                if (item.GetType() != typeof(UDTData))
                    editBoxes.Add(new UDTDataTextBox(item.Name, item));
            }

            return editBoxes;
        }

        private void updateEditBoxes(DataRowView selectedRow)
        {
            foreach (UDTDataTextBox editBox in editBoxes)
                editBox.row = selectedRow;
        }

        private void updateChildGrids(DataRowView selectedRow)
        {
            foreach (UDTDataGrid childGrid in childGrids)
                childGrid.parentRow = selectedRow;
        }

        public void createColumns(System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(Guid))
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }
    }
}
