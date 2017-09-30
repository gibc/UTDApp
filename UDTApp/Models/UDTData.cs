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
using UDTApp.ViewModels.DataEntryControls;

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

            ErrorsChanged += OnErrorsChanged;
        }

        public void editPropValidaionChanged()
        {
            if(!HasErrors && editProps.HasErrors)
            {
                List<string> errLst = new List<string>();
                errLst.Add("<dummy>");
                SetErrors(() => this.Name, errLst);
            }
            else if(!editProps.HasErrors && HasErrors)
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

            if(UDTXml.UDTXmlData.SchemaData.Count > 0)
            { 
                UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                master.validationChanged();
            }
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
        //[XmlIgnoreAttribute]
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

        [XmlIgnoreAttribute]
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

        public void SetAnyErrorAll(UDTData dataItem, bool value)
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

            editProps = new UDTTextEditProps(editPropValidaionChanged);
            //editProps.parentItem = this;
        }

        //private UDTTextEditProps _editProps = null;
        //public UDTTextEditProps editProps
        //{
        //    get { return _editProps; }
        //    set { SetProperty(ref _editProps, value); }
        //}
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


        private bool _required = false;
        public bool required
        {
            get { return _required; }
            set { SetProperty(ref _required, value); }
        }
    }

    [XmlInclude(typeof(UDTBaseEditProps))]
    public class UDTTextEditProps : UDTBaseEditProps
    {
        private UDTTextEditProps() : base() { }

        public UDTTextEditProps(Action editPropChanged) : base(editPropChanged)
        {
            minPicker = new UDTNumberPicker("Min Text Length", 254, 0, NumberPickerType.Integer, minChanged);
            maxPicker = new UDTNumberPicker("Max Text", 255, 1, NumberPickerType.Integer, maxChanged);
            minPicker.number = 0;
            maxPicker.number = 255;
        }


        private string _defaultText = "";
        public string defaultText
        {
            get { return _defaultText; }
            set 
            { 
                SetProperty(ref _defaultText, value); 
            }
        }

        public UDTNumberPicker minPicker { get; set; }
        public UDTNumberPicker maxPicker { get; set; }

        private void minChanged(decimal? newVal)
        {
            if (newVal >= maxPicker.number)
            {
                maxPicker.number = newVal + 1;
            }
        }

        private void maxChanged(decimal? newVal)
        {
            if (newVal <= minPicker.number)
            {
                minPicker.number = newVal - 1;
            }
        }

        //private string _maxLength = "255";
        ////[RegularExpression(@"^[0-9]{1,3}$", ErrorMessage = "error Message ")]
        ////[RegularExpression(@"^[0-9]", ErrorMessage = "error Message ")]
        ////[Required(ErrorMessage = "Max Length is required.")]
        ////[Range( 1, 255, ErrorMessage = "Max Length must a number between 1 and 255")]
        ////[CustomValidation(typeof(UDTTextEditProps), "ACheckValidNumber")]
        //[CustomValidation(typeof(UDTTextEditProps), "CheckMaxMore")]
        //public string maxLength
        //{
        //    get { return _maxLength; }
        //    set 
        //    { 
        //        SetProperty(ref _maxLength, value);
        //    }
        //}

        //public bool IsValidNumber(string name)
        //{
        //    return !(name.Length > 3 || name.Length <= 0 ||
        //            !name.All(char.IsDigit) || Int32.Parse(name) > 255);
        //}

        //public static System.ComponentModel.DataAnnotations.ValidationResult CheckMaxMore(string name, ValidationContext context)
        //{
        //    UDTTextEditProps dataObj = context.ObjectInstance as UDTTextEditProps;
            
        //    if (dataObj != null)
        //    {

        //        if (!dataObj.IsValidNumber(name))
        //        {
        //            string msgName = "Max Length";
        //            if (context.DisplayName == "minLength")
        //                msgName = "Min Length";
        //            string msg = string.Format("{0} must a number beween 1 and 255", msgName);
        //            return new System.ComponentModel.DataAnnotations.ValidationResult(msg);
        //        }

        //        int minVal = Int32.MinValue; 
        //        Int32.TryParse(dataObj.minLength, out minVal);
        //        int maxVal = Int32.MaxValue;
        //        Int32.TryParse(dataObj.maxLength, out maxVal);
        //        if (maxVal <= minVal || minVal == Int32.MinValue || maxVal == Int32.MaxValue)
        //        {
        //            return new System.ComponentModel.DataAnnotations.ValidationResult("Min Length must be less than Max Lenght");
        //        }
        //        if (dataObj.HasErrors)
        //        {
        //            if (dataObj.IsValidNumber(dataObj.maxLength))
        //                dataObj.SetErrors(() => dataObj.maxLength, new List<string>());
        //            if (dataObj.IsValidNumber(dataObj.minLength))
        //                dataObj.SetErrors(() => dataObj.minLength, new List<string>());
        //        }
        //    }
        //    return System.ComponentModel.DataAnnotations.ValidationResult.Success;
        //}

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
    }

    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlInclude(typeof(UDTNumberPicker))]
    public class UDTIntEditProps : UDTBaseEditProps
    {
        private UDTIntEditProps() : base() { }

        public UDTIntEditProps(Action editPropChanged) : base(editPropChanged) 
        {
            defaultPicker = new UDTNumberPicker("Default Value", Int32.MaxValue, Int32.MinValue);
            minPicker = new UDTNumberPicker("Min Value", Int32.MaxValue, Int32.MinValue, NumberPickerType.Integer, minChanged);
            maxPicker = new UDTNumberPicker("Max Value", Int32.MaxValue, Int32.MinValue, NumberPickerType.Integer, maxChanged);
        }

        public UDTNumberPicker defaultPicker { get; set; }
        public UDTNumberPicker minPicker { get; set; }
        public UDTNumberPicker maxPicker { get; set; }

        private void minChanged(decimal? newVal) 
        { 
            if(newVal >= maxPicker.number)
            {
                maxPicker.number = newVal + 1;
            }
        }

        private void maxChanged(decimal? newVal) 
        {
            if (newVal <= minPicker.number)
            {
                minPicker.number = newVal - 1;
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


    }

    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlInclude(typeof(UDTNumberPicker))]
    public class UDTDecimalEditProps : UDTBaseEditProps
    {
        private UDTDecimalEditProps() : base() { }

        public UDTDecimalEditProps(Action editPropChanged) : base(editPropChanged)
        {
            defaultPicker = new UDTNumberPicker("Default Value", Decimal.MaxValue, Decimal.MinValue, NumberPickerType.Decimal);
            minPicker = new UDTNumberPicker("Min Value", Decimal.MaxValue, Decimal.MinValue, NumberPickerType.Decimal, minChanged);
            maxPicker = new UDTNumberPicker("Max Value", Decimal.MaxValue, Decimal.MinValue, NumberPickerType.Decimal, maxChanged);
        }
        public UDTNumberPicker defaultPicker { get; set; }
        public UDTNumberPicker minPicker { get; set; }
        public UDTNumberPicker maxPicker { get; set; }

        //private void defaultChanged(decimal? newVal)
        //{
        //    if (newVal == null) defaultPicker.number = 0;
        //}
        private void minChanged(decimal? newVal)
        {
            if (newVal >= maxPicker.number)
            {
                maxPicker.number = newVal + 1;
            }
        }

        private void maxChanged(decimal? newVal)
        {
            if (newVal <= minPicker.number)
            {
                minPicker.number = newVal - 1;
            }
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
 
    }

    public enum DateDefault { CurrentDay, CurrentWeek, CurrentMonth, CurrentYear, None }
    [XmlInclude(typeof(UDTBaseEditProps))]
    [XmlInclude(typeof(DateDefault))]
    public class UDTDateEditProps : UDTBaseEditProps
    {
        private UDTDateEditProps() : base() { }

        public UDTDateEditProps(Action editPropChanged) : base(editPropChanged)
        { 
            defaultList.Add(DateDefault.CurrentDay);
            defaultList.Add(DateDefault.CurrentWeek);
            defaultList.Add(DateDefault.CurrentMonth);
            defaultList.Add(DateDefault.CurrentYear);
            defaultList.Add(DateDefault.None);
        }

        private DateDefault _defaultDate = DateDefault.None;        
        public DateDefault defaultDate
        {
            get { return _defaultDate; }
            set { SetProperty(ref _defaultDate, value); }
        }
        private List<DateDefault> _defaultList = new List<DateDefault>();
        public List<DateDefault> defaultList
        {
            get { return _defaultList; }
            set { SetProperty(ref _defaultList, value); }
        }

        private bool _dateRangeNotUsed = true;
        public bool dateRangeNotUsed
        {
            get { return _dateRangeNotUsed; }
            set { SetProperty(ref _dateRangeNotUsed, value); }
        }

        private DateTime _minDate = DateTime.Parse("1/1/2000");
        public DateTime minDate
        {
            get { return _minDate; }
            set { SetProperty(ref _minDate, value); }
        }

        private DateTime _maxDate = DateTime.Parse("1/1/2020");
        public DateTime maxDate
        {
            get { return _maxDate; }
            set { SetProperty(ref _maxDate, value); }
        }

    }

    //public enum NumberPickerType { Integer, Decimal}
    //public class UDTNumberPicker : ValidatableBindableBase
    //{
    //    [XmlIgnoreAttribute]
    //    public DelegateCommand<EventArgs> UpCommand { get; set; }
    //    [XmlIgnoreAttribute]
    //    public DelegateCommand<EventArgs> DownCommand { get; set; }
    //    [XmlIgnoreAttribute]
    //    public DelegateCommand<EventArgs> FastUpCommand { get; set; }
    //    [XmlIgnoreAttribute]
    //    public DelegateCommand<EventArgs> FastDownCommand { get; set; }

    //    private UDTNumberPicker() { }

    //    public UDTNumberPicker(string _name, decimal _numMax, decimal _numMin, 
    //        NumberPickerType _pickerType = NumberPickerType.Integer,           
    //        Action<decimal> _numberChanged = null)
    //    {
    //        name = _name;
    //        numberChanged = _numberChanged;
    //        numMax = _numMax;
    //        numMin = _numMin;
    //        pickerType = _pickerType;
    //        UpCommand = new DelegateCommand<EventArgs>(upBtnClk);
    //        DownCommand = new DelegateCommand<EventArgs>(downBtnClk);
    //        FastUpCommand = new DelegateCommand<EventArgs>(fastUpBtnClk);
    //        FastDownCommand = new DelegateCommand<EventArgs>(fastDownBtnClk);
    //    }

    //    public string name { get; set; }

    //    private decimal _number = 0;
    //    public decimal number
    //    {
    //        get { return _number; }
    //        set
    //        {
    //            SetProperty(ref _number, value);
    //            txtNumber = getNumText(number);
    //            if (numberChanged != null) numberChanged(_number);
    //        }
    //    }

    //    private string getNumText(decimal num)
    //    {
    //        string numTxt = "";
    //        if (pickerType == NumberPickerType.Integer)
    //            numTxt = string.Format("{0:n0}", number);
    //        else if (pickerType == NumberPickerType.Decimal)
    //        {
    //            numTxt = string.Format("{0}", number);
    //            if (_txtNumber.Length > 0 && _txtNumber.Last() == '.')
    //                return numTxt + '.';
    //            else return numTxt;
    //        }
    //        return numTxt;
    //    }

    //    private string _txtNumber = "";
    //    public string txtNumber
    //    {
    //        get 
    //        {
    //            if (!textParsed) return _txtNumber;
    //            return getNumText(number);
    //        }
    //        set
    //        {
    //            SetProperty(ref _txtNumber, filterDigits(value));
    //            textParsed = false;
    //            if (!containsOnlyZeros(_txtNumber))
    //            {
    //                textParsed = true;
    //                parseNumber(_txtNumber);
    //                if (numberChanged != null) numberChanged(_number);
    //            }
    //        }
    //    }

    //    private bool textParsed = false;
    //    private bool containsOnlyZeros(string val)
    //    {
    //        bool retVal = true;
    //        foreach(char c in val)
    //        {
    //            if (!(c == '0' || c == '.' || c == '-'))
    //            {
    //                return false;
    //            }
    //        }
    //        return retVal;
    //    }

    //    private void parseNumber(string txtNum)
    //    {
    //        if(pickerType == NumberPickerType.Integer)
    //        {
    //            int num;
    //            if (Int32.TryParse(txtNum, out num))
    //                _number = num;
    //            else if (txtNum[0] == '-')
    //            {
    //                _number = numMin;
    //            }
    //            else
    //            {
    //                _number = numMax;
    //            }
    //        }
    //        else if(pickerType == NumberPickerType.Decimal)
    //        {
    //            if (Decimal.TryParse(txtNum, out _number)) return;
    //            else if (txtNum[0] == '-')
    //            {
    //                _number = numMin;
    //            }
    //            else
    //            {
    //                _number = numMax;
    //            }
    //        }
    //        if (_number > numMax)
    //            _number = numMax;
    //        else if (_number < numMin)
    //            _number = numMin;
    //    }

    //    private string filterDigits(string txt)
    //    {
    //        string outTxt = "";
    //        if (string.IsNullOrEmpty(txt))
    //            return "0";
    //        if (pickerType == NumberPickerType.Integer)
    //        {
    //            foreach (char c in txt)
    //            {
    //                if (txt.First() == c)
    //                {
    //                    if (Char.IsDigit(c) || c == '+' || c == '-')
    //                        outTxt += c;
    //                }

    //                else if (Char.IsDigit(c))
    //                {
    //                    outTxt += c;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            bool haveDecPt = false;
    //            foreach (char c in txt)
    //            {
    //                if (txt.First() == c)
    //                {
    //                    if (Char.IsDigit(c) || c == '+' || c == '-' || c == '.')
    //                        outTxt += c;
    //                }
    //                else if (Char.IsDigit(c))
    //                {
    //                    outTxt += c;
    //                }
    //                else if (c == '.' && !haveDecPt)
    //                {
    //                    haveDecPt = true;
    //                    outTxt += c;
    //                }
    //            }
    //        }
    //        if (string.IsNullOrEmpty(outTxt))
    //            return "0";
    //        return outTxt;
    //    }

    //    private bool _notUsed = true;
    //    public bool notUsed
    //    {
    //        get { return _notUsed; }
    //        set
    //        {
    //            SetProperty(ref _notUsed, value);
    //        }
    //    }

    //    [XmlIgnoreAttribute]
    //    public Action<decimal> numberChanged { get; set; }

    //    private void upBtnClk(EventArgs args)
    //    {
    //        if(number < numMax)
    //        { 
    //            number++;
    //        }
    //    }
    //    private void downBtnClk(EventArgs args)
    //    {
    //        if(number > numMin)
    //        {
    //            number--;
    //        }
    //    }
    //    private void fastUpBtnClk(EventArgs args)
    //    {
    //        if (number < numMax - 100)
    //        { 
    //            number = number + 100;
    //        }
    //    }
    //    private void fastDownBtnClk(EventArgs args)
    //    {
    //        if (number > numMin + 100)
    //        { 
    //            number = number - 100;
    //        }
    //    }
    //    private decimal numMin = Decimal.MinValue;
    //    private decimal numMax = Decimal.MaxValue;
    //    NumberPickerType pickerType = NumberPickerType.Integer;
    //}

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
