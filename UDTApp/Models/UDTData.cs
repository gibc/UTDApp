using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using UDTApp.ViewModels;

namespace UDTApp.Models
{
    public enum UDTTypeName {DataBase, Group, Text, Real, Number, Date, Base}

    public class UDTData : UDTBase 
    {
        public UDTData()
        {
            //ChildData = new ObservableCollection<UDTBase>();
            tableData = new ObservableCollection<UDTData>();
            columnData = new ObservableCollection<UDTBase>();
            TypeName = UDTTypeName.Group;
            Name = TypeName.ToString();
            //backgroundBrush = Brushes.MistyRose;
            backgroundBrush = Brushes.White;
            ParentColumnNames = new List<string>();
            //buttonWidth = 60;
            //buttonHeight = 30;
            sortOrder = "zzz";

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

        private ObservableCollection<UDTData> _tableData;
        //[XmlIgnoreAttribute]
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
            MouseEnterCommand = new DelegateCommand<MouseEventArgs>(mouseEnter);
            MouseLeaveCommand = new DelegateCommand<MouseEventArgs>(mouseLeave);
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove, disable);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter, disable);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop, disable);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);
            PreviewDragEnterCommand = new DelegateCommand<DragEventArgs>(previewDragEnter);

            SaveNameCommand = new DelegateCommand<EventArgs>(saveName, canSaveName);
            DeleteItemCommand = new DelegateCommand<EventArgs>(deleteItem);
            //PopupOpenCommand = new DelegateCommand<EventArgs>(popupOpen, disable);
            //PopupOpenCommand = new DelegateCommand<EventArgs>(popupOpen);
            PopupLoadCommand = new DelegateCommand<EventArgs>(popupLoad);
            //MouseLeftButtonUpCommand = new DelegateCommand<EventArgs>(buttonRelease);
            //SizeChangedCommand = new DelegateCommand<SizeChangedEventArgs>(sizeChange);
            SizeChangedCommand = new DelegateCommand<RoutedEventArgs>(sizeChange);
            GroupBoxLoadedCommand = new DelegateCommand<RoutedEventArgs>(groupBoxLoaded);
            ItemNameEditGotFocusCommand = new DelegateCommand<RoutedEventArgs>(itemNameEditGotFocus);
            ItemNameEditLostFocusCommand = new DelegateCommand<RoutedEventArgs>(itemNameEditLostFocus);

            objId = Guid.NewGuid();
            backgroundBrush = Brushes.Black;
            //buttonWidth = 50;
            buttonWidth = 35;
            //buttonHeight = 40;
            buttonHeight = 12;
            sortOrder = "zzz";
        }

        //public string currentError
        //{
        //    get 
        //    {
        //        Dictionary<string, List<string>> errDic = GetAllErrors();
        //        return errDic["Name"][0];
        //    }
        //}

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
            //return !AnyErrors;
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
        public UDTBaseEditProps editProps
        {
            get { return _editProps; }
            set { SetProperty(ref _editProps, value); }
        }

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

        private UDTData _parentObj = null;
        [XmlIgnoreAttribute]
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
                DeleteItemCommand.RaiseCanExecuteChanged();

                //Debug.Write(string.Format("AnyErrors after CanExChanged {0}\r", value));

            }
        }

        private void SetAnyErrorAll(UDTData dataItem, bool value)
        {
            Debug.Write(string.Format(">>>> Enter SetAnyErrorAll value {0} Name: {1}\r", value, dataItem.Name));
            dataItem.AnyErrors = value;
            dataItem.EditBoxEnabled = !value;
            //foreach (UDTBase obj in dataItem.ChildData)
            foreach (UDTData obj in dataItem.tableData)
            {
                //if (obj.GetType() == typeof(UDTData))
                    SetAnyErrorAll(obj as UDTData, value);
                //else
                //{ 
                    obj.AnyErrors = value;
                    obj.EditBoxEnabled = !value;
                //}
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

        // used to show parent of group name without error adorner
        private string _displayName = "";
        public string DisplayName 
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private string _name = "";
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        [CustomValidation(typeof(UDTBase), "CheckDuplicateColumnName")]
        [CustomValidation(typeof(UDTBase), "CheckEmptyTable")]
        public string Name
        {
            get { return _name; }
            set 
            {
                DisplayName = value;
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

        //private void deleteItem(EventArgs eventArgs)
        //{
        //    PopUpOpen = false;
        //    removeItem(MasterGroup, this);
        //    MasterGroup.dataChanged();
        //}

        private void removeItem(UDTData data, UDTBase item)
        {
            //data.ChildData.Remove(item);
            //foreach(UDTBase obj in data.ChildData)
            if(item.GetType() == typeof(UDTData))
                data.tableData.Remove(item as UDTData);
            else
                data.columnData.Remove(item);
            foreach (UDTData obj in data.tableData)
            {
                //if(obj.GetType() == typeof(UDTData))
                    removeItem(obj as UDTData, item);
            }
        }

        private void deleteItem(EventArgs eventArgs)
        {
            //PopUpOpen = true;
            removeItem(MasterGroup, this);
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
            //Keyboard.Focus(firstButton)
            //var pu = eventArgs.Source;
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
                //PageZeroViewModel.viewModel.currentTextProps = null;
            }
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
                            udtData.Name = getUniqueGroupName(MasterGroup);
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
                    //col.Add(udtItem);

                    //UDTData utdData = this as UDTData;
                    //List<UDTBase> childList = utdData.ChildData.ToList();
                    //childList.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
                    //utdData.ChildData = new ObservableCollection<UDTBase>(childList);


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

        private void findGroupName(UDTData udtData, string name, ref bool retVal)
        {
            if (udtData.Name == name)
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
            if (txtBox.IsMouseOver && HasErrors)
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

                if (dataObj.columnData.Count <= 0 /*&& dataObj.tableData.Count <= 0*/)
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Group item must include at least one data item.");
            }

            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckDuplicateColumnName(string name, ValidationContext context)
        {
            UDTBase dataObj = context.ObjectInstance as UDTBase;
            if(dataObj != null && dataObj.parentObj != null && dataObj.GetType() != typeof(UDTData))
            {
                //foreach (UDTBase obj in dataObj.parentObj.ChildData)
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
            //foreach (UDTBase item in dataItem.ChildData)
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

    //public class UDTParentColumn
    //{
    //    public string ParentColumnName { get; set; }
    //}

    public class UDTTxtItem : UDTBase 
    {
        public UDTTxtItem()
        { 
            Type = "[varchar](255) NULL ";
            TypeName = UDTTypeName.Text;
            Name = TypeName.ToString();
            backgroundBrush = Brushes.LightBlue;
            sortOrder = "bbb";

            editProps = new UDTTextEditProps();
        }

        //private UDTTextEditProps _editProps = null;
        //public UDTTextEditProps editProps
        //{
        //    get { return _editProps; }
        //    set { SetProperty(ref _editProps, value); }
        //}
    }

    public class UDTBaseEditProps : BindableBase
    {
        public UDTBaseEditProps() { }

        private bool _required = true;
        public bool required
        {
            get { return _required; }
            set { SetProperty(ref _required, value); }
        }
    }

    public class UDTTextEditProps : UDTBaseEditProps
    {
        public UDTTextEditProps(){}

        private string _defaultText = "";
        public string defaultText
        {
            get { return _defaultText; }
            set { SetProperty(ref _defaultText, value); }
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

            editProps = new UDTIntEditProps();
        }    
    }

    public class UDTIntEditProps : UDTBaseEditProps
    {
        public UDTIntEditProps() { }
    }

    public class UDTDecimalItem : UDTBase//, UDTItem
    {
        public UDTDecimalItem()
        {
            Type = "[decimal](10, 5) NULL ";
            TypeName = UDTTypeName.Real;
            //Name = "";
            backgroundBrush = Brushes.LightSalmon;
            sortOrder = "ddd";

            editProps = new UDTDecimalEditProps();
        }                
 

    }

    public class UDTDecimalEditProps : UDTBaseEditProps
    {
        public UDTDecimalEditProps() { }
    }

    public class UDTDateItem : UDTBase//, UDTItem
    {
        public UDTDateItem()
        {
            Type = "[datetime] NULL";
            TypeName = UDTTypeName.Date;
            //Name = "";
            backgroundBrush = Brushes.LightYellow;
            sortOrder = "eee";

            editProps = new UDTDateEditProps();
        }                
 
    }

    public class UDTDateEditProps : UDTBaseEditProps
    {
        public UDTDateEditProps() { }
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
