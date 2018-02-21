using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UDTApp.EditControlViewModels;
using UDTApp.Models;
using UDTApp.SchemaModels;
using UDTApp.ViewModels.DataEntryControls;
using UDTAppControlLibrary.Controls;

namespace UDTApp.ViewModels
{
    public class UDTDataTab  : ValidatableBindableBase
    {
        public UDTDataTab(List<DataEditGrid> _childGrids)
        {
            tabPages = new ObservableCollection<UDTDataTabPage>();
            foreach(DataEditGrid grid in _childGrids)
            {
                tabPages.Add(new UDTDataTabPage(grid.currentDataItem.Name, grid));
            };
        }

        private ObservableCollection<UDTDataTabPage> _tabPages = null;
        public ObservableCollection<UDTDataTabPage> tabPages 
        {
            get { return _tabPages; }
            set { SetProperty(ref _tabPages, value); }
        }
    }
    public class UDTDataTabPage : ValidatableBindableBase
    {
        public UDTDataTabPage(string title, DataEditGrid grid)
        {
            pageTitle = title;
            pageContent = grid;
        }
        private string _pageTitle = null;
        public string pageTitle
        {
            get { return _pageTitle; }
            set { SetProperty(ref _pageTitle, value); }
        }
        private DataEditGrid _pageContent = null;
        public DataEditGrid pageContent
        {
            get { return _pageContent; }
            set { SetProperty(ref _pageContent, value); }
        }
    }
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

                // also set epanded grid views parent id
                dataEditGrid.parentId = value;

                DataTable childTbl = DBModel.Service.DataSet.Tables[childDef.Name];
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
            if( childDef.parentObj != null && parentId == Guid.Empty) return false;
            return !DBModel.Service.HasEditErrors;
        }

        public void createColumns(System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(Guid))
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }
    }

    public class UDTDataBoxBase : ValidatableBindableBase
    {
        public UDTDataBoxBase()
        {

        }

        public string colName { get; set; }
        public UDTBase udtItem = null;
        public Action<bool> validationChanged { get; set; }
        
        private DataRowView _row = null;
        public DataRowView row
        {
            get { return _row; }
            set
            {
                SetProperty(ref _row, value);
                setColumn();
            }
        }

        protected dynamic editProps = null;

        virtual protected void setColumn() { }
    }

    public class UDTDataDateBox : UDTDataBoxBase
    {
        public UDTDataDateBox(string _colName, UDTBase item, Action<bool> _validationChanged) 
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
            editProps = item.editProps;
        }

        private DateTime? _date = null;
        public DateTime? dateEntry
        {
            get { return _date; }
            set 
            { 
                SetProperty(ref _date, value);
                dateChanged();
            }
        }

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                UDTDateEditProps dateEditProps = editProps as UDTDateEditProps;
                dateEntry = TimeDefault.DateTimeValue(dateEditProps.defaultDate);
                //if (dateEditProps.defaultDate == DateTimeDefault.None)
                //{
                //    dateEntry = null;
                //}
                //else if (dateEditProps.defaultDate == DateTimeDefault.CurrentDay)
                //{
                //    dateEntry = DateTime.Now;
                //}
                //else if (dateEditProps.defaultDate == DateTimeDefault.CurrentWeek)
                //{
                //    DateTime now = DateTime.Now;
                //    int pastSunday = 6 - (int)now.DayOfWeek;
                //    dateEntry = new DateTime(now.Year, now.Month, now.Day - pastSunday);
                //}
                //else if (dateEditProps.defaultDate == DateTimeDefault.CurrentMonth)
                //{
                //    DateTime now = DateTime.Now;
                //    dateEntry = new DateTime(now.Year, now.Month, 1);
                //}
                //else if (dateEditProps.defaultDate == DateTimeDefault.CurrentYear)
                //{
                //    DateTime now = DateTime.Now;
                //    dateEntry = new DateTime(now.Year, 1, 1);
                //}
            }
            else if (udtItem.GetType() == typeof(UDTDateItem))
            {
                DateTime dateVal = (DateTime)row[colName];
                dateEntry = dateVal;
            }
        }

        private void dateChanged()
        {
            if (editProps.required && dateEntry == null)
            {
                List<string> errLst = new List<string>();
                errLst.Add("Date/Time entry is required.");
                SetErrors(() => this.dateEntry, errLst);
            }
            else
            {
                SetErrors(() => this.dateEntry, new List<string>());
            }
            validationChanged(HasErrors);
            if (!HasErrors)
            {
                UDTDateEditProps dateEditProps = editProps as UDTDateEditProps;
                if (dateEntry == null)
                {
                    row[colName] = DBNull.Value;
                }
                else
                {
                    if(!dateEditProps.dateRangeNotUsed)
                    {
                        if(dateEntry < dateEditProps.minDate)
                        {
                            dateEntry = dateEditProps.minDate;
                        }
                        else if (dateEntry > dateEditProps.maxDate)
                        {
                            dateEntry = dateEditProps.maxDate;
                        }
                    }

                    if (row[colName] == DBNull.Value)
                    {
                        row[colName] = dateEntry;
                    }
                    else 
                    {
                        DateTime currentVal = (DateTime)row[colName];
                        if (currentVal != dateEntry)
                            row[colName] = dateEntry;
                    }
                }
            }
        }
    }

    public class UDTDataDecimalBox : UDTDataNumberBox
    {
        public UDTDataDecimalBox(string _colName, UDTBase item, Action<bool> _validationChanged) :
            base(_colName, item, _validationChanged) { }
    }

    public class UDTDataNumberBox : UDTDataBoxBase
    {
        public UDTDataNumberBox(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
            //editProps = item.editProps as UDTIntEditProps;
            editProps = item.editProps;
            decimal maxNum = Decimal.MaxValue;
            decimal minNum = Decimal.MinValue;
            if (editProps.maxPicker.number != null)
                maxNum = (Decimal)editProps.maxPicker.number;
            if (editProps.minPicker.number != null)
                minNum = (Decimal)editProps.minPicker.number;
            NumberPickerType pickType = NumberPickerType.Integer;
            if (item.TypeName == UDTTypeName.Real)
                pickType = NumberPickerType.Decimal;
            numberEntryBox = new UDTNumberEntry
                (item.Name,
                maxNum,
                minNum,
                pickType,
                numberChanged);
        }

        private UDTNumberEntry _numberEntryBox = null;
        public UDTNumberEntry numberEntryBox 
        {
            get { return _numberEntryBox; }
            set { SetProperty(ref _numberEntryBox, value); }
        }

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                if (editProps.defaultPicker.number != null)
                {
                    _numberEntryBox.number = editProps.defaultPicker.number;
                }
                else _numberEntryBox.txtNumber = "";
            }
            else if (udtItem.GetType() == typeof(UDTIntItem))
            { 
                int intVal = (int)row[colName];
                _numberEntryBox.number = intVal;
            }
        }

        //private UDTIntEditProps editProps = null;
        //private dynamic editProps = null;

        private void numberChanged(decimal? number)
        {
            if (editProps.required && _numberEntryBox.number == null)
            {
                List<string> errLst = new List<string>();
                errLst.Add("Number entry is required.");
                SetErrors(() => this.numberEntryBox, errLst);
            }
            else
            {
                SetErrors(() => this.numberEntryBox, new List<string>());
            }
            validationChanged(HasErrors);
            if (!HasErrors)
            {
                if(row[colName] == DBNull.Value)
                {
                    row[colName] = number;
                }
                else
                {
                    if(udtItem.TypeName == UDTTypeName.Number)
                    {
                        int currentVal = (int)row[colName];
                        if(currentVal != (int)number)
                            row[colName] = number;
                    }
                    else if (udtItem.TypeName == UDTTypeName.Real)
                    {
                        decimal currentVal = (decimal)row[colName];
                        if (currentVal != (decimal)number)
                            row[colName] = number;
                    }
                }

            }
        }
    }

    public class UDTDataTextBox : UDTDataBoxBase//ValidatableBindableBase
    {
        public UDTDataTextBox(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
        }

        private string _editText = null;
        [CustomValidation(typeof(UDTDataTextBox), "CheckTextEntry")]
        public string editText 
        {
            get { return _editText; }
            set 
            {
                if (_editText == null)
                {
                    getDefaultValue(udtItem, ref value);
                }
                SetProperty(ref _editText, value);
                validationChanged(HasErrors);
                if(!HasErrors)
                {
                    if (row[colName] as string != value)
                        row[colName] = value;
                }
            }
        }

        private void getDefaultValue(UDTBase udtItem, ref string value)
        {
            if (udtItem.TypeName == UDTTypeName.Text)
            {
                UDTTextEditProps editProps = udtItem.editProps as UDTTextEditProps;
                if (editProps.defaultText.Length > 0)
                    value = editProps.defaultText;
            }
        }

        //private DataRowView _row = null;
        //public DataRowView row 
        //{
        //    get { return _row; }
        //    set 
        //    {
        //        SetProperty(ref _row, value);
        //        //if (_row[colName] == DBNull.Value)
        //        //    editText = null;
        //        //else if (udtItem.GetType() == typeof(UDTTxtItem) && _row[colName] != DBNull.Value)
        //        //    editText = (string)_row[colName];
        //        //else if (udtItem.GetType() == typeof(UDTDateItem))
        //        //    editText = (string)_row[colName].ToString();
        //        //else if (udtItem.GetType() == typeof(UDTIntItem))
        //        //    editText = (string)_row[colName].ToString();
        //        //else if (udtItem.GetType() == typeof(UDTDecimalItem))
        //        //    editText = (string)_row[colName].ToString();
        //    }
        //}

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
                editText = null;
            else if (udtItem.GetType() == typeof(UDTTxtItem) && row[colName] != DBNull.Value)
                editText = (string)row[colName];
            else if (udtItem.GetType() == typeof(UDTDateItem))
                editText = (string)row[colName].ToString();
            else if (udtItem.GetType() == typeof(UDTIntItem))
                editText = (string)row[colName].ToString();
            else if (udtItem.GetType() == typeof(UDTDecimalItem))
                editText = (string)row[colName].ToString();
        }

        //public string colName { get; set; }
        //public UDTBase udtItem = null;
        //public Action<bool> validationChanged { get; set; }

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckTextEntry(string name, ValidationContext context)
        {
            UDTDataTextBox dataObj = context.ObjectInstance as UDTDataTextBox;
            if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTDateItem))
            {
                UDTDateEditProps editProps = dataObj.udtItem.editProps as UDTDateEditProps;
                if (editProps.required && name.Length <= 0)
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Date entry is required.");
                }
                else if (!editProps.required && name.Length <= 0)
                {
                    return System.ComponentModel.DataAnnotations.ValidationResult.Success;
                } 
                DateTime val;
                if(!DateTime.TryParse(name, out val))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid date.");
                }
            }
            else if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTIntItem))
            {
                UDTIntEditProps editProps = dataObj.udtItem.editProps as UDTIntEditProps;
                if (editProps.required && name.Length <= 0)
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Number entry is required.");
                }
                else if (!editProps.required && name.Length <= 0)
                {
                    return System.ComponentModel.DataAnnotations.ValidationResult.Success;
                } 
                int val;
                if (!Int32.TryParse(name, out val))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid number.");
                }
            }
            else if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTDecimalItem))
            {
                UDTDecimalEditProps editProps = dataObj.udtItem.editProps as UDTDecimalEditProps;
                if (editProps.required && name.Length <= 0)
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Decimal entry is required.");
                }
                else if (!editProps.required && name.Length <= 0)
                {
                    return System.ComponentModel.DataAnnotations.ValidationResult.Success;
                } 
                decimal val;
                if (!Decimal.TryParse(name, out val))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Entry is not a valid decimal.");
                }
            }
            else if (dataObj != null && dataObj.udtItem.GetType() == typeof(UDTTxtItem))
            {
                UDTTextEditProps editProps = dataObj.udtItem.editProps as UDTTextEditProps;
                if (editProps.required && name.Length <= 0)
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Text entry is required.");
                }
                else if (!editProps.required && name.Length <= 0)
                {
                    return System.ComponentModel.DataAnnotations.ValidationResult.Success;
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
        static public DataEditViewModel dataEditViewModel = null;
        public DataEditViewModel()
        {
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            dataEditViewModel = this;
        }

        public void windowLoaded()
        {
            // load database from currently loaded schema
            if (UDTXml.UDTXmlData.SchemaData == null || UDTXml.UDTXmlData.SchemaData.Count <= 0)
                return;
            UDTData curentSchem = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            if (DBModel.Service.DataSet == null || curentSchem.Name != DBModel.Service.DataSet.DataSetName)
                loadDataSet();

            //    UDTDataSet.udtDataSet.readDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
            //UDTDataSet.udtDataSet.IsModified = false;
            //DataEditDataBase editDataBase = new DataEditDataBase();

            //UDTData dbItem = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            //foreach (UDTData table in dbItem.tableData)
            //{
            //    DataEditGrid grid = new DataEditGrid(table, navBtnClk);
            //    //if (currentEditGrid == null)
            //        currentEditGrid = grid;
            //    editDataBase.editGrids.Add(grid);
            //    createDataGrids(grid, table, navBtnClk);
            //}


            ////currentEditGrid = topDataEditGrid;
            ////dataEditDataBase = editDataBase;
            ////currentEditGrid.localNavButtonClick();
            //foreach (DataEditGrid grid in editDataBase.editGrids)
            //    grid.localNavButtonClick();
            //dataEditDataBase = editDataBase;

        }

        public void loadDataSet()
        {
            // load database from currently loaded schema

            DBModel.Service.readDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
            DBModel.Service.IsModified = false;

            loadDataGrid();

            //DataEditDataBase editDataBase = new DataEditDataBase();

            //UDTData dbItem = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            //foreach (UDTData table in dbItem.tableData)
            //{
            //    DataEditGrid grid = new DataEditGrid(table, navBtnClk);
            //    currentEditGrid = grid;
            //    editDataBase.editGrids.Add(grid);
            //    createDataGrids(grid, table, navBtnClk);
            //}

            //foreach (DataEditGrid grid in editDataBase.editGrids)
            //    grid.localNavButtonClick();
            //dataEditDataBase = editDataBase;
            
        }

        public void loadDataGrid()
        {
            DataEditDataBase editDataBase = new DataEditDataBase();

            UDTData dbItem = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            foreach (UDTData table in dbItem.tableData)
            {
                DataEditGrid grid = new DataEditGrid(table, navBtnClk);
                currentEditGrid = grid;
                editDataBase.editGrids.Add(grid);
                createDataGrids(grid, table, navBtnClk);
 
            }

            foreach (DataEditGrid grid in editDataBase.editGrids)
            {
                clkNav(grid);
            }

            dataEditDataBase = editDataBase;
        }

        private void clkNav(DataEditGrid grid)
        {
            //if (grid.gridData != null && grid.gridData.Count > 0)
            //    grid.SelectedItem = grid.gridData[0];

            grid.localNavButtonClick();
            foreach (DataEditGrid gd in grid.childGrids)
            {
                gd.localNavButtonClick();
            }         
        }

        private DataEditDataBase _dataEditDataBase = null;
        public DataEditDataBase dataEditDataBase 
        {
            get { return _dataEditDataBase; }
            set { SetProperty(ref _dataEditDataBase, value); }
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
                childGrid.parentGrid = parentGrid;

                //parentGrid.childGrids.Add(new UDTDataGrid(childGrid, udtData.Name, item, navButtonClick
                //    /*, parentGrid.canClick*/));
                parentGrid.childGrids.Add(childGrid);

                //childGrid.parentGrid = new UDTDataGrid(parentGrid, parentGrid.DataViewName, udtData, navButtonClick
                //    /*, parentGrid.canClick*/);
                createDataGrids(childGrid, item, navButtonClick);
            }
            parentGrid.detailTab = new UDTDataTab(parentGrid.childGrids);
        }

        private void navBtnClk(Guid parentId, DataEditGrid grid)
        {
            currentEditGrid = grid;
            currentEditGrid.parentId = parentId;
            //grid.DisplayTable(grid.currentDataItem);
        }
    }

    public enum EditGridDisplayPos { Parent, Main, Child };

    public class DataEditDataBase : ValidatableBindableBase
    {
        public DataEditDataBase()
        { 
        }

        private ObservableCollection<DataEditGrid> _editGrids = new ObservableCollection<DataEditGrid>();
        public ObservableCollection<DataEditGrid> editGrids
        {
            get { return _editGrids; }
            set { SetProperty(ref _editGrids, value); }
        }
    }

    public class DataEditGrid : ValidatableBindableBase
    {
        public DelegateCommand UpdateDatasetCommand { get; set; }
        public DelegateCommand AddRowCommand { get; set; }
        public DelegateCommand DeleteRowCommand { get; set; }
        public DelegateCommand<DataGrid> GridLoadedCommand { get; set; }
        public DelegateCommand NavBtnCommand { get; set; }
        public DelegateCommand<SizeChangedEventArgs> ListBoxResizeCommand { get; set; }

        public DataEditGrid(UDTData udtData, Action<Guid, DataEditGrid> navButtonClick)
        {
            UpdateDatasetCommand = new DelegateCommand(updateDataset);
            AddRowCommand = new DelegateCommand(addRow, canAddRow);
            DeleteRowCommand = new DelegateCommand(deleteRow, canDelete);
            GridLoadedCommand = new DelegateCommand<DataGrid>(gridLoaded);
            NavBtnCommand = new DelegateCommand(localNavButtonClick, canNav);
            ListBoxResizeCommand = new DelegateCommand<SizeChangedEventArgs>(listBoxResize);
            globalButtonClick = navButtonClick;
            currentDataItem = udtData;
            //DataViewName = udtData.Name;
            if (udtData.tableData.Count > 0) 
                childGridsVisable = Visibility.Visible;
            if(udtData.parentObj != null)
                _parentGridVisable = Visibility.Visible;

            editBoxes = new List<UDTDataBoxBase>();
            foreach (UDTBase item in udtData.columnData)
            {
                // create editBox type based on item typeName
                if (item.TypeName == UDTTypeName.Number )
                    //editBoxes.Add(new UDTDataNumberBox(item.Name, item, editBoxValidationChanged));
                    editBoxes.Add(new NumberViewModel(item.Name, item, editBoxValidationChanged));
                else if (item.TypeName == UDTTypeName.Real)
                    editBoxes.Add(new DecimalViewModel(item.Name, item, editBoxValidationChanged));
                else if (item.TypeName == UDTTypeName.Date)
                {
                    UDTDateEditProps props = item.editProps as UDTDateEditProps;
                    if(props.calandarTool)
                        editBoxes.Add(new UDTDataDateBox(item.Name, item, editBoxValidationChanged));
                    else
                        editBoxes.Add(new DateViewModel(item.Name, item, editBoxValidationChanged));
                }
                else
                    //editBoxes.Add(new UDTDataTextBox(item.Name, item, editBoxValidationChanged));
                    editBoxes.Add(new TextViewModel(item.Name, item, editBoxValidationChanged));
            }

            childGrids = new List<DataEditGrid>();

 
        }

        private UDTDataTab _detailTab = null;
        public UDTDataTab detailTab 
        {
            get { return _detailTab; }
            set { SetProperty(ref _detailTab, value); }
        }

        private EditGridDisplayPos _displayPos = EditGridDisplayPos.Main;        
        public EditGridDisplayPos displayPos 
        {
            get { return _displayPos; }
            set { SetProperty(ref _displayPos, value); }
        }

        DataRowView _selectedItem = null;
        public DataRowView SelectedItem
        {
            get { return _selectedItem;  }
            set 
            {
                SetProperty(ref _selectedItem, value);

                if (value != null)
                {
                    editBoxVisibility = Visibility.Visible;
                }
                else if (value == null)
                {
                    editBoxVisibility = Visibility.Collapsed;
                }


                if (editBoxes != null && _selectedItem != null) updateEditBoxes(_selectedItem);
                if (childGrids != null) updateChildGrids(_selectedItem);
                if (childGrids != null)
                {
                    foreach(DataEditGrid cg in childGrids)
                    {
                        if(cg.gridData != null && cg.gridData.Count > 0)
                        {
                            cg.SelectedIndex = 0;
                            cg.SelectedItem = cg.gridData[0];
                        }
                    }
                }

                DeleteRowCommand.RaiseCanExecuteChanged();
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set 
            { 
                SetProperty(ref _selectedIndex, value);
            }
        }

        private void updateDataset()
        {
            DBModel.Service.saveDataset();
        }

        private void listBoxResize(SizeChangedEventArgs e)
        {
            ListBoxPanelWidth = (int)e.NewSize.Width/2;
        }

        private int _ListBoxPanelWidth = 200;
        public int ListBoxPanelWidth 
        {
            get { return _ListBoxPanelWidth; }
            set { SetProperty(ref _ListBoxPanelWidth, value); }
        }

        private Visibility _addDeleteBtnVisibility = Visibility.Collapsed;
        public Visibility addDeleteBtnVisibility 
        {
            get { return _addDeleteBtnVisibility; }
            set { SetProperty(ref _addDeleteBtnVisibility, value); }
        }

        private void addRow()
        {
            DataRowView row = gridData.AddNew();
            row["Id"] = Guid.NewGuid();
            foreach (string colName in currentDataItem.ParentColumnNames)
            {
                if (colName == currentDataItem.parentObj.Name)
                //if (colName == parentGrid.currentDataItem.parentObj.Name)
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
            if (DBModel.Service.HasEditErrors) return false;
            if (currentDataItem.parentObj == null) return true;
            if (currentDataItem.parentObj.TypeName == UDTTypeName.DataBase) return true;
            return (parentId != null && parentId != Guid.Empty);
        }

        private void deleteRow()
        {
            // warn if child rows present
            if (childGrids != null && childGrids.Any(p => p.gridData != null && p.gridData.Count > 0))
            {
                if (MessageBox.Show("Delete the selected row and all child rows?", "Delete Rows",
                    MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // to invoke cascading delete operations
            // find and delete the row in the dataset corresponding to the seleted dataveiw row
            DataTable tbl = gridData.Table;
            Guid pntId = (Guid)SelectedItem["Id"];
            string filter = string.Format("Id = '{0}'", SelectedItem["Id"]);
            DataRow[] foundRows = tbl.Select(filter);
            foundRows[0].Delete();

            //if (childGrids != null)
            //{
            //    // tell child grids to re-qurey for now deleted child rows
            //    foreach (DataEditGrid cg in childGrids)
            //    {
            //        if (cg.gridData != null && cg.gridData.Count > 0)
            //        {
            //            cg.parentId = pntId;
            //        }
            //    }
            //}

            requeryChildGrids(childGrids);

            if (gridData.Count > 0)
            {
                SelectedItem = gridData[0];
            }
        }

        void requeryChildGrids(List<DataEditGrid> parentGrid)
        {
            if (parentGrid != null)
            {
                // tell child grids to re-qurey for now deleted child rows
                foreach (DataEditGrid cg in parentGrid)
                {
                    requeryChildGrids(cg.childGrids);

                    cg.parentId = Guid.Empty;
                }
            }
        }

        private bool canNav()
        {
            if (displayPos == EditGridDisplayPos.Parent) return true;
            if (displayPos == EditGridDisplayPos.Child && 
                parentId != null && parentId != Guid.Empty) return true;
            return false;
        }

        private Action<Guid, DataEditGrid> globalButtonClick;
        public void localNavButtonClick()
        {
            this.displayPos = EditGridDisplayPos.Main;
            if(SelectedItem != null)
                this.editBoxVisibility = Visibility.Visible;
            else
                this.editBoxVisibility = Visibility.Collapsed;
            this.addDeleteBtnVisibility = Visibility.Visible;
            this.navBtnVisibility = Visibility.Collapsed;

            this.IsExpanderOpen = true;
            this.IsChildExpanderOpen = false;

            if (parentGrid != null)
            {
                parentGrid.displayPos = EditGridDisplayPos.Parent;
                parentGrid.editBoxVisibility = Visibility.Collapsed;
                parentGrid.navBtnVisibility = Visibility.Visible;
                parentGrid.addDeleteBtnVisibility = Visibility.Collapsed;
                parentGrid.IsExpanderOpen = false;
            }
            else this.parentId = Guid.Empty;

            foreach (DataEditGrid child in childGrids)
            { 
                child.displayPos = EditGridDisplayPos.Child;
                child.editBoxVisibility = Visibility.Collapsed;
                child.navBtnVisibility = Visibility.Visible;
                child.addDeleteBtnVisibility = Visibility.Collapsed;
            }


            NavBtnCommand.RaiseCanExecuteChanged();

            SelectedItem = SelectedItem;

        }

        private Visibility _navBtnVisibility = Visibility.Collapsed;
        public Visibility navBtnVisibility 
        {
            get { return _navBtnVisibility; }
            set { SetProperty(ref _navBtnVisibility, value); }
        }

        private void gridLoaded(DataGrid e)
        {
            e.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(OnAutoGeneratingColumn);
            e.AutoGenerateColumns = false;
            e.AutoGenerateColumns = true;


            IEnumerable<DataColumn> dataCols = gridData.Table.Columns.Cast<DataColumn>();
            List<DataColumn> colLst = dataCols.Where(x => x.ExtendedProperties.Count > 0).ToList<DataColumn>();
            foreach (DataColumn column in colLst)
            {
                string fmtString = (string)column.ExtendedProperties["fmt"];
                DataGridColumn gc = e.Columns.First(x => x.Header == column.ColumnName);
                DataGridTextColumn tc = gc as DataGridTextColumn;
                tc.Binding.StringFormat = fmtString;
            }
        }

        protected void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(Guid))
            {
                e.Column.Visibility = Visibility.Hidden;
            }

        }




        private bool canDelete()
        {
            return SelectedItem != null;
        }

        private UDTData _currentDataItem = null;
        public UDTData currentDataItem 
        {
            get { return _currentDataItem; }
            set 
            {
                SetProperty(ref _currentDataItem, value);
                DataViewName = value.Name;
            }
        }

        private DataView getGridData(UDTData dataItem, Guid parentId)
        {
            DataView dv = null;

            if(parentId == Guid.Empty && dataItem.ParentColumnNames.Count == 0)
            {
                // if this is top level table, return all rows
                dv = new DataView(DBModel.Service.DataSet.Tables[dataItem.Name]);     
            }
            else if (DBModel.Service.DataSet.Tables[dataItem.Name].Rows.Count > 0)
            {
                // if child table that has rows, return rows that match parent row id
                DataTable childTbl = DBModel.Service.DataSet.Tables[dataItem.Name];
                string filter = string.Format("{0} = '{1}'", dataItem.parentObj.Name, parentId);
                dv = new DataView(childTbl,
                    filter, "", DataViewRowState.CurrentRows);
            }
            else
            {
                // else return empty dataset
                dv = new DataView();
                dv.Table = DBModel.Service.DataSet.Tables[dataItem.Name];
            }

            if (dv != null && dv.Count > 0)
            {
                SelectedItem = dv[0];
                SelectedIndex = 0;
            }

            return dv;
        }


        private Guid _parentId = Guid.Empty;
        public Guid parentId
        {
            get { return _parentId; }
            set
            {
                SetProperty(ref _parentId, value);
                NavBtnCommand.RaiseCanExecuteChanged();
                AddRowCommand.RaiseCanExecuteChanged();
                //// if no parent grid then this is child of single database obj so parentObj name is
                //// correct
                //if(parentGrid == null)
                //    gridData = getGridData(currentDataItem.parentObj.Name, currentDataItem, parentId);
                //// else grid may be child of multiple parent grids so parentGrid's dataItem name is
                //// correct
                //else
                //    gridData = getGridData(parentGrid.currentDataItem.Name, currentDataItem, parentId);

                gridData = getGridData(currentDataItem, parentId);

            }
        }

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

                //parentGrid.parentId = parentId;
            }

            //DataViewName = string.Format("Data Group: {0}", dataItem.Name);
            //DataViewName = dataItem.Name;

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
            return !DBModel.Service.HasEditErrors;
        }

        //private void childBtnClick(UDTData dataItem)
        //{
        //    DisplayTable(dataItem);
        //}

        //private void returnBtnClick(UDTData dataItem)
        //{
        //    //parentIds.Pop();

        //    updateChildButtons(dataItem);
        //    //gridData = getGridData(dataItem, parentIds.Peek());
        //    gridData = getGridData(dataItem, parentId);
        //    currentDataItem = dataItem;
        //    SelectedIndex = 0;
        //    SelectedItem = null;

        //}

        //private bool returnBtnCanExe()
        //{
        //    return true;
        //}

        private string _dataViewName = ""; 
        public string DataViewName 
        { 
            get 
            {
                //if (parentGrid == null)
                    return currentDataItem.Name;
                //else
                //    return currentDataItem.Name + "->";
            }
            set { SetProperty(ref _dataViewName, value); }
        }

        private string _dataViewParentName = ""; 
        public string DataViewParentName 
        { 
            get 
            {                
                string dvName = "";
                if(parentGrid != null)
                    getDataViewName(parentGrid, ref dvName);
                return dvName;
            }
            set
            {
                SetProperty(ref _dataViewParentName, value);
            }
        }

        private void getDataViewName(DataEditGrid grid, ref string dvName)
        {

            if(grid.parentGrid != null)
            {
                getDataViewName(grid.parentGrid, ref dvName);
                //dvName += "->";
            }
            dvName += grid.currentDataItem.Name + "->";
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
                //if (value != null && value.Count > 0)
                //{
                //    var row = value[0]; ;
                //    SelectedItem = value[0];
                //}
            }
        }

        private DataEditGrid _parentGrid = null;
        public DataEditGrid parentGrid
        {
            get { return _parentGrid; }
            set
            {
                SetProperty(ref _parentGrid, value);
            }
        }

        private Visibility _editBoxVisibility = Visibility.Collapsed;
        public Visibility editBoxVisibility 
        {
            get 
            {
                return _editBoxVisibility; 
            }
            set 
            { 
                SetProperty(ref _editBoxVisibility, value);
                if (value == Visibility.Collapsed) editBoxMsgVisibility = Visibility.Visible;
                else editBoxMsgVisibility = Visibility.Collapsed;
            }
        }

        private Visibility _editBoxMsgVisibility = Visibility.Collapsed;
        public Visibility editBoxMsgVisibility
        {
            get
            {
                return _editBoxMsgVisibility;
            }
            set { SetProperty(ref _editBoxMsgVisibility, value); }
        }

        private List<UDTDataBoxBase> _editBoxes = null;
        public List<UDTDataBoxBase> editBoxes 
        {
            get { return _editBoxes; }
            set
            {
                SetProperty(ref _editBoxes, value);
            }
        }

        private List<DataEditGrid> _childGrids = null;
        public List<DataEditGrid> childGrids
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

        public bool _IsChildExpanderOpen = false;
        public bool IsChildExpanderOpen
        {
            get { return _IsChildExpanderOpen; }
            set { SetProperty(ref _IsChildExpanderOpen, value); }
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

        private bool _IsExpanderOpen = false;
        public bool IsExpanderOpen
        {
            get { return _IsExpanderOpen; }
            set { SetProperty(ref _IsExpanderOpen, value); }
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


        //List<DataEditGrid> createChildGrids(UDTData dataItem)
        //{
        //    //List<UDTDataGrid> childGrids = new List<UDTDataGrid>();

        //    ////foreach (UDTBase item in dataItem.ChildData)
        //    //foreach (UDTData item in dataItem.tableData)
        //    //{
        //    //    //if (item.GetType() == typeof(UDTData))
        //    //        // put back? childGrids.Add(new UDTDataGrid(dataItem.Name, item as UDTData, childBtnClick, canClick));
        //    //}

        //    if (childGrids.Count > 0)
        //    {
        //        childGridsVisable = Visibility.Visible;
        //        parentMargin = new Thickness(0, 0, 0, 0);
        //    }
        //    else
        //    { 
        //        childGridsVisable = Visibility.Collapsed;
        //        parentMargin = new Thickness(0, 0, 30, 0);
        //    }

        //    return childGrids;
        //}

        //List<UDTDataTextBox> createEditBoxes(UDTData dataItem)
        //{
        //    List<UDTDataTextBox> boxes = new List<UDTDataTextBox>();

        //    if (SelectedItem == null) return boxes;
        //    if (editBoxes.Count > 0) return editBoxes;

        //    foreach(UDTBase item in dataItem.columnData)
        //    {
        //        boxes.Add(new UDTDataTextBox(item.Name, item, editBoxValidationChanged));
        //    }

        //    return boxes;
        //}

        private void editBoxValidationChanged(bool hasErrors)
        {
            foreach (DataEditGrid childGrid in childGrids)
            {
                //childGrid.raiseCanExecuteChanged();
            }
            foreach(UDTDataBoxBase editBox in editBoxes)
            {
                if(editBox.HasErrors)
                {
                    DBModel.Service.validationChange(true);
                    AddRowCommand.RaiseCanExecuteChanged();
                    //return;
                }
            }
            DBModel.Service.validationChange(false);
            AddRowCommand.RaiseCanExecuteChanged();
        }

        private void updateEditBoxes(DataRowView selectedRow)
        {
            
            foreach (UDTDataBoxBase editBox in editBoxes)
                editBox.row = selectedRow;
        }

        private void updateChildGrids(DataRowView selectedRow)
        {
            foreach (DataEditGrid childGrid in childGrids)
            {
                if (selectedRow != null) 
                { 
                    if (selectedRow["Id"] != DBNull.Value)
                    {
                        Guid id = (Guid)selectedRow["Id"];
                        childGrid.parentId = (Guid)selectedRow["Id"];
                    }
                    //else
                    //    childGrid.parentId = Guid.Empty;
                }
                else
                    childGrid.parentId = Guid.Empty;

                //childGrid.raiseCanExecuteChanged();
            }
        }

        //public void createColumns(System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        //{
        //    //if (e.PropertyType == typeof(Guid))
        //    //{
        //    //    e.Column.Visibility = Visibility.Hidden;
        //    //}
        //}
    }
}
