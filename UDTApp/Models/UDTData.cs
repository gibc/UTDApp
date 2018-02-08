using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using UDTApp.DataBaseProvider;
using UDTApp.ListManager;
using UDTApp.ViewModels;
using UDTApp.ViewModels.DataEntryControls;
using UDTAppControlLibrary.Controls;

namespace UDTApp.Models
{
    public enum UDTTypeName {DataBase, Group, Text, Real, Number, Date, Base}

    public class UDTData : UDTBase 
    {
        public UDTData()
        {
            tableData = new ManagedObservableCollection<UDTData>();
            columnData = new ObservableCollection<UDTBase>();
            TypeName = UDTTypeName.Group;
            Name = TypeName.ToString();
            backgroundBrush = Brushes.White;
            ParentColumnNames = new List<string>();
            dbType = DBType.none;
            sortOrder = "zzz";
        }

        public override UDTBase Clone()
        {
            UDTData tableItem = new UDTData();
            tableItem.Name = Name;
            tableItem.serverName = serverName;
            tableItem.dbType = dbType;
            tableItem.editProps = null;
            tableItem.savParentColumnNames = new List<string>(ParentColumnNames);
            tableItem.savColumnData = new ObservableCollection<UDTBase>();
            columnData.ToList().ForEach(p => tableItem.savColumnData.Add(p.Clone()));
            tableItem.tableData = new ManagedObservableCollection<UDTData>();
            tableData.ToList().ForEach(p => tableItem.tableData.Add(p.Clone() as UDTData));
            return tableItem;
        }

        public DBType dbType
        {
            get;
            set;
        }


        private string _serverName = "";
        public string serverName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        [XmlIgnoreAttribute]
        private string _savServerName = "";
        public string savServerName
        {
            get { return _savServerName; }
            set { _savServerName = value; }
        }

        public delegate void validationChangedDel();
        public event validationChangedDel validationChangedEvent;
        public void validationChanged()
        {
            if (validationChangedEvent != null) validationChangedEvent();
        }

        public delegate void dataChangeDel();
        public event dataChangeDel dataChangeEvent;
        public void dataChanged()
        {
            if (dataChangeEvent != null) dataChangeEvent();
        }

        [XmlIgnoreAttribute]
        public override bool isModified
        {
            get
            {
                if (isAllButColumnModified)
                    return true;
                // check col mods including edit props
                if (columnData.Any(p => p.isModified))
                    return true;
                return false;
            }
        }

        [XmlIgnoreAttribute]
        private bool isAllButColumnModified
        {
            get
            {
                if (string.IsNullOrEmpty(savName))
                    return true;
                if (savName != Name)
                    return true;
                if (savServerName != serverName)
                    return true;
                //if (columnData.Any(p => p.isNameModified))
                //    return true;
                if (savColumnData == null)
                    return true;
                // check any added cols
                if (columnData.Any(p => savColumnData.FirstOrDefault(q => p.Name == q.Name) == null))
                    return true;
                if (isColumnDeleted)
                    return true;
                return false;
            }
        }

        [XmlIgnoreAttribute]
        public bool isTableSchemsModified
        {
            get
            {
                if (isAllButColumnModified)
                    return true;
                if (isParentColModified)
                    return true;
                // check col mods NOT including edit prop mods
                if (columnData.Any(p => p.isNameModified))
                    return true;
                return false;
            }
        }

        [XmlIgnoreAttribute]
        public bool isParentColModified
        {
            get
            {
                // if parent column deleted
                if (savParentColumnNames != null && 
                    savParentColumnNames.Any(p => ParentColumnNames.FirstOrDefault(q => p == q) == null)) return true;
                // if parent column added
                if (ParentColumnNames != null 
                    && ParentColumnNames.Any(p => savParentColumnNames.FirstOrDefault(q => p == q) == null)) return true;

                return false;
            }
        }


        [XmlIgnoreAttribute]
        public bool isSchemaModified
        {
            get
            {
                if (isModified) return true;
                if (isParentColModified) return true;
                if (savTableData == null || tableData.Any(p => p.isModified)) return true;
                // is table deleted
                if(savTableData.Where(p => tableData.FirstOrDefault(q => q.Name == p.savName) == null).Any()) return true;
                //if (isTableDeleted) return true;
                if (tableData.Any(p => p.isSchemaModified)) return true;
                return false;
            }
        }


        //[XmlIgnoreAttribute]
        //public bool isTableDeleted
        //{
        //    get
        //    {
        //        // table is deleted only when it is not referenced in any tableData collection
        //        bool retVal = false;
        //        findGroupName(MasterGroup, savName, ref retVal);
        //        return !retVal;
        //    }
        //}

        [XmlIgnoreAttribute]
        public bool isColumnDeleted
        {
            get
            {
                if (savTableData == null) return false;
               return savColumnData.Any(p => columnData.FirstOrDefault(a => a.savName == p.Name) == null);
            }
        }


        private ManagedObservableCollection<UDTData> _tableData;
        //[XmlIgnoreAttribute]
        public ManagedObservableCollection<UDTData> tableData
        {
            get
            {
                return _tableData;
            }
            set
            {
                SetProperty(ref _tableData, value);
            }
        }

        private ObservableCollection<UDTData> _savTableData = null;
        [XmlIgnoreAttribute]
        public ObservableCollection<UDTData> savTableData
        {
            get
            {
                return _savTableData;
            }
            set
            {
                _savTableData = value;
            }
        }

        private ObservableCollection<UDTBase> _columnData;
        //[XmlIgnoreAttribute]
        public ObservableCollection<UDTBase> columnData
        {
            get
            {
                return _columnData;
            }
            set
            {
                SetProperty(ref _columnData, value);
            }
        }

        private ObservableCollection<UDTBase> _savColumnData = null;
        [XmlIgnoreAttribute]
        public ObservableCollection<UDTBase> savColumnData
        {
            get
            {
                return _savColumnData;
            }
            set
            {
                _savColumnData = value;
            }
        }

        // group children can have more than one parent
        public List<string> ParentColumnNames { get; set; }

        [XmlIgnoreAttribute]
        public List<string> savParentColumnNames { get; set; }

        public override void setSavedProps()
        {
            savName = Name;
            savServerName = serverName;
            savParentColumnNames = new List<string>(ParentColumnNames);
            columnData.ToList().ForEach(p => p.setSavedProps());
            savColumnData = new ObservableCollection<UDTBase>();
            columnData.ToList().ForEach(p => savColumnData.Add(p.Clone()));
            savTableData = new ObservableCollection<UDTData>();
            tableData.ToList().ForEach(p => savTableData.Add(p.Clone() as UDTData));
        }

        public void setAllSavedProps()
        {
            setSavedProps();
            tableData.ToList().ForEach(p => p.setAllSavedProps());
            savTableData = new ObservableCollection<UDTData>(tableData);
        }
    }

    [XmlInclude(typeof(UDTTxtItem))]
    [XmlInclude(typeof(UDTTextEditProps))]
    [XmlInclude(typeof(UDTIntItem))]
    [XmlInclude(typeof(UDTIntEditProps))]
    [XmlInclude(typeof(UDTData))]
    [XmlInclude(typeof(UDTDateItem))]
    [XmlInclude(typeof(UDTDateEditProps))]
    [XmlInclude(typeof(UDTDecimalItem))]
    [XmlInclude(typeof(UDTDecimalEditProps))]
    [XmlInclude(typeof(UDTNumberPicker))]
    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlRoot("UDTBase"), XmlType("UDTBase")]
    public class UDTBase : ValidatableBindableBase //, IXmlSerializable 
    {
        public UDTBase()
        {
            MouseEnterCommand = new DelegateCommand<MouseEventArgs>(mouseEnter);
            MouseLeaveCommand = new DelegateCommand<MouseEventArgs>(mouseLeave);
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove, disable);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter, disable);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop, disable);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);
            PreviewDragEnterCommand = new DelegateCommand<DragEventArgs>(previewDragEnter);

            SaveNameCommand = new DelegateCommand<EventArgs>(saveName, canSaveName);
            DeleteItemCommand = new DelegateCommand<EventArgs>(deleteItem);
            PopupLoadCommand = new DelegateCommand<EventArgs>(popupLoad);
            SizeChangedCommand = new DelegateCommand<RoutedEventArgs>(sizeChange);
            GroupBoxLoadedCommand = new DelegateCommand<RoutedEventArgs>(groupBoxLoaded);
            ItemNameEditGotFocusCommand = new DelegateCommand<RoutedEventArgs>(itemNameEditGotFocus);
            ItemNameEditLostFocusCommand = new DelegateCommand<RoutedEventArgs>(itemNameEditLostFocus);

            objId = Guid.NewGuid();
            backgroundBrush = Brushes.Black;
            buttonWidth = 35;
            buttonHeight = 12;
            sortOrder = "zzz";

            ErrorsChanged += OnErrorsChanged;
        }

        public virtual UDTBase Clone()
        {
            UDTBase intItem = new UDTBase();
            intItem.Name = Name;
            intItem.editProps = null;
            intItem.objId = objId;
            return intItem;
        }

        public void editPropValidaionChanged()
        {
            if (!HasErrors && editProps.HasErrors)
            {
                List<string> errLst = new List<string>();
                errLst.Add("<dummy>");
                SetErrors(() => this.Name, errLst);
            }
            else if (!editProps.HasErrors && HasErrors)
            {
                List<string> errLst = (List<string>)GetErrors("Name");
                errLst.Remove("<dummy>");
                SetErrors(() => this.Name, errLst);
            }
            updateErrorList("Name");
        }

        private void updateErrorList(string propName)
        {
            List<string> showErrors = new List<string>();
            if (HasErrors)
            {
                List<string> errList = (List<string>)GetErrors(propName);
                showErrors.AddRange(errList);
                showErrors.Remove("<dummy>");
            }
            if (editProps != null && editProps.HasErrors)
            {
                showErrors.AddRange(editProps.currentValidationError);
            }
            currentValidationError = showErrors;
        }

        private List<string> _currentValidationError = new List<string>();
        public List<string> currentValidationError
        {
            get { return _currentValidationError; }
            set { SetProperty(ref _currentValidationError, value); }
        }

        private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (editProps != null && !HasErrors && editProps.HasErrors)
            {
                List<string> lst = new List<string>();
                lst.Add("<dummy>");
                SetErrors(() => this.Name, lst);
            }
            updateErrorList(e.PropertyName);

            if (UDTXml.UDTXmlData.SchemaData.Count > 0)
            {
                UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                master.validationChanged();
            }
        }


        private bool disable(EventArgs eventArgs)
        {
            if (ToolBoxItem) return true;
            if (parentObj == null) return true;
            return true;
        }

        [XmlIgnoreAttribute]
        public DelegateCommand<MouseEventArgs> MouseEnterCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<MouseEventArgs> MouseLeaveCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<DragEventArgs> DragEnterCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<DragEventArgs> DragDropCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<DragEventArgs> DragOverCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> SaveNameCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> DeleteItemCommand { get; set; }
        //[XmlIgnoreAttribute]
        //public DelegateCommand<EventArgs> PopupOpenCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> PopupLoadCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<DragEventArgs> PreviewDragEnterCommand { get; set; }
        [XmlIgnoreAttribute]
        //public DelegateCommand<SizeChangedEventArgs> SizeChangedCommand { get; set; }
        public DelegateCommand<RoutedEventArgs> SizeChangedCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<RoutedEventArgs> GroupBoxLoadedCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<RoutedEventArgs> ItemNameEditGotFocusCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<RoutedEventArgs> ItemNameEditLostFocusCommand { get; set; }

        private UDTBaseEditProps _editProps = null;
        //[XmlIgnoreAttribute]
        public UDTBaseEditProps editProps
        {
            get { return _editProps; }
            set { SetProperty(ref _editProps, value); }
        }

        private UDTBaseEditProps _savEditProps = null;
        [XmlIgnoreAttribute]
        public UDTBaseEditProps savEditProps
        {
            get { return _savEditProps; }
            set { SetProperty(ref _savEditProps, value); }
        }

        // serializes first so can add objects to table dictionary with ids
        //[XmlElement(Order = 1)] exception unless used on all fields, proporties
        public Guid objId;
        public Guid dragObjId = Guid.Empty;

        private SolidColorBrush _backgroundBrush = null;
        [XmlIgnoreAttribute]
        public SolidColorBrush backgroundBrush
        {
            get
            {
                if (_backgroundBrush == null)
                {
                    var color = (Color)ColorConverter.ConvertFromString(brushColor);
                    _backgroundBrush = new SolidColorBrush(color);
                }
                return _backgroundBrush;
            }

            set { SetProperty(ref _backgroundBrush, value); }
        }

        private string _brushColor = null;
        public string brushColor
        {
            get { return backgroundBrush.Color.ToString(); }
            set { _brushColor = value; }
        }

        private int _buttonWidth = 0;
        public int buttonWidth
        {
            get
            {
                if (_toolBoxItem) return 54;
                return _buttonWidth;
            }
            set { SetProperty(ref _buttonWidth, value); }
        }

        private int _buttonHeight = 0;
        public int buttonHeight
        {
            get
            {
                if (_toolBoxItem) return 22;
                return _buttonHeight;
            }
            set { SetProperty(ref _buttonHeight, value); }
        }

        public int dragButtonFontSize
        {
            get
            {
                if (_toolBoxItem) return 13;
                return 9;
            }
        }

        public string sortOrder { get; set; }

        // serialize this instead of parentObj reference so
        // parentObj get can find ojbect in dataTable dictionary
        public Guid parentObjId { get; set; }
       
        private UDTData _parentObj = null;
        [XmlIgnoreAttribute]
        public UDTData parentObj 
        {
            get
            {
                if(_parentObj == null && parentObjId != Guid.Empty && TableDictionary.itemDic.ContainsKey(parentObjId))
                {
                    TableRef tableRef = null;
                    if (TableDictionary.itemDic.TryGetValue(parentObjId, out tableRef))
                    {
                        _parentObj = tableRef.item;
                    }

                }
                return _parentObj;
            }
            set
            {
                SetProperty(ref _parentObj, value);
                if (_parentObj != null)
                    parentObjId = _parentObj.objId;
            }
        }

        private bool newDrop = false;

        [XmlIgnoreAttribute]
        public UDTData MasterGroup
        {
            get
            {
                if (UDTXml.UDTXmlData.SchemaData == null || UDTXml.UDTXmlData.SchemaData.Count <= 0) return null;
                return UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            }
        }

        private bool _anyErrors = true;
        public bool AnyErrors
        {
            get { return _anyErrors; }
            set 
            {
                SetProperty(ref _anyErrors, value);
                MouseMoveCommand.RaiseCanExecuteChanged();
                DragEnterCommand.RaiseCanExecuteChanged();
                DragDropCommand.RaiseCanExecuteChanged();
                DragOverCommand.RaiseCanExecuteChanged();
                DeleteItemCommand.RaiseCanExecuteChanged();

                //Debug.Write(string.Format("AnyErrors after CanExChanged {0}\r", value));

            }
        }

        public void SetAnyErrorAll(UDTData dataItem, bool value)
        {
            Debug.Write(string.Format(">>>> Enter SetAnyErrorAll value {0} Name: {1}\r", value, dataItem.Name));
            dataItem.AnyErrors = value;
            dataItem.EditBoxEnabled = !value;
            foreach (UDTData obj in dataItem.tableData)
            {
                SetAnyErrorAll(obj as UDTData, value);
                obj.AnyErrors = value;
                obj.EditBoxEnabled = !value;
            }
            foreach(UDTBase obj in dataItem.columnData)
            {
                obj.AnyErrors = value;
                obj.EditBoxEnabled = !value;

            }
     
        }

        [XmlIgnoreAttribute]
        public Action<bool> setAnyError { get; set; }

        private bool _popUpOpen = false;
        public bool PopUpOpen 
        {
            get 
            { 
                return _popUpOpen;
            }
            set 
            { 
                SetProperty(ref _popUpOpen, value);
            }
        }

        private bool _errorTextVisable = false;
        public bool ErrorTextVisable 
        {
            get 
            { 
                return _errorTextVisable;
            }
            set { SetProperty(ref _errorTextVisable, value); }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set { SetProperty(ref _isEnabled, value);  }
        }

        public int EditBoxMinWidth
        {
            get
            {
                return 40;
            }
        }

        private bool _editBoxEnabled = false;
        public bool EditBoxEnabled
        {
            get
            {
                Debug.Write(string.Format("<<<Get EBEnable HasErrors {0} name: {1} _editBoxEnabled {2}\r",
                    HasErrors, Name, _editBoxEnabled));
                if (ToolBoxItem)
                    return false;
                if (HasErrors)
                    return true;
                return _editBoxEnabled;
 
            }

            set 
            {
                Debug.Write(string.Format(">>>Set EBEnable value {0} name: {1}\r", value, Name));
                SetProperty(ref _editBoxEnabled, value);
            }

        }

        public Thickness ButtonWrapPanelMargin 
        { 
            get
            {
                if(ToolBoxItem)
                    return new Thickness(0, 0, 0, 0);
                else
                    return new Thickness(0, 0, -10, 0);
            }
 
        }

        public string Type { get; set; }

        // used to show parent of group name without error adorner
        private string _displayName = "";
        public string DisplayName 
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        [XmlIgnoreAttribute]
        private string _savName = "";
        public string savName
        {
            get { return _savName; }
            set { _savName = value; }
        }

        [XmlIgnoreAttribute]
        public bool isNameModified
        {
            get
            {
                if (string.IsNullOrEmpty(savName))
                    return true;
                if (savName != Name)
                    return true;
                return false;
            }
        }

        [XmlIgnoreAttribute]
        public virtual bool isModified
        {
            get
            {
                //if(string.IsNullOrEmpty(savName))
                //    return true;
                //if (savName != Name)
                //    return true;
                return isNameModified;
            }
        }

        public virtual void setSavedProps()
        {
            savName = Name;
        }


        public static List<string> sqlWordList = 
                                @"ADD EXTERNAL PROCEDURE
                                ALL FETCH PUBLIC
                                ALTER FILE  RAISERROR
                                AND FILLFACTOR READ
                                ANY FOR READTEXT
                                AS  FOREIGN RECONFIGURE
                                ASC FREETEXT  REFERENCES
                                AUTHORIZATION  FREETEXTTABLE REPLICATION
                                BACKUP FROM  RESTORE
                                BEGIN  FULL RESTRICT
                                BETWEEN FUNCTION RETURN
                                BREAK GOTO REVERT
                                BROWSE GRANT   REVOKE
                                BULK    GROUP RIGHT
                                BY HAVING  ROLLBACK
                                CASCADE HOLDLOCK ROWCOUNT
                                CASE IDENTITY    ROWGUIDCOL
                                CHECK   IDENTITY_INSERT RULE
                                CHECKPOINT IDENTITYCOL SAVE
                                CLOSE   IF SCHEMA
                                CLUSTERED IN  SECURITYAUDIT
                                COALESCE    INDEX SELECT
                                COLLATE INNER   SEMANTICKEYPHRASETABLE
                                COLUMN  INSERT SEMANTICSIMILARITYDETAILSTABLE
                                COMMIT INTERSECT   SEMANTICSIMILARITYTABLE
                                COMPUTE INTO SESSION_USER
                                CONSTRAINT IS  SET
                                CONTAINS    JOIN SETUSER
                                CONTAINSTABLE KEY SHUTDOWN
                                CONTINUE    KILL SOME
                                CONVERT LEFT    STATISTICS
                                CREATE  LIKE SYSTEM_USER
                                CROSS LINENO  TABLE
                                CURRENT LOAD TABLESAMPLE
                                CURRENT_DATE MERGE   TEXTSIZE
                                CURRENT_TIME    NATIONAL THEN
                                CURRENT_TIMESTAMP NOCHECK TO
                                CURRENT_USER    NONCLUSTERED TOP
                                CURSOR NOT TRAN
                                DATABASE    NULL TRANSACTION
                                DBCC NULLIF  TRIGGER
                                DEALLOCATE  OF TRUNCATE
                                DECLARE OFF TRY_CONVERT
                                DEFAULT OFFSETS TSEQUAL
                                DELETE ON  UNION
                                DENY    OPEN UNIQUE
                                DESC OPENDATASOURCE  UNPIVOT
                                DISK    OPENQUERY UPDATE
                                DISTINCT OPENROWSET  UPDATETEXT
                                DISTRIBUTED OPENXML USE
                                DOUBLE OPTION  USER
                                DROP    OR VALUES
                                DUMP ORDER   VARYING
                                ELSE    OUTER VIEW
                                END OVER    WAITFOR
                                ERRLVL  PERCENT WHEN
                                ESCAPE PIVOT   WHERE
                                EXCEPT  PLAN WHILE
                                EXEC PRECISION   WITH
                                EXECUTE PRIMARY WITHIN GROUP
                                EXISTS  PRINT WRITETEXT
                                EXIT PROC".Split(' ')
                                    .Where(x => !string.IsNullOrWhiteSpace(x)).Select(s => s.Trim()).ToList();

        private string _name = "";
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 15 characters.")]
        //[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "Name can include only letters, numbers, '_' or '-'")]
        [CustomValidation(typeof(UDTBase), "CheckDuplicateColumnName")]
        [CustomValidation(typeof(UDTBase), "CheckEmptyTable")]
        [CustomValidation(typeof(UDTBase), "CheckSqlWord")]
        public string Name
        {
            get { return _name; }
            set 
            {
                DisplayName = value;
                if (!string.IsNullOrEmpty(_name) && _name != value && MasterGroup != null)
                    MasterGroup.dataChanged();
                SetProperty(ref _name, value);
                if (HasErrors)
                    PopUpOpen = true;
                else
                    PopUpOpen = false;
                ErrorTextVisable = HasErrors;
                if (MasterGroup != null) SetAnyErrorAll(MasterGroup, HasErrors);
                if (MasterGroup != null) MasterGroup.validationChanged();

                newDrop = HasErrors;
                //RaisePropertyChanged("EditBoxEnabled");
                SaveNameCommand.RaiseCanExecuteChanged();
            }
        }

        private UDTTypeName _typeName = UDTTypeName.Base;
        public UDTTypeName TypeName
        {
            get
            {
                return _typeName;
            }
            set 
            { 
                SetProperty(ref _typeName, value); 
                if(value == UDTTypeName.DataBase)
                {
                    ToolBoxItem = false;
                    dragButtonVisibility = Visibility.Collapsed;
                    editButtonVisibility = Visibility.Collapsed;
                    backgroundBrush = Brushes.SandyBrown;
                    TableDictionary.itemDic = new Dictionary<Guid, TableRef>();
                    // TBD: put database ref in table dic so parentOjb references will return
                    // master item and eliminate the need for parentObj fix up
                    TableRef tableRef = new TableRef() { refCount = 1, item = (UDTData)this };
                    TableDictionary.itemDic.Add(this.objId, tableRef);
                }
            }
        }

        private Visibility _dragButtonVisibility = Visibility.Visible;
        public Visibility dragButtonVisibility 
        {
            get { return _dragButtonVisibility; }
            set { SetProperty(ref _dragButtonVisibility, value); }
        }

        private Visibility _editButtonVisibility = Visibility.Collapsed;
        public Visibility editButtonVisibility
        {
            get { return _editButtonVisibility; }
            set { SetProperty(ref _editButtonVisibility, value); }
        }


        private bool _toolBoxItem = true;
        public bool ToolBoxItem 
        { 
            get {return _toolBoxItem; }
            set 
            { 
                _toolBoxItem = value; 
                SchemaItem = !value;
                if (value == true) editButtonVisibility = Visibility.Collapsed;
                else if(TypeName != UDTTypeName.DataBase) editButtonVisibility = Visibility.Visible;
            } 
        }

        public bool SchemaItem { get; set; }

        virtual public bool IsReadOnly
        {
            get { return ToolBoxItem; }
        }

        private void saveName(EventArgs eventArgs)
        {
            PopUpOpen = false;
        }

        private bool isTableEmpty(UDTData dataItem)
        {
            if (!UDTDataSet.udtDataSet.dataBaseExists(MasterGroup.dbType, MasterGroup.Name)) return true;

            if (UDTDataSet.udtDataSet.DataSet != null)
            {
                if (!UDTDataSet.udtDataSet.DataSet.Tables.Contains(dataItem.Name)) return true;

                DataTable tb = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];
                if (tb.Rows.Count <= 0) return true;

                foreach (UDTBase col in dataItem.columnData)
                {
                    if (col.Name == "Id") continue;

                    EnumerableRowCollection<DataRow> rows = tb.AsEnumerable().
                        Where(r => r[col.Name] != DBNull.Value);
                    if (rows.Any() && col.TypeName == UDTTypeName.Text)
                    {
                        rows = rows.Where(r => !string.IsNullOrEmpty((string)r[col.Name]));
                    }
                    if (rows.Any()) return false;                      
                }
                return true;
            }
            else
            {
                UDTDataSet.dbProvider = new DbProvider(MasterGroup.dbType, MasterGroup.serverName);
                return UDTDataSet.udtDataSet.isTableEmpty(dataItem, MasterGroup.Name);
            }

        }

        private bool isColumnEmpty(UDTData dataItem, UDTBase colItem)
        {
            if (isTableEmpty(dataItem)) return true;

            if (UDTDataSet.udtDataSet.DataSet != null)
            {
                DataTable tb = UDTDataSet.udtDataSet.DataSet.Tables[dataItem.Name];
                if (!tb.Columns.Contains(colItem.Name)) return true;
                if (colItem.TypeName == UDTTypeName.Text)
                {
                    EnumerableRowCollection<DataRow> rows = tb.AsEnumerable().Where(r => r[colItem.Name] != DBNull.Value);
                    if(rows.Any())
                        rows = rows.Where(r => !string.IsNullOrEmpty((string)r[colItem.Name]));
                    return !rows.Any();
                }
                else
                {
                    EnumerableRowCollection<DataRow> rows = tb.AsEnumerable().Where(r => r[colItem.Name] != DBNull.Value);
                    return !rows.Any();
                }
            }
            else
            {
                UDTDataSet.dbProvider = new DbProvider(MasterGroup.dbType, MasterGroup.serverName);
                return UDTDataSet.udtDataSet.isColumnEmpty(dataItem.Name, colItem.Name, MasterGroup.Name);
            }
        }

        private bool isTreeBranchEmpty(UDTData data, UDTBase item)
        {
            if (item.GetType() == typeof(UDTData))
            {
                if (data.tableData.Contains(item))
                {
                    if (item.Name == item.savName && !isTableEmpty(item as UDTData))
                    {
                        MessageBox.Show(
                            string.Format("Review and delete the data stored in the '{0}' group before removing it from the data design.", item.Name),
                             "Data Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
            }
            else
            {
                if (data.columnData.Contains(item))
                {
                    if (item.Name == item.savName && !isColumnEmpty(data, item))
                    {
                        MessageBox.Show(string.Format("Review and delete the data stored in the '{0}' item before removing it from the data design.", item.Name),
                            "Data Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
            }
            foreach (UDTData obj in data.tableData)
            {
                if (!isTreeBranchEmpty(obj as UDTData, item)) return false;
            }

            return true;
        }

        private void removeItem(UDTData data, UDTBase item)
        {

            if (item.GetType() == typeof(UDTData))
            {
                if (data.tableData.Contains(item))
                {
                    //if (item.Name == item.savName && !isTableEmpty(item as UDTData))
                    //{
                    //    MessageBox.Show(
                    //        string.Format("Review and delete the data stored in the '{0}' group before removing it from the data design.", item.Name),
                    //         "Data Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                    //    //{
                    //    //    data.tableData.Remove(item as UDTData);
                    //    //}
                    //    return false;
                    //}
                    //else
                    {
                        data.tableData.Remove(item as UDTData);
                        MasterGroup.dataChanged();
                    }
                }
            }
            else
            {
                if (data.columnData.Contains(item))
                {
                    //if (item.Name == item.savName && !isColumnEmpty(data, item))
                    //{
                    //    MessageBox.Show(string.Format("Review and delete the data stored in the '{0}' item before removing it from the data design.", item.Name),
                    //        "Data Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                    //    //{
                    //    //    data.columnData.Remove(item);
                    //    //}
                    //    return false;
                    //}
                    //else
                    {
                        data.columnData.Remove(item);
                        MasterGroup.dataChanged();
                    }
                }
            }
            foreach (UDTData obj in data.tableData)
            {
                removeItem(obj as UDTData, item);
            }
            //return true;
        }

        private void deleteItem(EventArgs eventArgs)
        {
            // cant remove top, database otj
            if (parentObj != null) return;

            // don't remove obj if it or items lower in tree have data in database
            if (!isTreeBranchEmpty(parentObj, this)) return;

            // remove only this item and items below this item in the obj tree
            removeItem(parentObj, this);

            parentObj.ParentColumnNames.Remove(this.Name);
            
            this.parentObj.ValidateProperty("Name");
            if(parentObj.groupBox != null)
            {
                // this is nessecary to pull error messages from viewModel into the control's Validation property
                BindingExpression binding = parentObj.groupBox.GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }

            MasterGroup.dataChanged();
        }

        private void popupLoad(EventArgs eventArgs)
        {
            RoutedEventArgs args = eventArgs as RoutedEventArgs;
            Popup pu = args.Source as Popup;
            TextBox tb = pu.FindName("NameBox") as TextBox;
            var fe = Keyboard.Focus(tb);
        }

        private TextBox groupBox = null;
        private void groupBoxLoaded(RoutedEventArgs e)
        {
            TextBox gpBox = e.Source as TextBox;
            groupBox = gpBox;
        }

        private void itemNameEditGotFocus(RoutedEventArgs e)
        {
            if (TypeName == UDTTypeName.Text || TypeName == UDTTypeName.Date ||
                TypeName == UDTTypeName.Real || TypeName == UDTTypeName.Number)
            {
                PageZeroViewModel.viewModel.currentEditItem = this;
            }
            else PageZeroViewModel.viewModel.currentEditItem = null;
        }
        private void itemNameEditLostFocus(RoutedEventArgs e)
        {
            if (TypeName == UDTTypeName.Text)
            {
                UDTTxtItem txtItem = this as UDTTxtItem;
            }
        }

        // change table data items control margin when item added to collection
        static private int tableDataIndent = 25;
        static private int tableDataTopMargin = -15;
        private void sizeChange(RoutedEventArgs e)
        {

            ItemsControl itmCnt = e.Source as ItemsControl;
            if (itmCnt.Items.Count > 0)
                tableDataMargin = new Thickness(tableDataIndent, tableDataTopMargin, 0, 0);

            ((INotifyCollectionChanged)itmCnt.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(addEvent);

        }

        private void addEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            tableDataMargin = new Thickness(tableDataIndent, tableDataTopMargin, 0, 0);
        }

        private Thickness _tableDataMargin = new Thickness(tableDataIndent, 4, 0, 0);
        public Thickness tableDataMargin
        {
            get
            {
                return _tableDataMargin;
            }
            set { SetProperty(ref _tableDataMargin, value); }
        }

        private bool canSaveName(EventArgs eventArgs)
        {
            return !HasErrors;
        }

        private void dragOver(DragEventArgs dragArgs)
        {
            Button btn = dragArgs.Source as Button;
            dragArgs.Effects = DragDropEffects.Copy;
        }

        private void dragDrop(DragEventArgs dragArgs)
        {
            inDrag = false;
            Button btn = dragArgs.Source as Button;
            TextBox box = dragArgs.Source as TextBox;
            if (!dragArgs.Handled && btn != null || box != null)
            {
                //ObservableCollection<UDTBase> col = Ex.GetSecurityId(btn);

                UDTBase udtItem = getItemFromDragArgs(dragArgs);
                UDTBase udtBase = udtItem as UDTBase;

                // prevent copy to self
                if (udtBase.dragObjId == this.objId) return;

                //if(udtItem.Name == "") udtItem.Name = "<Name>";
            
                UDTData udtData;
                if (udtItem != null )
                {
                    UDTData parent = this as UDTData;
                    udtItem.parentObj = parent;
                    if (udtItem.GetType() == typeof(UDTData))
                    {
                        udtData = udtItem as UDTData;
                        // database item is not parent column in child items
                        if (parent.TypeName == UDTTypeName.Group) 
                            udtData.ParentColumnNames.Add(this.Name);
                        if (!parent.tableData.Contains(udtData))
                        {
                            // check if table already exits to handle
                            // more than one reference to same table
                            bool retVal = false;
                            findGroupName(MasterGroup, udtData.Name, ref retVal);
                            if (!retVal)
                            {
                                udtData.Name = getUniqueGroupName(MasterGroup);
                            }
                            parent.tableData.Add(udtData);
                        }
                    }
                    else
                    {
                        if (!parent.columnData.Contains(udtItem))
                        {
                            udtItem.Name = getUniqueColumnName(parent.columnData);
                            parent.columnData.Add(udtItem);
                        }

                    }

                    parent.ValidateProperty("Name");
    
                    udtItem.newDrop = true;
                    MasterGroup.dataChanged();
                }
                dragArgs.Handled = true;
                _currentItem = null;
            }
        }

        private string getUniqueColumnName(ObservableCollection<UDTBase> columnData)
        {
            string colName = "DataItem";
            for (char c = 'A'; c <= 'Z'; c++)
            {
                UDTBase item = columnData.FirstOrDefault(x => x.Name == colName);
                if (item == null)
                    return colName;
                else
                    colName = "DataItem" + c; 
            }
            return "";
        }

        private string getUniqueGroupName(UDTData tableData)
        {
            string tableName = "GroupItem";
            for (char c = 'A'; c <= 'Z'; c++)
            {
                bool retVal = false;
                findGroupName(tableData, tableName, ref retVal);
                if(retVal == false)
                    return tableName;
                else
                    tableName = "GroupItem" + c;
            }
            return "";
        }

        protected void findGroupName(UDTData udtData, string name, ref bool retVal)
        {
            if (!string.IsNullOrEmpty(udtData.savName) && udtData.savName == name || udtData.Name == name)
            {
                retVal = true;
                return;
            }
            foreach(UDTData child in udtData.tableData)
            {
                findGroupName(child, name, ref retVal);
            }
        }

        private UDTBase _currentItem = null;
        private void dragEnter(DragEventArgs dragArgs)
        {
            Button btn = dragArgs.Source as Button;
            if (btn != null)
            {
                UDTBase udtItem = getItemFromDragArgs(dragArgs);

                _currentItem = udtItem;

            }
        }

        private void previewDragEnter(DragEventArgs dragArgs)
        {
            dragArgs.Handled = true;

            UDTBase dragItem = getItemFromDragArgs(dragArgs);
            if (dragItem != null)
            {
                // only tables can be dropped on database item
                if(this.TypeName == UDTTypeName.DataBase && dragItem.TypeName != UDTTypeName.Group)
                    dragArgs.Effects = DragDropEffects.None;
                else 
                    dragArgs.Effects = DragDropEffects.Copy;

            }
            else
                dragArgs.Effects = DragDropEffects.None;
        }
 
        private void mouseEnter(MouseEventArgs data)
        {
            TextBox txtBox = data.Source as TextBox; 
            if (txtBox.IsMouseOver && HasErrors || (editProps != null && editProps.HasErrors))
            {
                PopUpOpen = true;
            }
            else
            {
                PopUpOpen = false;
            }

        }

        private void mouseLeave(MouseEventArgs data)
        {
            TextBox txtBox = data.Source as TextBox;
            if(!txtBox.IsMouseOver)
            { 
                PopUpOpen = false;
            }
        }

        private bool inMove = false;
        private static bool inDrag = false;
        private void mouseMove(MouseEventArgs data)
        {
            Button btn = data.Source as Button;
            TextBox box = data.Source as TextBox;

            if ((btn != null || box != null) && data.LeftButton == MouseButtonState.Pressed && !inMove
                && (this.GetType() == typeof(UDTData) && this.parentObj != null || this.ToolBoxItem)) 
            {

                inMove = true;
                inDrag = true;

                UDTBase udtItem;
                if (this.ToolBoxItem)
                {
                    udtItem = (UDTBase)Activator.CreateInstance(this.GetType());
                    udtItem.ToolBoxItem = false;
                }
                else
                    udtItem = this;

                udtItem.dragObjId = this.objId;

                if (udtItem != null )
                {
                    if (btn != null)
                        DragDrop.DoDragDrop(btn,
                          udtItem,
                          DragDropEffects.Copy); 
                    else if (box != null)
                        DragDrop.DoDragDrop(box,
                          udtItem,
                          DragDropEffects.Copy);
                }

                //Debug.Write(string.Format("<<<Exit mouseMove\r", _currentItem));
                data.Handled = true;
                inMove = false;
                inDrag = false;
            }
        }

        private UDTBase getItemFromDragArgs(DragEventArgs dragArgs)
        {
            UDTBase udtItem = (UDTData)dragArgs.Data.GetData(typeof(UDTData));
            if (udtItem != null) return udtItem;
            udtItem = (UDTTxtItem)dragArgs.Data.GetData(typeof(UDTTxtItem));
            if (udtItem != null) return udtItem;
            udtItem = (UDTIntItem)dragArgs.Data.GetData(typeof(UDTIntItem));
            if (udtItem != null) return udtItem;
            udtItem = (UDTDecimalItem)dragArgs.Data.GetData(typeof(UDTDecimalItem));
            if (udtItem != null) return udtItem;
            udtItem = (UDTDateItem)dragArgs.Data.GetData(typeof(UDTDateItem));
            return udtItem;
        }

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckEmptyTable(string name, ValidationContext context)
        {
            UDTData dataObj = context.ObjectInstance as UDTData;
            if (dataObj != null && !dataObj.ToolBoxItem)
            {
                if(dataObj.TypeName == UDTTypeName.DataBase)
                {
                    if(dataObj.tableData.Count <= 0)
                        return new System.ComponentModel.DataAnnotations.ValidationResult("Data base must include at least one group item.");
                }
                else if (dataObj.columnData.Count <= 0)
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Group item must include at least one data item.");
                dataObj.PopUpOpen = false;
            }

            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckSqlWord(string name, ValidationContext context)
        {
            UDTBase dataObj = context.ObjectInstance as UDTBase;
            if (dataObj != null && !dataObj.ToolBoxItem)
            {
                if (UDTBase.sqlWordList.Contains(name.ToUpper()))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Error: Reserved SQL key word.");
                }
            }

            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }
        public static System.ComponentModel.DataAnnotations.ValidationResult CheckDuplicateColumnName(string name, ValidationContext context)
        {
            UDTBase dataObj = context.ObjectInstance as UDTBase;
            if(dataObj != null && dataObj.parentObj != null && dataObj.GetType() != typeof(UDTData))
            {
                foreach(UDTBase obj in dataObj.parentObj.columnData)
                {
                    if (obj.Name == name && obj.objId != dataObj.objId)
                        return new System.ComponentModel.DataAnnotations.ValidationResult("Duplicate item name. Item names must be unique within an item group.");
                }
            }
            else if (dataObj != null && dataObj.parentObj != null && dataObj.GetType() == typeof(UDTData))
            {
                if (dataObj.findGroupName(dataObj.MasterGroup, dataObj.objId, name))
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Duplicate group name. Group names must be unique.");
            }

            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

        private bool findGroupName(UDTData dataItem, Guid currentObjId, string name)
        {
            if (dataItem == null) return false;
            foreach (UDTBase item in dataItem.tableData)
            {
                if(item.GetType() == typeof(UDTData))
                {
                    if (item.Name == name && item.objId != currentObjId)
                        return true;
                    return findGroupName(item as UDTData, currentObjId, name);
                }
            }
            return false;
            
        }

    }

    public class UDTRelation
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }
        public string ChildColumnName { get; set; }
    }

    public class UDTTxtItem : UDTBase
    {
        public UDTTxtItem()
        { 
            Type = "[varchar](255) NULL ";
            TypeName = UDTTypeName.Text;
            Name = TypeName.ToString();
            backgroundBrush = Brushes.LightBlue;
            sortOrder = "bbb";

            editProps = new UDTTextEditProps(editPropValidaionChanged);
            //editProps.parentItem = this;
        }

        public override void setSavedProps()
        {
            base.setSavedProps();
            savEditProps = new UDTTextEditProps(editProps);
        }

        public override UDTBase Clone()
        {
            UDTTxtItem txtItem = new UDTTxtItem();
            txtItem.Name = Name;
            txtItem.editProps = new UDTTextEditProps(editProps);
            return txtItem;
        }

        [XmlIgnoreAttribute]
        public override bool isModified
        {
            get
            {
                if (base.isModified) return true;
                if (savEditProps == null)
                    return true;
                return !editProps.Equals(savEditProps);
            }
        }
    }

    public class UDTBaseEditProps : ValidatableBindableBase
    {
        public UDTBaseEditProps()
        {
            editPropValidationChanged = null;      
        }
        public UDTBaseEditProps(Action editPropChanged) 
        {
            ErrorsChanged += OnErrorsChanged;
            editPropValidationChanged = editPropChanged;
        }

        static string[] noErrors = new string[0];
        private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (HasErrors)
            {
                List<string> errList = new List<string>();
                string[] errAry = null;
                try
                {
                    errAry = (string[])GetErrors(e.PropertyName);
                }
                catch
                {
                    errList = (List<string>)GetErrors(e.PropertyName);
                }
                currentValidationError = errList;
            }
            else currentValidationError = new List<string>();
            editPropValidationChanged();

            UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            master.validationChanged();
        }

        // NOTE: will be set by the non-default ctor that is called by the 
        // default ctor of all classes that create instantes of this class
        [XmlIgnoreAttribute]
        public Action editPropValidationChanged { get; set; }


        private List<string> _currentValidationError = new List<string>();
        public List<string> currentValidationError
        {
            get { return _currentValidationError; }
            set { SetProperty(ref _currentValidationError, value); }
        }

        public void editPropsDataChanged()
        {
            if (UDTXml.UDTXmlData.SchemaData != null && UDTXml.UDTXmlData.SchemaData.Count > 0)
            {
                UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                master.dataChanged();
            }
        }


        private bool _required = false;
        public bool required
        {
            get { return _required; }
            set
            {
                SetProperty(ref _required, value);
                editPropsDataChanged();
            }
        }
    }

    [XmlInclude(typeof(UDTBaseEditProps))]
    public class UDTTextEditProps : UDTBaseEditProps
    {
        private UDTTextEditProps() : base() { }

        public UDTTextEditProps(UDTBaseEditProps copyProps) : base()
        {
            UDTTextEditProps _copyProps = copyProps as UDTTextEditProps;
            defaultText = _copyProps.defaultText;
            maxLength = _copyProps.maxLength;
            minLength = _copyProps.minLength;
            required = _copyProps.required;
        }

        public override bool Equals(Object obj)
        {
            UDTTextEditProps savProps = obj as UDTTextEditProps;
            bool retVal = (defaultText == savProps.defaultText &&
            maxLength == savProps.maxLength &&
            minLength == savProps.minLength &&
            required == savProps.required);
            if (retVal)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public UDTTextEditProps(Action editPropChanged) : base(editPropChanged)
        {
            //minPicker = new UDTNumberPicker("Min Text Length", 254, 0, NumberPickerType.Integer, minChanged);
            //maxPicker = new UDTNumberPicker("Max Text", 255, 1, NumberPickerType.Integer, maxChanged);
            //minPicker.number = 0;
            //maxPicker.number = 255;
        }


        private string _defaultText = "";
        public string defaultText
        {
            get { return _defaultText; }
            set 
            { 
                SetProperty(ref _defaultText, value);
                editPropsDataChanged();
            }
        }

        //public UDTNumberPicker minPicker { get; set; }
        //public UDTNumberPicker maxPicker { get; set; }

        private Int32? _minLength = 2;        
        public Int32? minLength 
        {
            get { return _minLength; }
            set
            {
                SetProperty(ref _minLength, value);
                editPropsDataChanged();
                if (maxLength != null && value != null && value >= maxLength)
                    maxLength = null;
                    //maxLength = value + 1;
            }
        }
        private Int32? _maxLength = 254;
        public Int32? maxLength 
        {
            get { return _maxLength; }
            set
            {
                SetProperty(ref _maxLength, value);
                editPropsDataChanged();
                if (minLength != null && value != null && value <= minLength)
                    minLength = null;
                    //minLength = value - 1;
            }
        }

    }


    public class UDTIntItem : UDTBase
    {
        public UDTIntItem()
        {
            Type = "[int] NULL ";
            TypeName = UDTTypeName.Number;
            //Name = "";
            backgroundBrush = Brushes.LightGreen;
            sortOrder = "ccc";

            editProps = new UDTIntEditProps(editPropValidaionChanged);
        }

        public override void setSavedProps()
        {
            base.setSavedProps();
            savEditProps = new UDTIntEditProps(editProps);
        }

        public override UDTBase Clone()
        {
            UDTIntItem intItem = new UDTIntItem();
            intItem.Name = Name;
            intItem.editProps = new UDTIntEditProps(editProps);
            return intItem;
        }

        [XmlIgnoreAttribute]
        public override bool isModified
        {
            get
            {
                if(base.isModified) return true;
                if (savEditProps == null) return true;
                return !editProps.Equals(savEditProps);
            }
        }
    }

    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlInclude(typeof(UDTNumberPicker))]
    public class UDTIntEditProps : UDTBaseEditProps
    {
        private UDTIntEditProps() : base() { }

        public UDTIntEditProps(UDTBaseEditProps copyProps) : base()
        {
            UDTIntEditProps _copyProps = copyProps as UDTIntEditProps;
            defaultValue = _copyProps.defaultValue;
            maxValue = _copyProps.maxValue;
            minValue = _copyProps.minValue;
            required = _copyProps.required;
        }

        public override bool Equals(Object obj)
        {
            UDTIntEditProps savProps = obj as UDTIntEditProps;
            return (defaultValue == savProps.defaultValue &&
            maxValue == savProps.maxValue &&
            minValue == savProps.minValue &&
            required == savProps.required);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public UDTIntEditProps(Action editPropChanged) : base(editPropChanged) 
        {
            //defaultPicker = new UDTNumberPicker("Default Value", Int32.MaxValue, Int32.MinValue);
            //minPicker = new UDTNumberPicker("Min Value", Int32.MaxValue, Int32.MinValue, NumberPickerType.Integer, minChanged);
            //maxPicker = new UDTNumberPicker("Max Value", Int32.MaxValue, Int32.MinValue, NumberPickerType.Integer, maxChanged);
        }

        //public UDTNumberPicker defaultPicker { get; set; }
        //public UDTNumberPicker minPicker { get; set; }
        //public UDTNumberPicker maxPicker { get; set; }

        private Int32? _defaultValue = null;        
        public Int32? defaultValue 
        {
            get { return _defaultValue; }
            set { SetProperty(ref _defaultValue, value); }
        }
        private Int32? _minValue = null;
        public Int32? minValue
        {
            get { return _minValue; }
            set 
            { 
                SetProperty(ref _minValue, value);
                if (maxValue != null && value != null && value >= maxValue)
                    maxValue = value + 1;
            }
        }
        private Int32? _maxValue = null;
        public Int32? maxValue
        {
            get { return _maxValue; }
            set 
            { 
                SetProperty(ref _maxValue, value);
                if (minValue != null && value != null && value <= minValue)
                    minValue = value - 1;
            }
        }

    }

    public class UDTDecimalItem : UDTBase
    {
        public UDTDecimalItem()
        {
            Type = "[decimal](10, 5) NULL ";
            TypeName = UDTTypeName.Real;
            backgroundBrush = Brushes.LightSalmon;
            sortOrder = "ddd";

            editProps = new UDTDecimalEditProps(editPropValidaionChanged);
        }

        public override UDTBase Clone()
        {
            UDTDecimalItem dateItem = new UDTDecimalItem();
            dateItem.Name = Name;
            dateItem.editProps = new UDTDecimalEditProps(editProps);
            return dateItem;
        }

        public override void setSavedProps()
        {
            base.setSavedProps();
            savEditProps = new UDTDecimalEditProps(editProps);
        }

        [XmlIgnoreAttribute]
        public override bool isModified
        {
            get
            {
                if (base.isModified) return true;
                if (savEditProps == null) return true;
                return !editProps.Equals(savEditProps);
            }
        }
    }

    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlInclude(typeof(UDTNumberPicker))]
    public class UDTDecimalEditProps : UDTBaseEditProps
    {
        private UDTDecimalEditProps() : base() { }

        public UDTDecimalEditProps(Action editPropChanged) : base(editPropChanged)
        {
            //defaultPicker = new UDTNumberPicker("Default Value", Decimal.MaxValue, Decimal.MinValue, NumberPickerType.Decimal);
            //minPicker = new UDTNumberPicker("Min Value", Decimal.MaxValue, Decimal.MinValue, NumberPickerType.Decimal, minChanged);
            //maxPicker = new UDTNumberPicker("Max Value", Decimal.MaxValue, Decimal.MinValue, NumberPickerType.Decimal, maxChanged);
        }
        //public UDTNumberPicker defaultPicker { get; set; }
        //public UDTNumberPicker minPicker { get; set; }
        //public UDTNumberPicker maxPicker { get; set; }

        public UDTDecimalEditProps(UDTBaseEditProps copyProps) : base()
        {
            UDTDecimalEditProps _copyProps = copyProps as UDTDecimalEditProps;
            defaultValue = _copyProps.defaultValue;
            decimalFormat = _copyProps.decimalFormat;
            maxValue = _copyProps.maxValue;
            minValue = _copyProps.minValue;
            required = _copyProps.required;
        }

        public override bool Equals(Object obj)
        {
            UDTDecimalEditProps savProps = obj as UDTDecimalEditProps;
            return (defaultValue == savProps.defaultValue &&
            decimalFormat == savProps.decimalFormat &&
            maxValue == savProps.maxValue &&
            minValue == savProps.minValue &&
            required == savProps.required);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private Decimal? _defaultValue = null;
        public Decimal? defaultValue
        {
            get { return _defaultValue; }
            set
            {
                SetProperty(ref _defaultValue, value);
                editPropsDataChanged();
            }
        }
        private Decimal? _minValue = null;
        public Decimal? minValue
        {
            get { return _minValue; }
            set
            {
                SetProperty(ref _minValue, value);
                if (maxValue != null && value != null && value >= maxValue)
                    maxValue = value + 1;
                editPropsDataChanged();
            }
        }
        private Decimal? _maxValue = null;
        public Decimal? maxValue
        {
            get { return _maxValue; }
            set
            {
                SetProperty(ref _maxValue, value);
                if (minValue != null && value != null && value <= minValue)
                    minValue = value - 1;
                editPropsDataChanged();
            }
        }

        private DecimalFormatType _decimalFormat = DecimalFormatType.Decimal; 
        public DecimalFormatType decimalFormat 
        {
            get { return _decimalFormat; }
            set
            {
                SetProperty(ref _decimalFormat, value);
                editPropsDataChanged();
            }
        }
        public List<DecimalFormatType> formatList
        {
            get { return Enum.GetValues(typeof(DecimalFormatType)).Cast<DecimalFormatType>().ToList();}
        }
        
    }

    public class UDTDateItem : UDTBase
    {
        public UDTDateItem()
        {
            Type = "[datetime] NULL";
            TypeName = UDTTypeName.Date;
            //Name = "";
            backgroundBrush = Brushes.LightYellow;
            sortOrder = "eee";

            editProps = new UDTDateEditProps(editPropValidaionChanged);
        }

        public override void setSavedProps()
        {
            base.setSavedProps();
            savEditProps = new UDTDateEditProps(editProps);
        }

        public override UDTBase Clone()
        {
            UDTDateItem dateItem = new UDTDateItem();
            dateItem.Name = Name;
            dateItem.editProps = new UDTDateEditProps(editProps);
            return dateItem;
        }

        [XmlIgnoreAttribute]
        public override bool isModified
        {
            get
            {
                if (base.isModified) return true;
                if (savEditProps == null) return true;
                return !editProps.Equals(savEditProps);
            }
        }

    }

    //public enum DateDefault { CurrentDay, CurrentWeek, CurrentMonth, CurrentYear, None }
    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlInclude(typeof(DateTimeDefault))]
    public class UDTDateEditProps : UDTBaseEditProps
    {
        private UDTDateEditProps() : base() { }

        public UDTDateEditProps(Action editPropChanged) : base(editPropChanged)
        {
        }

        public UDTDateEditProps(UDTBaseEditProps copyProps) : base()
        {
            UDTDateEditProps _copyProps = copyProps as UDTDateEditProps;
            calandarTool = _copyProps.calandarTool;
            dateFormat = _copyProps.dateFormat;
            defaultDate = _copyProps.defaultDate;
            editBoxTool = _copyProps.editBoxTool;
            required = _copyProps.required;
        }


        public override bool Equals(Object obj)
        {
            UDTDateEditProps savProps = obj as UDTDateEditProps;
            return (calandarTool == savProps.calandarTool &&
            dateFormat == savProps.dateFormat &&
            defaultDate == savProps.defaultDate &&
            editBoxTool == savProps.editBoxTool &&
            required == savProps.required);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        private DateTimeDefault _defaultDate = DateTimeDefault.None;
        public DateTimeDefault defaultDate
        {
            get { return _defaultDate; }
            set
            {
                SetProperty(ref _defaultDate, value);
                editPropsDataChanged();
            }
        }


        public List<DateTimeDefault> defaultList
        {
            get { return Enum.GetValues(typeof(DateTimeDefault)).Cast<DateTimeDefault>().ToList(); }
        }

        private DateTimeFormat _dateFormat = DateTimeFormat.Date_Only;
        public DateTimeFormat dateFormat
        {
            get { return _dateFormat; }
            set
            {
                SetProperty(ref _dateFormat, value);
                editPropsDataChanged();
            }
        }
        public List<DateTimeFormat> formatList
        {
            get { return Enum.GetValues(typeof(DateTimeFormat)).Cast<DateTimeFormat>().ToList(); }
        }

        private Boolean _editBoxTool = true;
        public Boolean editBoxTool
        {
            get { return _editBoxTool; }
            set
            {
                SetProperty(ref _editBoxTool, value);
                editPropsDataChanged();
            }
        }
        private Boolean _calandarTool = false;
        public Boolean calandarTool
        {
            get { return _calandarTool; }
            set
            {
                SetProperty(ref _calandarTool, value);
                editPropsDataChanged();
            }
        }

        private bool _dateRangeNotUsed = true;
        public bool dateRangeNotUsed
        {
            get { return _dateRangeNotUsed; }
            set
            {
                SetProperty(ref _dateRangeNotUsed, value);
                editPropsDataChanged();
            }
        }

        //private DateTime? _minDate = DateTime.Parse("1/1/2000");
        private DateTime? _minDate = null;
        public DateTime? minDate
        {
            get { return _minDate; }
            set 
            { 
                SetProperty(ref _minDate, value);
                if (value >= maxDate)
                    maxDate = null;
                editPropsDataChanged();
            }
        }

        //private DateTime? _maxDate = DateTime.Parse("1/1/2020");
        private DateTime? _maxDate = null;
        public DateTime? maxDate
        {
            get { return _maxDate; }
            set 
            { 
                SetProperty(ref _maxDate, value);
                if (value <= minDate)
                    minDate = null;
                editPropsDataChanged();
            }
        }

    }

    public class UDTItemList
    {
        private static Collection<UDTBase> _itemList = null;
        public static Collection<UDTBase> ItemList
        { 
            get
            {
                if (_itemList == null)
                {
                    _itemList = new Collection<UDTBase>();
                    _itemList.Add(new UDTData());
                    _itemList.Add(new UDTTxtItem());
                    _itemList.Add(new UDTIntItem());
                    _itemList.Add(new UDTDecimalItem());
                    _itemList.Add(new UDTDateItem());
                }
                return _itemList;
            }
        }
    }
}
