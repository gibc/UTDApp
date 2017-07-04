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
        where D : ModelBase
        where C : ModelBase
    {
        public abstract C SelectedItem { get; set; }
        public abstract int SelectedIndex { get; set; }
        public abstract bool IsPropertyEdited { get; }
        public abstract void SetTextProps(C dataSet, string value = "");
        public abstract void LoadTextPops(C dataSet);
        public abstract C CreateNewDataSet(); 
        public abstract ObservableCollection<D> GetSelectedCol(int selectedIndex);
        public abstract ObservableCollection<C> GetEditedCol();

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

        virtual public bool IsInputEnabled
        {
            get { return (SelectedItem != null || _newDataSet != null); }
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

        private string _name;
        [Required]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty(ref _name, value);
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                SetProperty(ref _description, value);
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                SetProperty(ref _title, value);
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
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
            return GetEditedCol() != null;
        }


        private void DeleteDataSet()
        {
            GetEditedCol().Remove(SelectedItem);
            if (GetEditedCol().Count > 0)
                SelectedIndex = 0;
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
                LoadTextPops(GetEditedCol()[SelectedIndex]);
            }
            else
            {
                LoadTextPops(_newDataSet);
                GetEditedCol().Add(_newDataSet);
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
            if (GetEditedCol().Count > 0)
            {
                if (SelectedIndex == -1) SelectedIndex = 0;
                SetTextProps(GetEditedCol()[SelectedIndex]);
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
