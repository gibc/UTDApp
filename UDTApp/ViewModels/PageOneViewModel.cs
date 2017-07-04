using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UDTApp.Models;
using UDTAppControlLibrary.Behaviour;

namespace UDTApp.ViewModels
{

    public class PageOneViewModel : ViewModelBase<DataItem, DataSet>
    {
        override public int SelectedIndex
        {
            get { return DataSetList.SelectedIndex; }

            set
            {
                int si = value;
                SetProperty(ref si, value);
                DataSetList.SelectedIndex = si;

                RaisePropertyChanged("SelectedItem");
                if (DataSetList.SelectedIndex > -1)
                {
                    DataItems = GetSelectedCol(SelectedIndex);
                }
                else
                    DataItems = null;
            }
        }

        private DataSet _selectedItem;
        override public DataSet SelectedItem
        {
            get
            { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
                if (value != null)
                {
                    Name = value.Name;
                    Description = value.Description;
                }
                else
                {
                    Name = "";
                    Description = "";
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsInputEnabled");
                _newDataSet = null;
            }
        }

        override public bool IsPropertyEdited { 
            get
            {
                return DataSets[SelectedIndex].Name != Name ||
                    DataSets[SelectedIndex].Description != Description;
            }
        }
        override public void SetTextProps(DataSet dataSet, string value = "")
        {
            Name = value;
            Description = value;
            if (dataSet != null)
            {
                Name = dataSet.Name;
                Description = dataSet.Description;
            }
        }
        override public void LoadTextPops(DataSet dataSet)
        {
            dataSet.Name = Name;
            dataSet.Description = Description;

        }
        override public DataSet CreateNewDataSet()
        {
            // returns C
            return new DataSet("", "", new ObservableCollection<DataItem>(), new ObservableCollection<DataSetRelation>());
        }
        override public ObservableCollection<DataItem> GetSelectedCol(int selectedIndex)
        {
            // takes nothing, returns D
            return GetEditedCol()[selectedIndex].DataItems;
        }
        override public ObservableCollection<DataSet> GetEditedCol()
        {
            // returns C
            return DataSets;
        }
    }

    //public class PageOneViewModel : ValidatableBindableBase, INavigationAware//: SetupPageBase, IDataEditActions
    //{
    //    private DataSetList _dataSetList;
    //    public DelegateCommand AddCommand { get; set; }
    //    public DelegateCommand DeleteCommand { get; set; }
    //    public DelegateCommand SaveCommand { get; set; }
    //    public DelegateCommand CancelCommand { get; set; }
    //    public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

    //    public delegate void SetModelPropsDel<T>(T dataObj, string val);
    //    SetModelPropsDel<DataSet> setModelProps;

    //    public DelegateCommand setModelPropsCommand { get; set; }

    //    protected ISubPage _p;


    //    public PageOneViewModel()
    //    {

    //        _p = new SubPageOne(this);

    //        IsMasterVisible = true;
    //        IsDetailVisible = false;
    //        IsRelationVisible = false;
    //        IsItemVisible = false;

    //        CreateColumnsCommand = new DelegateCommand<DataGridAutoGeneratingColumnEventArgs>(createColumns);

    //        _dataSetList = new DataSetList();
    //        DataSets = DataSetList.Sets;

    //        AddCommand = new DelegateCommand(AddDataSet, canAddDataSet);
    //        DeleteCommand = new DelegateCommand(DeleteDataSet, canDelete);
    //        SaveCommand = new DelegateCommand(SaveUpdate, canSave);
    //        CancelCommand = new DelegateCommand(cancelUpdate, canCancel);
    //        ValidationEnabled = validationEnabled;
    //    }

    //    private bool _autoColumns = true;
    //    public bool AutoColumns
    //    {
    //        get { return _autoColumns; }
    //        set { SetProperty(ref _autoColumns, value); }
    //    }

    //    private bool validationEnabled()
    //    {
    //        return IsInputEnabled;
    //    }

    //    virtual public bool IsInputEnabled
    //    {
    //        get { return (_selectedItem != null || _newDataSet != null); }
    //    }

    //    private bool _isMasterVisible = false;
    //    public bool IsMasterVisible { 
    //        get { return _isMasterVisible; }
    //        set { SetProperty(ref _isMasterVisible, value); }
    //    }

    //    private bool _isDetailVisible = false;
    //    public bool IsDetailVisible {
    //        get { return _isDetailVisible; }
    //        set { SetProperty(ref _isDetailVisible, value); }
    //    }

    //    private bool _isItemVisible = false;
    //    public bool IsItemVisible
    //    {
    //        get { return _isItemVisible; }
    //        set { SetProperty(ref _isItemVisible, value); }
    //    }

    //    private bool _isRelationVisible = false;
    //    public bool IsRelationVisible { 
    //        get { return _isRelationVisible; }
    //        set { SetProperty(ref _isRelationVisible, value); }
    //    }

    //    public dynamic DataSets
    //    {
    //        get { return DataSetList.Sets as ObservableCollection<DataSet>; }
    //        set { DataSetList.Sets = value; }
    //        //get 
    //        //{
    //        //    return _dataSetList.Sets as ObservableCollection<DataSet>; 
    //        //}
    //        //set { _dataSetList.Sets = value; }
    //    }

    //    virtual public int SelectedIndex
    //    {
    //        get { return DataSetList.SelectedIndex; }

    //        set
    //        {
    //            int si = value;
    //            SetProperty(ref si, value);
    //            DataSetList.SelectedIndex = si;
    
    //            RaisePropertyChanged("SelectedItem");
    //            if (DataSetList.SelectedIndex > -1)
    //            {
    //                //DataItems = _p.SetChildDataSet(DataSets, DataSetList.SelectedIndex);
    //                DataItems = setChildDataSet<DataItem>(value);

    //                //_p.DataItems = DataItems;
    //                //_p.DataItems = _p.SetChildDataSet(DataSets, DataSetList.SelectedIndex);
    //            //if(DataSets != null) DataItems = DataSets[_dataSetList.SelectedIndex].DataItems;
    //            }
    //            else
    //                DataItems = null;
    //        }
    //    }

    //    private dynamic setChildDataSet<T>(int value)
    //    {
    //        if (typeof(T) == typeof(DataItem))
    //            return DataSets[value].DataItems;
    //        if (typeof(T) == typeof(DataSetRelation))
    //            return DataSets[value].DataSetRelations;
    //        return null;
    //    }

    //    private DataSet _selectedItem;
    //    public DataSet SelectedItem
    //    {
    //        get
    //        { return _selectedItem; }
    //        set
    //        {
    //            SetProperty(ref _selectedItem, value);
    //            if (value != null)
    //            {
    //                Name = value.Name;
    //                Description = value.Description;
    //            }
    //            else
    //            {
    //                Name = "";
    //                Description = "";
    //            }
    //            DeleteCommand.RaiseCanExecuteChanged();
    //            CancelCommand.RaiseCanExecuteChanged();
    //            RaisePropertyChanged("IsInputEnabled");
    //            _newDataSet = null;
    //        }
    //    }

    //    //private ObservableCollection<DataItem> _dataItems;
    //    private dynamic _dataItems;
    //    //public ObservableCollection<DataItem> DataItems
    //    public dynamic DataItems
    //    {
    //        get { return _dataItems; }
    //        set
    //        {
    //            SetProperty(ref _dataItems, value);
    //            AddCommand.RaiseCanExecuteChanged();
    //            DeleteCommand.RaiseCanExecuteChanged();
    //        }
    //    }

    //    private string _name;
    //    [Required]
    //    [StringLength(15, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 15 characters.")]
    //    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
    //    public string Name
    //    {
    //        get
    //        {
    //            return _name;
    //        }
    //        set
    //        {
    //            SetProperty(ref _name, value);
    //            SaveCommand.RaiseCanExecuteChanged();
    //            CancelCommand.RaiseCanExecuteChanged();
    //        }
    //    }

    //    private string _description;
    //    public string Description
    //    {
    //        get
    //        {
    //            return _description;
    //        }
    //        set
    //        {
    //            SetProperty(ref _description, value);
    //            SaveCommand.RaiseCanExecuteChanged();
    //            CancelCommand.RaiseCanExecuteChanged();
    //        }
    //    }

    //    private string _title;
    //    public string Title
    //    {
    //        get
    //        {
    //            return _title;
    //        }
    //        set
    //        {
    //            SetProperty(ref _title, value);
    //            SaveCommand.RaiseCanExecuteChanged();
    //            CancelCommand.RaiseCanExecuteChanged();
    //        }
    //    }

    //    List<PropertyInfo> _UIProperties = null;
    //    virtual protected List<PropertyInfo> UIProperties
    //    {
    //        get
    //        {
    //            if(_UIProperties == null)
    //            {
    //                _UIProperties = new List<PropertyInfo>();
    //                _UIProperties.Add(PropertyHelper<PageOneViewModel>.GetProperty(x => x.Name));
    //                _UIProperties.Add(PropertyHelper<PageOneViewModel>.GetProperty(x => x.Description));
    //                _UIProperties.Add(PropertyHelper<PageOneViewModel>.GetProperty(x => x.Title));
    //            }
    //            return _UIProperties;
    //        }
    //    }

    //    private void SetUIProperties(string value)
    //    {
    //        foreach(PropertyInfo prop in UIProperties)
    //        {
    //            prop.SetValue(this, value);
    //        }
    //    }

    //    protected dynamic _newDataSet = null;
    //    private void AddDataSet()
    //    {
    //        // below required to force validation error if input fields already empty
    //        AddDataSet<DataSet>();
    //        return;

    //        _p.SetTextProps(_newDataSet, "xxx");
    //        _newDataSet = _p.CreateNewDataSet();
    //        _p.SetTextProps(_newDataSet, "");
    //        DeleteCommand.RaiseCanExecuteChanged();
    //        CancelCommand.RaiseCanExecuteChanged();
    //        SaveCommand.RaiseCanExecuteChanged();
    //        RaisePropertyChanged("IsInputEnabled");
    //    }

    //    public void AddDataSet<T>() where T : ModelBase, new()
    //    {
    //        SetUIProperties("xxx");
    //        T newDataSet = new T();
    //        newDataSet.GetData(this);
    //        _newDataSet = newDataSet;

    //        DeleteCommand.RaiseCanExecuteChanged();
    //        CancelCommand.RaiseCanExecuteChanged();
    //        SaveCommand.RaiseCanExecuteChanged();
    //        RaisePropertyChanged("IsInputEnabled");
    //    }

    //    private bool canAddDataSet()
    //    {
    //        return canAddDataSet<DataSet>(DataSets);

    //        return DataItems != null;
    //    }

    //    private bool canAddDataSet<T>(ObservableCollection<T> dataSet)
    //    {
    //        return dataSet != null;
    //    }

    //    private void DeleteDataSet()
    //    {
    //        PropertyInfo prop = PropertyHelper<PageOneViewModel>.GetProperty(x => x.SelectedIndex);
    //        DeleteDataSet<DataSet>(DataSets, prop);
    //        return;

    //        DataItems.Remove(_p.SelectedItem);
    //        if (DataItems.Count > 0)
    //            _p.SelectedIndex = 0;
    //    }

    //    private void DeleteDataSet<T>(ObservableCollection<T> dataSet, PropertyInfo selectedIndexProp)
    //    {
    //        int selectedIndex = (int)selectedIndexProp.GetValue(this);
    //        dataSet.Remove(dataSet[selectedIndex]);
    //        if (dataSet.Count > 0)
    //            selectedIndexProp.SetValue(this, 0);
    //    }

    //    bool canDelete()
    //    {
    //        PropertyInfo selectionIndexProp = PropertyHelper<PageOneViewModel>.GetProperty(x => x.SelectedIndex);
    //        return canDelete(selectionIndexProp);

    //        if (_newDataSet != null) return false;
    //        return _p.SelectedIndex > -1;
    //    }

    //    bool canDelete(PropertyInfo selectionIndexProp)
    //    {
    //        if (_newDataSet != null) return false;
    //        return (int)selectionIndexProp.GetValue(this) > -1;
    //    }



    //    private void SaveUpdate()
    //    {
    //        PropertyInfo prop = PropertyHelper<PageOneViewModel>.GetProperty(x => x.SelectedItem);
    //        SaveUpdate<DataSet>(DataSets, prop);
    //        return;

    //        if (_newDataSet == null)
    //        {
    //            //_p.LoadTextPops(_p.DataItems[_p.SelectedIndex]);
    //            _p.LoadTextPops(DataItems[_p.SelectedIndex]);
    //            //RaisePropertyChanged("DataItems");
    //        }
    //        else
    //        {
    //            _p.LoadTextPops(_newDataSet);
    //            DataItems.Add(_newDataSet);
    //            _p.SelectedItem = _newDataSet;
    //            _newDataSet = null;
    //        }
    //        CancelCommand.RaiseCanExecuteChanged();
    //        SaveCommand.RaiseCanExecuteChanged();
    //    }

    //    private void SaveUpdate<T>(ObservableCollection<T> dataSet, PropertyInfo selectedItemProp) where T : ModelBase
    //    {
    //        T dataObj = (T)selectedItemProp.GetValue(this);
    //        if (_newDataSet == null)
    //        {
    //            dataObj.PutData(this);
    //        }
    //        else
    //        {
    //            dataObj.PutData(_newDataSet);
    //            dataSet.Add(_newDataSet);
    //            //_p.SelectedItem = _newDataSet;
    //            selectedItemProp.SetValue(this, _newDataSet);
    //            _newDataSet = null;
    //        }
    //        CancelCommand.RaiseCanExecuteChanged();
    //        SaveCommand.RaiseCanExecuteChanged();
    //    }


    //    private bool canSave()
    //    {
    //        PropertyInfo selectionIndexProp = PropertyHelper<PageOneViewModel>.GetProperty(x => x.SelectedIndex);
    //        return canSave<DataSet>(DataSets, selectionIndexProp);

    //        if (HasErrors) return false;
    //        if (_p.SelectedIndex == -1 && _newDataSet == null) return false;
    //        if (_p.SelectedIndex > -1)
    //        {
    //            return _p.IsPropertyEdited; 
    //        }
    //        return true;
    //    }

    //    private bool canSave<T>(ObservableCollection<T> dataSet, PropertyInfo selectionIndexProp)
    //    {
    //        int selectionIndex = (int)selectionIndexProp.GetValue(this);
    //        if (HasErrors) return false;
    //        if (selectionIndex == -1 && _newDataSet == null) return false;
    //        if (selectionIndex > -1)
    //        {
    //            T dataObject = dataSet[selectionIndex];
    //            return IsDirty<T>(dataObject);
    //        }
    //        return true;
    //    }


    //    private void cancelUpdate()
    //    {
    //        PropertyInfo selelectedIndexProp = PropertyHelper<PageOneViewModel>.GetProperty(x => x.SelectedIndex);
    //        cancelUpdate<DataSet>(DataSets, selelectedIndexProp);
    //        return;

    //        _p.SetTextProps(null, "xxx");
    //        _newDataSet = null;
    //        if (DataItems.Count > 0)
    //        {
    //            if (_p.SelectedIndex == -1) _p.SelectedIndex = 0;
    //            _p.SetTextProps(DataItems[_p.SelectedIndex]);
    //        }
    //        else
    //            _p.SetTextProps(null, "");
    //        RaisePropertyChanged("IsInputEnabled");
    //        SaveCommand.RaiseCanExecuteChanged();
    //    }

    //    private void cancelUpdate<T>(ObservableCollection<T> dataSets, PropertyInfo selectedIndexProp) where T : ModelBase
    //    {
    //        SetUIProperties("xxx");
    //        _newDataSet = null;
    //        int selectedIndex = (int)selectedIndexProp.GetValue(this);

    //        if (dataSets.Count > 0)
    //        {
    //            if (selectedIndex == -1) selectedIndexProp.SetValue(this, 0);
    //            dataSets[(int)selectedIndexProp.GetValue(this)].GetData(this);
    //        }
    //        else
    //            SetUIProperties("");

    //        RaisePropertyChanged("IsInputEnabled");
    //        SaveCommand.RaiseCanExecuteChanged();
    //    }

    //    private bool canCancel()
    //    {
    //        PropertyInfo selectionIndexProp = PropertyHelper<PageOneViewModel>.GetProperty(x => x.SelectedIndex);
    //        return canCancel<DataSet>(DataSets, selectionIndexProp);

    //        if (_newDataSet != null) return true;
    //        if (_p.SelectedIndex > -1)
    //        {
    //            return _p.IsPropertyEdited;
    //        }
    //        return false;
    //    }

    //    private bool canCancel<T>(ObservableCollection<T> dataSet, PropertyInfo selectionIndexProp)
    //    {
    //        if (_newDataSet != null) return true;
    //        int selectionIndex = (int)selectionIndexProp.GetValue(this);
    //        if (selectionIndex > -1)
    //        {
    //            return IsDirty<T>(dataSet[selectionIndex]);
    //        }
    //        return false;
    //    }

    //    private bool IsDirty<T>(T dataObj)
    //    {
    //        List<PropertyInfo> dataPropList = new List<PropertyInfo>(typeof(T).GetProperties());
    //        foreach (var prop in UIProperties)
    //        {
    //            PropertyInfo dataProp = dataPropList.Find(x => x.Name == prop.Name);
    //            if (dataProp != null && dataProp.GetValue(dataObj) != prop.GetValue(this)) return true;
    //        }
    //        return false;
    //    }

    //    public static class PropertyHelper<T>
    //    {
    //        public static PropertyInfo GetProperty<TValue>(
    //            Expression<Func<T, TValue>> selector)
    //        {
    //            System.Linq.Expressions.Expression body = selector;
    //            if (body is LambdaExpression)
    //            {
    //                body = ((LambdaExpression)body).Body;
    //            }
    //            switch (body.NodeType)
    //            {
    //                case ExpressionType.MemberAccess:
    //                    return (PropertyInfo)((MemberExpression)body).Member;
    //                default:
    //                    throw new InvalidOperationException();
    //            }
    //        }
    //    }


    //    static public void createColumns(DataGridAutoGeneratingColumnEventArgs e)
    //    {
    //        //if (e.PropertyType.Name.Contains("ObservableCollection") || e.Column.Header.ToString().Contains("ID"))
    //        if (!ModelBase.IsRecordProperty(e))
    //        {
    //            e.Column.Visibility = Visibility.Hidden;
    //        }
    //    }



    //    public bool IsNavigationTarget(NavigationContext navigationContext)
    //    {return true;}
    //    public void OnNavigatedTo(NavigationContext navigationContext) 
    //    {
    //        SelectedIndex = DataSetList.SelectedIndex;

    //    }
    //    public void OnNavigatedFrom(NavigationContext navigationContext) 
    //    {
    //        SelectedIndex = DataSetList.SelectedIndex;

    //    }


    //}


}
