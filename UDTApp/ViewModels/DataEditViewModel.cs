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
        public DelegateCommand ButtonClickCommand { get; set; }


        public UDTDataGrid(DataEditGrid editGrid, string _parentColName, UDTData _childDef, Action<Guid, DataEditGrid> _buttonClick
            /*, Func<bool> _canExecute*/)
        {
            parentColName = _parentColName;
            childDef = _childDef;
            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);
            buttonClick = _buttonClick;
            //ButtonClickCommand = new DelegateCommand(btnClick, _canExecute);
            ButtonClickCommand = new DelegateCommand(btnClick, canExecutNavBtn);
            dataEditGrid = editGrid;
        }

        public void raiseCanExecuteChanged()
        {
            ButtonClickCommand.RaiseCanExecuteChanged();
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

        private Guid _parentId = Guid.Empty;
        public Guid parentId
        {
            get { return _parentId; }
            set
            {
                SetProperty(ref _parentId, value);

                DataTable childTbl = UDTDataSet.udtDataSet.DataSet.Tables[childDef.Name];
                DataView dv;
                if (childTbl.Rows.Count > 0 && _parentId != Guid.Empty  && parentColName.Length > 0)
                {
                    string filter = string.Format("{0} = '{1}'", parentColName, _parentId);
                    dv = new DataView(childTbl,
                        filter, "", DataViewRowState.CurrentRows);
                }
                else
                {
                    dv = new DataView();
                    dv.Table = childTbl;
                }

                gridData = dv;

                ButtonClickCommand.RaiseCanExecuteChanged();
            }

        }

        private Action<Guid, DataEditGrid> buttonClick { get; set; }

        private string parentColName { get; set; }
        public UDTData childDef { get; set; }

        private void btnClick()
        {
            //buttonClick(childDef);
            buttonClick(parentId, dataEditGrid);
        }

        public bool canDataGridBtnExecute()
        {
            return false;
        }

        private DataEditGrid dataEditGrid;

        private bool canExecutNavBtn()
        {
            if( parentId == Guid.Empty) return false;
            return !UDTDataSet.udtDataSet.HasEditErrors;
        }

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
        public UDTDataTextBox(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
        }

        private string _editText = null;
        [Required(ErrorMessage = "Entry is required.")]
        [CustomValidation(typeof(UDTDataTextBox), "CheckTextEntry")]
        public string editText 
        {
            get { return _editText; }
            set 
            { 
                SetProperty(ref _editText, value);
                validationChanged(HasErrors);
                if(!HasErrors)
                {
                    if (row[colName] as string != value)
                        row[colName] = value;
                }
            }
        }

        private DataRowView _row = null;
        public DataRowView row 
        {
            get { return _row; }
            set 
            {
                SetProperty(ref _row, value);
                if (_row[colName] == DBNull.Value)
                    editText = "";
                else if (udtItem.GetType() == typeof(UDTTxtItem) && _row[colName] != DBNull.Value)
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
        public Action<bool> validationChanged { get; set; }

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
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid decimal.");
                }
            }


            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

    }

    // used by return to parent button
    public class UDTDataButton
    {
        public DelegateCommand ButtonClickCommand { get; set; }

        public UDTDataButton(UDTData item, Action<UDTData> _buttonClick, Func<bool> _canClick, string preFix = "")
        {
            dataItem = item;
            buttonName = preFix + item.Name;
            buttonClick = _buttonClick;
            canClick = _canClick;
            //ButtonClickCommand = new DelegateCommand(btnClick, btnCanClick);
            ButtonClickCommand = new DelegateCommand(btnClick, btnCanClick);
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

        public bool btnCanClick()
        {
            return canClick();
        }
        
    }

    public class DataEditViewModel : ValidatableBindableBase
    {
        public DelegateCommand WindowLoadedCommand { get; set; }

        public DataEditViewModel()
        {
            WindowLoadedCommand = new DelegateCommand(windowLoaded);

        }

        private void windowLoaded()
        {
            // load database from currently loaded schema
            //SelectedItem = null;
            UDTDataSet.udtDataSet.readDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
            UDTDataSet.udtDataSet.IsModified = false;
            //DisplayTable(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
            topDataEditGrid = new DataEditGrid(UDTXml.UDTXmlData.SchemaData[0] as UDTData, navBtnClk);
            createDataGrids(topDataEditGrid, UDTXml.UDTXmlData.SchemaData[0] as UDTData, navBtnClk);
            currentEditGrid = topDataEditGrid;
            currentEditGrid.DisplayTable(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
        }

        private DataEditGrid _currentEditGrid = null;
        public DataEditGrid currentEditGrid
        {
            get { return _currentEditGrid; }
            set { SetProperty(ref _currentEditGrid, value); }
        }

        private DataEditGrid topDataEditGrid = null;

        private void createDataGrids(DataEditGrid parentGrid, UDTData udtData, Action<Guid, DataEditGrid> navButtonClick)
        {
            foreach(UDTData item in udtData.tableData)
            {
                DataEditGrid childGrid = new DataEditGrid(item, navButtonClick);

                parentGrid.childGrids.Add(new UDTDataGrid(childGrid, udtData.Name, item, navButtonClick
                    /*, parentGrid.canClick*/));

                childGrid.parentGrid = new UDTDataGrid(parentGrid, parentGrid.DataViewName, udtData, navButtonClick
                    /*, parentGrid.canClick*/);

                createDataGrids(childGrid, item, navButtonClick);
            }
        }

        private void navBtnClk(Guid parentId, DataEditGrid grid)
        {
            currentEditGrid = grid;
            currentEditGrid.parentId = parentId;
            grid.DisplayTable(grid.currentDataItem);
        }
    }

    //public class DataEditViewModel : ValidatableBindableBase    
    public class DataEditGrid : ValidatableBindableBase
    {
        //public DelegateCommand WindowLoadedCommand { get; set; }
        public DelegateCommand UpdateDatasetCommand { get; set; }
        public DelegateCommand AddRowCommand { get; set; }
        public DelegateCommand DeleteRowCommand { get; set; }
        public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        //public DataEditViewModel()       
        public DataEditGrid(UDTData udtData, Action<Guid, DataEditGrid> navButtonClick)
        {
            //WindowLoadedCommand = new DelegateCommand(windowLoaded);
            UpdateDatasetCommand = new DelegateCommand(updateDataset);
            AddRowCommand = new DelegateCommand(addRow, canAddRow);
            DeleteRowCommand = new DelegateCommand(deleteRow, canDelete);
            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);

            currentDataItem = udtData;

            editBoxes = new List<UDTDataTextBox>();
            foreach (UDTBase item in udtData.columnData)
            {
                editBoxes.Add(new UDTDataTextBox(item.Name, item, editBoxValidationChanged));
            }

            childGrids = new List<UDTDataGrid>();

            //foreach (UDTData item in udtData.tableData)
            //{
            //    childGrids.Add(new UDTDataGrid(this, udtData.Name, item as UDTData, navButtonClick, canClick));
            //}
            //if (childGrids.Count > 0)
            //{
            //    childGridsVisable = Visibility.Visible;
            //    parentMargin = new Thickness(0, 0, 0, 0);
            //}
            //else
            //{
            //    childGridsVisable = Visibility.Collapsed;
            //    parentMargin = new Thickness(0, 0, 30, 0);
            //}

            returnBtn = null;
            if (udtData.parentObj != null)
                returnBtn = new UDTDataButton(udtData.parentObj as UDTData, returnBtnClick, returnBtnCanExe, "<< ");
        }

        DataRowView _selectedItem = null;
        public DataRowView SelectedItem
        {
            get { return _selectedItem;  }
            set 
            { 
                _selectedItem = value;

                editBoxes = createEditBoxes(currentDataItem);

                //if (value == null)  return;
              
                //if(childTables != null)
                //    foreach (UDTDataButton btn in childTables)
                //        btn.raiseCanExecuteChanged();

                if (value == null) return;


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

        //private void windowLoaded()
        //{
        //    // load database from currently loaded schema
        //    SelectedItem = null;
        //    UDTDataSet.udtDataSet.readDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
        //    UDTDataSet.udtDataSet.IsModified = false;
        //    DisplayTable(UDTXml.UDTXmlData.SchemaData[0] as UDTData);

        //}

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
                //    row[colName] = parentIds.Peek();
                    row[colName] = parentId;
                else
                    row[colName] = DBNull.Value;
            }
            row.EndEdit();

            SelectedIndex = gridData.Table.Rows.Count - 1;
            SelectedItem = row;

        }

        private bool canAddRow()
        {
            if (UDTDataSet.udtDataSet.HasEditErrors) return false;
            //if (currentDataItem != null && currentDataItem.ParentColumnNames.Count > 0 && 
            //    parentIds.Peek() == Guid.Empty)
            //    return false;
            return true;
        }

        private void deleteRow()
        {
            SelectedItem.Delete();
            //UDTDataSet.udtDataSet.validationChange(HasErrors);
        }

        private bool canDelete()
        {
            return SelectedItem != null;
        }

        public UDTData currentDataItem = null;

        private DataView getGridData(UDTData dataItem, Guid parentId)
        {
            DataView dv = null;

            if(parentId == Guid.Empty)
            {
                dv = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);
            }
            else if (UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name].Rows.Count > 0)
            {
                DataTable childTbl = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];
                string filter = string.Format("{0} = '{1}'", dataItem.parentObj.Name, parentId);
                dv = new DataView(childTbl,
                    filter, "", DataViewRowState.CurrentRows);
            }
            else
            {
                dv = new DataView();
                dv.Table = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];
            }

            return dv;
        }

        public void DisplayTable(UDTData dataItem)
        {
            //if (SelectedItem != null) parentIds.Push( (Guid)SelectedItem["Id"]);
            //else parentIds.Push(Guid.Empty);

            //if (dataItem.parentObj != null)
            //{
            //    DataTable childTbl = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];
            //    DataView dv;
            //    if (childTbl.Rows.Count > 0)
            //    {
            //        string filter = string.Format("{0} = '{1}'", currentDataItem.Name, SelectedItem["Id"]);
            //        dv = new DataView(childTbl,
            //            filter, "", DataViewRowState.CurrentRows);
            //    }
            //    else
            //    {
            //        dv = new DataView();
            //        dv.Table = childTbl;
            //    }
            //    gridData = dv;
            //}
            //else
            //    gridData = new DataView(UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name]);

            if (SelectedItem != null)
                gridData = getGridData(dataItem, (Guid)SelectedItem["Id"]);
            else
                gridData = getGridData(dataItem, Guid.Empty);

            currentDataItem = dataItem;

            updateChildButtons(dataItem);

            SelectedIndex = 0;
            SelectedItem = null;

        }

        //private Stack<Guid> parentIds = new Stack<Guid>();
        public Guid parentId = Guid.Empty;

        private void updateChildButtons(UDTData dataItem)
        {
            //List<UDTDataButton> childList = new List<UDTDataButton>();
            //List<UDTDataButton> childList = null;
            //childTableDic.TryGetValue(dataItem.Name, out childList);
            //if(childList != null)
            //{
            //    foreach (UDTDataButton btn in childList)
            //        btn.raiseCanExecuteChanged();
            //}
            //else
            //{
            //    childList = new List<UDTDataButton>();
            //    foreach (UDTBase item in dataItem.tableData)
            //    {
            //        //if (item.GetType() == typeof(UDTData))
            //        //{
            //            childList.Add(new UDTDataButton(item as UDTData, childBtnClick, canClick));
            //        //}
            //    }
            //    childTableDic.Add(dataItem.Name, childList);
            //}
            //childTables = childList;

            //returnBtn = null;
            //parentGrid = null;
            if (dataItem.parentObj != null)
            {
                 //returnBtn = new UDTDataButton(dataItem.parentObj as UDTData, returnBtnClick, returnBtnCanExe, "<< ");

                //string parentCol = "";
                //Guid parentId = Guid.Empty;
                //if (dataItem.parentObj.parentObj != null)
                //{ 
                //    parentCol = dataItem.parentObj.parentObj.Name;
                //    Guid temp = parentIds.Pop();
                //    parentId = parentIds.Peek();
                //    parentIds.Push(temp);
                //}
                // put back? parentGrid = new UDTDataGrid(parentCol, dataItem.parentObj as UDTData, returnBtnClick, canClick);

                parentGrid.parentId = parentId;
            }

            //DataViewName = string.Format("Data Group: {0}", dataItem.Name);
            DataViewName = dataItem.Name;

            //editBoxes = createEditBoxes(dataItem);
            //childGrids = createChildGrids(dataItem);
            if (childGrids.Count > 0)
            {
                childGridsVisable = Visibility.Visible;
                parentMargin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                childGridsVisable = Visibility.Collapsed;
                parentMargin = new Thickness(0, 0, 30, 0);
            }

            if (parentGrid != null)
                parentGridVisable = Visibility.Visible;
            else
                parentGridVisable = Visibility.Collapsed;


            //SelectedIndex = 0;

        }

        private UDTDataButton _returnBtn = null;
        public UDTDataButton returnBtn
        {
            get { return _returnBtn; }
            set { SetProperty(ref _returnBtn, value); }
        }

        public bool canClick()
        {
            if( SelectedItem == null ) return false;
            return !UDTDataSet.udtDataSet.HasEditErrors;
        }

        private void childBtnClick(UDTData dataItem)
        {
            DisplayTable(dataItem);
        }

        private void returnBtnClick(UDTData dataItem)
        {
            //parentIds.Pop();

            updateChildButtons(dataItem);
            //gridData = getGridData(dataItem, parentIds.Peek());
            gridData = getGridData(dataItem, parentId);
            currentDataItem = dataItem;
            SelectedIndex = 0;
            SelectedItem = null;

        }

        private bool returnBtnCanExe()
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

        //private Dictionary<string, List<UDTDataButton>> childTableDic = new Dictionary<string, List<UDTDataButton>>();
        //private List<UDTDataButton> _childTables = null;
        //public List<UDTDataButton> childTables
        //{
        //    get { return _childTables; }
        //    set
        //    {
        //        SetProperty(ref _childTables, value);
        //    }
        //}

        private UDTDataGrid _parentGrid = null;
        public UDTDataGrid parentGrid
        {
            get { return _parentGrid; }
            set
            {
                SetProperty(ref _parentGrid, value);
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

        private Thickness _parentMargin = new Thickness(0, 0, 0, 0);
        public Thickness parentMargin
        {
            get { return _parentMargin; }
            set
            {
                SetProperty(ref _parentMargin, value);
            }           
        }

        private Visibility _childGridsVisable = Visibility.Hidden;
        public Visibility childGridsVisable
        {
            get { return _childGridsVisable; }
            set
            {
                SetProperty(ref _childGridsVisable, value);
            }        
        }

        private Visibility _parentGridVisable = Visibility.Collapsed;
        public Visibility parentGridVisable
        {
            get { return _parentGridVisable; }
            set
            {
                SetProperty(ref _parentGridVisable, value);
            }
        }


        List<UDTDataGrid> createChildGrids(UDTData dataItem)
        {
            //List<UDTDataGrid> childGrids = new List<UDTDataGrid>();

            ////foreach (UDTBase item in dataItem.ChildData)
            //foreach (UDTData item in dataItem.tableData)
            //{
            //    //if (item.GetType() == typeof(UDTData))
            //        // put back? childGrids.Add(new UDTDataGrid(dataItem.Name, item as UDTData, childBtnClick, canClick));
            //}

            if (childGrids.Count > 0)
            {
                childGridsVisable = Visibility.Visible;
                parentMargin = new Thickness(0, 0, 0, 0);
            }
            else
            { 
                childGridsVisable = Visibility.Collapsed;
                parentMargin = new Thickness(0, 0, 30, 0);
            }

            return childGrids;
        }

        List<UDTDataTextBox> createEditBoxes(UDTData dataItem)
        {
            List<UDTDataTextBox> boxes = new List<UDTDataTextBox>();

            if (SelectedItem == null) return boxes;
            if (editBoxes.Count > 0) return editBoxes;

            //foreach (UDTBase item in dataItem.ChildData)
            foreach(UDTBase item in dataItem.columnData)
            {
                //if (item.GetType() != typeof(UDTData))
                    boxes.Add(new UDTDataTextBox(item.Name, item, editBoxValidationChanged));
            }

            return boxes;
        }

        private void editBoxValidationChanged(bool hasErrors)
        {
            foreach (UDTDataGrid childGrid in childGrids)
            {
                childGrid.raiseCanExecuteChanged();
            }
            foreach(UDTDataTextBox editBox in editBoxes)
            {
                if(editBox.HasErrors)
                { 
                    UDTDataSet.udtDataSet.validationChange(true);
                    AddRowCommand.RaiseCanExecuteChanged();
                    return;
                }
            }
            UDTDataSet.udtDataSet.validationChange(false);
            AddRowCommand.RaiseCanExecuteChanged();

        }

        private void updateEditBoxes(DataRowView selectedRow)
        {
            
            foreach (UDTDataTextBox editBox in editBoxes)
                editBox.row = selectedRow;
        }

        private void updateChildGrids(DataRowView selectedRow)
        {
            foreach (UDTDataGrid childGrid in childGrids)
            {
                if (selectedRow != null) 
                { 
                    if (selectedRow["Id"] != DBNull.Value)
                    {
                        Guid id = (Guid)selectedRow["Id"];
                        childGrid.parentId = (Guid)selectedRow["Id"];
                    }
                    else
                        childGrid.parentId = Guid.Empty;
                }

                childGrid.raiseCanExecuteChanged();
            }
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
