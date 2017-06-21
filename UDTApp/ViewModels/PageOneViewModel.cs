using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class PageOneViewModel : ValidatableBindableBase, INavigationAware//: SetupPageBase, IDataEditActions
    {
        private DataSetList _dataSetList;
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        protected ISubPage _p;

        public PageOneViewModel()
        {

            _p = new SubPageOne(this);

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
            get { return (_selectedItem != null || _newDataSet != null); }
        }

        private bool _isMasterVisible = false;
        public bool IsMasterVisible { 
            get { return _isMasterVisible; }
            set { SetProperty(ref _isMasterVisible, value); }
        }

        private bool _isDetailVisible = false;
        public bool IsDetailVisible {
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
        public bool IsRelationVisible { 
            get { return _isRelationVisible; }
            set { SetProperty(ref _isRelationVisible, value); }
        }

        public dynamic DataSets
        {
            get { return DataSetList.Sets as ObservableCollection<DataSet>; }
            set { DataSetList.Sets = value; }
            //get 
            //{
            //    return _dataSetList.Sets as ObservableCollection<DataSet>; 
            //}
            //set { _dataSetList.Sets = value; }
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
                    DataItems = DataSets[DataSetList.SelectedIndex].DataItems;
                    //if(DataSets != null) DataItems = DataSets[_dataSetList.SelectedIndex].DataItems;
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

        private ObservableCollection<DataItem> _dataItems;
        public ObservableCollection<DataItem> DataItems
        //public dynamic DataItems
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



        protected dynamic _newDataSet = null;
        private void AddDataSet()
        {
            // below required to force validation error if input fields already empty
            _p.SetTextProps(_newDataSet, "xxx");
            _newDataSet = _p.CreateNewDataSet();
            _p.SetTextProps(_newDataSet, "");
            DeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("IsInputEnabled");
        }

        private bool canAddDataSet()
        {
            return _p.DataSet != null;
        }

        private void DeleteDataSet()
        {
            _p.DataSet.Remove(_p.SelectedItem);
            if (_p.DataSet.Count > 0)
                _p.SelectedIndex = 0;
        }

        bool canDelete()
        {
            if (_newDataSet != null) return false;
            return _p.SelectedIndex > -1;
        }

        private void SaveUpdate()
        {
            if (_newDataSet == null)
            {
                _p.LoadTextPops(_p.DataSet[_p.SelectedIndex]);
            }
            else
            {
                _p.LoadTextPops(_newDataSet);
                _p.DataSet.Add(_newDataSet);
                _p.SelectedItem = _newDataSet;
                _newDataSet = null;
            }
            CancelCommand.RaiseCanExecuteChanged();

        }

        private bool canSave()
        {
            if (HasErrors) return false;
            if (_p.SelectedIndex == -1 && _newDataSet == null) return false;
            if (_p.SelectedIndex > -1)
            {
                return _p.IsPropertyEdited; 
            }
            return true;
        }

        private void cancelUpdate()
        {
            _p.SetTextProps(null, "xxx");
            _newDataSet = null;
            if (_p.DataSet.Count > 0)
            {
                if (_p.SelectedIndex == -1) _p.SelectedIndex = 0;
                _p.SetTextProps(_p.DataSet[_p.SelectedIndex]);
            }
            else
                _p.SetTextProps(null, "");
            RaisePropertyChanged("IsInputEnabled");
        }

        private bool canCancel()
        {
            if (_newDataSet != null) return true;
            if (_p.SelectedIndex > -1)
            {
                return _p.IsPropertyEdited;
            }
            return false;
        }

        static public void createColumns(DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.Name.Contains("ObservableCollection") || e.Column.Header.ToString().Contains("ID"))
            {
                e.Column.Visibility = Visibility.Hidden;
            }
        }



        public bool IsNavigationTarget(NavigationContext navigationContext)
        {return true;}
        public void OnNavigatedTo(NavigationContext navigationContext) 
        {
            SelectedIndex = DataSetList.SelectedIndex;

        }
        public void OnNavigatedFrom(NavigationContext navigationContext) 
        {
            SelectedIndex = DataSetList.SelectedIndex;

        }


    }


}
