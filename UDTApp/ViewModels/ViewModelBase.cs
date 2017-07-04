using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public abstract class ViewModelBase<D, C> : ValidatableBindableBase
        where D : ModelBase // set to detail grid type: DataItem, DataItem, DataSetRelation
        where C : ModelBase // set to edited type:      DataSet,  DataItem, DataSetRelation
    {

        public abstract bool IsPropertyEdited { get; }
        public abstract bool IsInputEnabled { get; }
        public abstract void SetTextProps(C dataSet, string value = "");
        public abstract void SetMasterTextProps(DataSet dataSet, string value = "");
        public abstract void SetChildTextProps(C dataSet, string value = "");
        public abstract void LoadTextProps(C dataSet);
        public abstract C CreateNewDataSet(); 
        public abstract ObservableCollection<D> GetSelectedCol(int selectedIndex);
        public abstract ObservableCollection<C> EditedCol { get; }
        public abstract int EditedIndex { get; set; }

        private DataSetList _dataSetList;
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public ViewModelBase()
        {

            IsMasterVisible = true;
            IsDetailVisible = false;
            IsRelationVisible = false;
            IsItemVisible = false;

            CreateColumnsCommand = new DelegateCommand<DataGridAutoGeneratingColumnEventArgs>(createColumns);

            _dataSetList = new DataSetList();
            DataSets = DataSetList.Sets;

            AddCommand = new DelegateCommand(AddDataSet, canAddDataSet);
            DeleteCommand = new DelegateCommand(DeleteDataSet, canDelete);
            SaveCommand = new DelegateCommand(SaveUpdate, canSave);
            CancelCommand = new DelegateCommand(cancelUpdate, canCancel);
            ValidationEnabled = validationEnabled;
        }

        private bool _autoColumns = true;
        public bool AutoColumns
        {
            get { return _autoColumns; }
            set { SetProperty(ref _autoColumns, value); }
        }

        private bool validationEnabled()
        {
            return IsInputEnabled;
        }

        private bool _isMasterVisible = false;
        public bool IsMasterVisible
        {
            get { return _isMasterVisible; }
            set { SetProperty(ref _isMasterVisible, value); }
        }

        private bool _isDetailVisible = false;
        public bool IsDetailVisible
        {
            get { return _isDetailVisible; }
            set { SetProperty(ref _isDetailVisible, value); }
        }

        private bool _isItemVisible = false;
        public bool IsItemVisible
        {
            get { return _isItemVisible; }
            set { SetProperty(ref _isItemVisible, value); }
        }

        private bool _isRelationVisible = false;
        public bool IsRelationVisible
        {
            get { return _isRelationVisible; }
            set { SetProperty(ref _isRelationVisible, value); }
        }

        public ObservableCollection<DataSet> DataSets
        {
            get { return DataSetList.Sets as ObservableCollection<DataSet>; }
            set { DataSetList.Sets = value; }

        }

        private ObservableCollection<D> _dataItems;
        public ObservableCollection<D> DataItems
        {
            get { return _dataItems; }
            set
            {
                SetProperty(ref _dataItems, value);
                AddCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private int _childSelectedIndex = -1;
        public int ChildSelectedIndex
        {
            get { return _childSelectedIndex; }
            set
            {
                SetProperty(ref _childSelectedIndex, value);
                RaisePropertyChanged("ChildSelectedItem");
            }
        }

        public int SelectedIndex
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
                    //DataItems = DataSets[SelectedIndex].DataItems;
                }
                else
                    DataItems = null;
            }
        }

        private DataSet _selectedItem;
        public DataSet SelectedItem
        {
            get
            { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
                if (value != null)
                {
                    SetMasterTextProps(value, "");
                }
                else
                {
                    SetMasterTextProps(null, "");
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsInputEnabled");
                _newDataSet = null;
            }
        }

        private DataItem _childSelectedItem;
        public DataItem ChildSelectedItem
        {
            get
            { return _childSelectedItem; }
            set
            {
                SetProperty(ref _childSelectedItem, value);
                if (value != null)
                {

                    SetChildTextProps(value as C, "");
                }
                else
                {
                    SetChildTextProps(null, "");
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsInputEnabled");
                _newDataSet = null;
            }
        }



        protected dynamic _newDataSet = null;
        private void AddDataSet()
        {
            SetTextProps(_newDataSet, "xxx");
            _newDataSet = CreateNewDataSet();
            SetTextProps(_newDataSet, "");
            DeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("IsInputEnabled");
        }

        private bool canAddDataSet()
        {
            return EditedCol != null;
        }


        private void DeleteDataSet()
        {
            //EditedCol.Remove(SelectedItem);
            EditedCol.Remove(EditedCol[EditedIndex]);
            if (EditedCol.Count > 0)
                EditedIndex = 0;
        }

        bool canDelete()
        {
            if (_newDataSet != null) return false;
            return SelectedIndex > -1;
        }


        private void SaveUpdate()
        {

            if (_newDataSet == null)
            {
                LoadTextProps(EditedCol[EditedIndex]);
                EditedCol[EditedIndex].State = ObjectState.Dirty;
            }
            else
            {
                LoadTextProps(_newDataSet);
                EditedCol.Add(_newDataSet);
                SelectedItem = _newDataSet;
                _newDataSet = null;
            }
            CancelCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool canSave()
        {

            if (HasErrors) return false;
            if (SelectedIndex == -1 && _newDataSet == null) return false;
            if (SelectedIndex > -1)
            {
                return IsPropertyEdited;
            }
            return true;
        }

        private void cancelUpdate()
        {
            SetTextProps(null, "xxx");
            _newDataSet = null;
            if (EditedCol.Count > 0)
            {
                if (EditedIndex == -1) EditedIndex = 0;
                SetTextProps(EditedCol[EditedIndex]);
                EditedCol[EditedIndex].State = ObjectState.Updated;
            }
            else
                SetTextProps(null, "");
            RaisePropertyChanged("IsInputEnabled");
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool canCancel()
        {
            if (_newDataSet != null) return true;
            if (SelectedIndex > -1)
            {
                return IsPropertyEdited;
            }
            return false;
        }

        //private bool IsDirty<T>(T dataObj)
        //{
        //    List<PropertyInfo> dataPropList = new List<PropertyInfo>(typeof(T).GetProperties());
        //    foreach (var prop in UIProperties)
        //    {
        //        PropertyInfo dataProp = dataPropList.Find(x => x.Name == prop.Name);
        //        if (dataProp != null && dataProp.GetValue(dataObj) != prop.GetValue(this)) return true;
        //    }
        //    return false;
        //}

        //public static class PropertyHelper<T>
        //{
        //    public static PropertyInfo GetProperty<TValue>(
        //        Expression<Func<T, TValue>> selector)
        //    {
        //        System.Linq.Expressions.Expression body = selector;
        //        if (body is LambdaExpression)
        //        {
        //            body = ((LambdaExpression)body).Body;
        //        }
        //        switch (body.NodeType)
        //        {
        //            case ExpressionType.MemberAccess:
        //                return (PropertyInfo)((MemberExpression)body).Member;
        //            default:
        //                throw new InvalidOperationException();
        //        }
        //    }
        //}


        static public void createColumns(DataGridAutoGeneratingColumnEventArgs e)
        {
            //if (e.PropertyType.Name.Contains("ObservableCollection") || e.Column.Header.ToString().Contains("ID"))
            if (!ModelBase.IsRecordProperty(e))
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        { return true; }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //SelectedIndex = DataSetList.SelectedIndex;

        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //SelectedIndex = DataSetList.SelectedIndex;

        }
    }
}
