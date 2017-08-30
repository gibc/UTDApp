using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using UDTApp.ViewModels;

namespace UDTApp.Models
{
    //public class UDTTableView : ValidatableBindableBase
    //{
    //    //public UDTTableView(ObservableCollection<UDTBase> itemCol)
    //    //{
    //    //    columnData = new ObservableCollection<UDTBase>();
    //    //    tableData = new ObservableCollection<UDTData>();
    //    //    foreach(UDTBase item in itemCol)
    //    //    {
    //    //        if (item.GetType() == typeof(UDTData))
    //    //            tableData.Add(item as UDTData);
    //    //        else
    //    //            columnData.Add(item);
    //    //    }
    //    //}

    //    private ObservableCollection<UDTBase> _columnData;
    //    public ObservableCollection<UDTBase> columnData 
    //    {
    //        get { return _columnData; } 
    //        set
    //        {
    //            SetProperty(ref _columnData, value);
    //        }
    //    }

    //    private ObservableCollection<UDTData> _tableData;         
    //    public ObservableCollection<UDTData> tableData 
    //    {
    //        get { return _tableData; }
    //        set
    //        {
    //            SetProperty(ref _tableData, value);
    //        }
    //    } 
    //}

    public class UDTData : UDTBase //, UDTItem
    {
        //[XmlIgnoreAttribute]
        //public DelegateCommand<EventArgs> SizeChangedCommand { get; set; }

        public UDTData()
        {
            ChildData = new ObservableCollection<UDTBase>();
            tableData = new ObservableCollection<UDTData>();
            columnData = new ObservableCollection<UDTBase>();
            TypeName = "Group";
            Name = TypeName;
            //backgroundBrush = Brushes.MistyRose;
            backgroundBrush = Brushes.White;
            ParentColumnNames = new List<string>();
            //buttonWidth = 60;
            //buttonHeight = 30;
            sortOrder = "zzz";

        }
        //private void sizeChange(EventArgs eventArgs)
        //{

        //}

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



        override public bool AllowDrop
        {
            get { return !ToolBoxItem; }
        }
        // on write child data insert or update UDTRelation record
        //   where 
        //     parent and child names are parent and child colleciton name
        //     child column name is pareent collection name 
        // on read UDT data set do select * from DataRelation where ParentName = [Name]
        //  for ecah relation
        //     create UDTData collection
        //     select * from [ChildName] where [ChildColumnName] = [Id of this record]
        //        for each record add record to collection
        // To create CRUD UI
        //   find UDTData collectons with ChildData.count = 0;
        //     create display and edit page for each UDTData item
        private ObservableCollection<UDTBase> _childData;
        public ObservableCollection<UDTBase> ChildData 
        { 
            get
            {
                return _childData;
            }
            set 
            {
                SetProperty(ref _childData, value);
            }
        }

        private ObservableCollection<UDTData> _tableData;
        [XmlIgnoreAttribute]
        public ObservableCollection<UDTData> tableData
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

        //private Thickness _tableDataMargin = new Thickness(30, 0, 0, 0);        
        //public Thickness tableDataMargin
        //{
        //    get 
        //    {
        //        return _tableDataMargin;
        //    }
        //    set { SetProperty(ref _tableDataMargin, value); }
        //}

        private ObservableCollection<UDTBase> _columnData;
        [XmlIgnoreAttribute]
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
        // add this if we want group children to have more than one parent
        //public ObservableCollection<UDTParentColumn> ParentColumnNames { get; set; }
        public List<string> ParentColumnNames { get; set; }
    }

    [XmlInclude(typeof(UDTTxtItem))]
    [XmlInclude(typeof(UDTIntItem))]
    [XmlInclude(typeof(UDTData))]
    [XmlInclude(typeof(UDTDateItem))]
    [XmlInclude(typeof(UDTDecimalItem))]
    [XmlRoot("UDTBase"), XmlType("UDTBase")]
    public class UDTBase : ValidatableBindableBase//: ValidatableBindableBase, INotifyDataErrorInfo
    {
        public UDTBase()
        {
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove, disable);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter, disable);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop, disable);
            //DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver, disable);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);
            PreviewDragEnterCommand = new DelegateCommand<DragEventArgs>(previewDragEnter);

            SaveNameCommand = new DelegateCommand<EventArgs>(saveName, canSaveName);
            DeleteItemCommand = new DelegateCommand<EventArgs>(deleteItem);
            //PopupOpenCommand = new DelegateCommand<EventArgs>(popupOpen, disable);
            PopupOpenCommand = new DelegateCommand<EventArgs>(popupOpen);
            PopupLoadCommand = new DelegateCommand<EventArgs>(popupLoad);
            //MouseLeftButtonUpCommand = new DelegateCommand<EventArgs>(buttonRelease);
            //SizeChangedCommand = new DelegateCommand<SizeChangedEventArgs>(sizeChange);
            SizeChangedCommand = new DelegateCommand<RoutedEventArgs>(sizeChange);

            objId = Guid.NewGuid();
            backgroundBrush = Brushes.Black;
            //buttonWidth = 50;
            buttonWidth = 35;
            //buttonHeight = 40;
            buttonHeight = 12;
            sortOrder = "zzz";
        }

        private bool disable(EventArgs eventArgs) 
        {
            if (ToolBoxItem) return true;
            if (parentObj == null) return true;
            //if (inDrag) return true;
            //if(MasterGroup != null)
            //{
            //    Debug.Write(string.Format("In disable !AnyError {0}\r", !MasterGroup.AnyErrors));
            //    return !MasterGroup.AnyErrors;
            //}
            return !AnyErrors;
            //return true;
        }
        
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
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> PopupOpenCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> PopupLoadCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<DragEventArgs> PreviewDragEnterCommand { get; set; }
        [XmlIgnoreAttribute]
        //public DelegateCommand<SizeChangedEventArgs> SizeChangedCommand { get; set; }
        public DelegateCommand<RoutedEventArgs> SizeChangedCommand { get; set; }

        


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

        [XmlIgnoreAttribute]
        private UDTData _parentObj = null;       
        public UDTData parentObj 
        {
            get { return _parentObj; }
            set { SetProperty(ref _parentObj, value); }
        }
        private bool newDrop = false;

        public UDTData MasterGroup 
        { 
            get
            {
                if (UDTXml.UDTXmlData.SchemaData.Count <= 0) return null;
                return UDTXml.UDTXmlData.SchemaData[0] as UDTData;
            }
        }

        private bool _anyErrors = true;
        public bool AnyErrors
        {
            get { return _anyErrors; }
            set 
            {
                //if (setAnyError != null)
                //    setAnyError(value); 
                SetProperty(ref _anyErrors, value);
                MouseMoveCommand.RaiseCanExecuteChanged();
                DragEnterCommand.RaiseCanExecuteChanged();
                DragDropCommand.RaiseCanExecuteChanged();
                DragOverCommand.RaiseCanExecuteChanged();
                PopupOpenCommand.RaiseCanExecuteChanged();

                //Debug.Write(string.Format("AnyErrors after CanExChanged {0}\r", value));

            }
        }

        private void SetAnyErrorAll(UDTData dataItem, bool value)
        {
            Debug.Write(string.Format(">>>> Enter SetAnyErrorAll value {0} Name: {1}\r", value, dataItem.Name));
            dataItem.AnyErrors = value;
            dataItem.EditBoxEnabled = !value;
            foreach(UDTBase obj in dataItem.ChildData)
            {
                if (obj.GetType() == typeof(UDTData))
                    SetAnyErrorAll(obj as UDTData, value);
                else
                { 
                    obj.AnyErrors = value;
                    obj.EditBoxEnabled = !value;
                }
            }
            //foreach (UDTBase obj in dataItem.columnData)
            //{
            //    //obj.AnyErrors = value;
            //    //obj.EditBoxEnabled = !value;
            //}            
        }

        [XmlIgnoreAttribute]
        public Action<bool> setAnyError { get; set; }

        private bool _popUpOpen = false;
        public bool PopUpOpen 
        {
            get 
            { 
                //if (HasErrors)
                //{
                //    _popUpOpen = true;
                //}
                //Debug.Write(string.Format("Get PopUpOpen {0}\r", _popUpOpen));

                return _popUpOpen;
            }
            set 
            { 
                SetProperty(ref _popUpOpen, value);
                //Debug.Write(string.Format("Set PopUpOpen {0}\r", value));

                //if (MasterGroup != null && MasterGroup.setAnyError != null && !inDrag)
                //{
                //    MasterGroup.setAnyError(!value);
                //    MasterGroup.setAnyError(value);
                //}
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
                //if (ToolBoxItem)
                //    return 0;
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
                //if (ToolBoxItem)
                //    _editBoxEnabled = false;
                //else if (newDrop)
                //    _editBoxEnabled = true;
                //else 
                //    _editBoxEnabled = !AnyErrors;
                SetProperty(ref _editBoxEnabled, value);
            }

        }

        //private Thickness _buttonWrapPanel = new Thickness(0, 0, 0, 0);
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

        private string _name = "";
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        [CustomValidation(typeof(UDTBase), "CheckDuplicateColumnName")]
        public string Name
        {
            get { return _name; }
            set 
            {
                if (_name != value && MasterGroup != null) MasterGroup.dataChanged();
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

        private string _typeName = "";         
        public string TypeName 
        { 
            get
            {
                return _typeName;
            }
            set { SetProperty(ref _typeName, value); }
        }

        private bool _toolBoxItem = true;
        public bool ToolBoxItem 
        { 
            get {return _toolBoxItem; }
            set { _toolBoxItem = value; SchemaItem = !value; } 
        }

        public bool SchemaItem { get; set; }

        private bool _allowDrop = false;
        virtual public bool AllowDrop
        {
            //get { return _allowDrop; }
            get { return true; }
            set { _allowDrop = value; }
        }

        //private bool _isReadOnly = false;
        virtual public bool IsReadOnly
        {
            get { return ToolBoxItem; }
            //set { _isReadOnly = value; }
        }

        private void saveName(EventArgs eventArgs)
        {
            PopUpOpen = false;
        }

        private void deleteItem(EventArgs eventArgs)
        {
            PopUpOpen = false;
            removeItem(MasterGroup, this);
            MasterGroup.dataChanged();
        }

        private void removeItem(UDTData data, UDTBase item)
        {
            data.ChildData.Remove(item);
            foreach(UDTBase obj in data.ChildData)
            {
                if(obj.GetType() == typeof(UDTData))
                    removeItem(obj as UDTData, item);
            }
        }

        private void popupOpen(EventArgs eventArgs)
        {
            //PopUpOpen = true;
            removeItem(MasterGroup, this);
        }

        private void popupLoad(EventArgs eventArgs)
        {
            //Keyboard.Focus(firstButton)
            //var pu = eventArgs.Source;
            RoutedEventArgs args = eventArgs as RoutedEventArgs;
            Popup pu = args.Source as Popup;
            TextBox tb = pu.FindName("NameBox") as TextBox;
            var fe = Keyboard.Focus(tb);
        }

        // change table data items control margin when item added to collection
        static private int tableDataIndent = 25;
        static private int tableDataTopMargin = -15;
        private void sizeChange(RoutedEventArgs e)
        {
            //return;

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

                if(udtItem.Name == "") udtItem.Name = "<Name>";

                UDTData udtData;
                //if (udtItem != null && col != null && !col.Contains(udtItem))
                if (udtItem != null )
                {
                    UDTData parent = this as UDTData;
                    udtItem.parentObj = parent;
                    //udtItem.Name = parent.Name + ":" + udtItem.Name;
                    if (udtItem.GetType() == typeof(UDTData))
                    {
                        udtData = udtItem as UDTData;
                        udtData.ParentColumnNames.Add(this.Name);
                        if (!parent.tableData.Contains(udtData))
                            parent.tableData.Add(udtData);
                    }
                    else
                    {
                        //udtItem.AnyErrors = false;
                        if (!parent.columnData.Contains(udtItem))
                            parent.columnData.Add(udtItem);
                        //parent.ChildData.Add(udtItem);

                    }
                        
                    udtItem.newDrop = true;
                    //col.Add(udtItem);

                    UDTData utdData = this as UDTData;
                    List<UDTBase> childList = utdData.ChildData.ToList();
                    childList.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
                    utdData.ChildData = new ObservableCollection<UDTBase>(childList);


                    MasterGroup.dataChanged();
                }
                dragArgs.Handled = true;
                _currentItem = null;
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


            // Check that the data being dragged is a file
            if (getItemFromDragArgs(dragArgs) != null)
            {

                dragArgs.Effects = DragDropEffects.Copy;

            }
            else
                dragArgs.Effects = DragDropEffects.None;
        }

        private bool inMove = false;
        private static bool inDrag = false;
        private void mouseMove(MouseEventArgs data)
        {
            Button btn = data.Source as Button;
            TextBox box = data.Source as TextBox;
            //ObservableCollection<UDTBase> col = Ex.GetSecurityId(btn);

            if ((btn != null || box != null) && data.LeftButton == MouseButtonState.Pressed && !inMove)
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

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckDuplicateColumnName(string name, ValidationContext context)
        {
            UDTBase dataObj = context.ObjectInstance as UDTBase;
            if(dataObj != null && dataObj.parentObj != null && dataObj.GetType() != typeof(UDTData))
            {
                foreach(UDTBase obj in dataObj.parentObj.ChildData)
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
            foreach (UDTBase item in dataItem.ChildData)
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

    //public class UDTParentColumn
    //{
    //    public string ParentColumnName { get; set; }
    //}

    public class UDTTxtItem : UDTBase //, UDTItem
    {
        public UDTTxtItem()
        { 
            Type = "[varchar](255) NULL ";
            TypeName = "Text";
            //TypeName = "T\ne\nx\nt";
            //Name = "";
            Name = TypeName;
            backgroundBrush = Brushes.LightBlue;
            sortOrder = "bbb";
        }

    }

    public class UDTIntItem : UDTBase//, UDTItem
    {
        public UDTIntItem()
        {
            Type = "[int] NULL ";
            TypeName = "Number";
            //Name = "";
            backgroundBrush = Brushes.LightGreen;
            sortOrder = "ccc";
        }    


    }
    public class UDTDecimalItem : UDTBase//, UDTItem
    {
        public UDTDecimalItem()
        {
            Type = "[decimal](10, 5) NULL ";
            TypeName = "Real";
            //Name = "";
            backgroundBrush = Brushes.LightSalmon;
            sortOrder = "ddd";
        }                
 

    }
    public class UDTDateItem : UDTBase//, UDTItem
    {
        public UDTDateItem()
        {
            Type = "[datetime] NULL";
            TypeName = "Date";
            //Name = "";
            backgroundBrush = Brushes.LightYellow;
            sortOrder = "eee";
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
