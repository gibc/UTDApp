using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class UDTButtonGrid<D> : ValidatableBindableBase where D : ModelBase
    {
        public ObservableCollection<D> DataSets { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public UDTButtonGrid(ObservableCollection<D> dataSets,
            Action<D, string> setEditProps,
            Action<D> loadEditProps,
            Action<int> setChildCollection,
            Predicate<D> isPropertyEdited,
            Func<D> createDataSet
            )
        {
            DataSets = dataSets;
            SetEditProps = setEditProps;
            LoadEditProps = loadEditProps;
            SetChildCollection = setChildCollection;
            IsPropertyEdited = isPropertyEdited;
            CreateDataSet = createDataSet;

            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);

            AddCommand = new DelegateCommand(AddDataSet, canAddDataSet);
            DeleteCommand = new DelegateCommand(DeleteDataSet, canDelete);
            SaveCommand = new DelegateCommand(SaveUpdate, canSave);
            CancelCommand = new DelegateCommand(cancelUpdate, canCancel);

            ValidationEnabled = validationEnabled;

        }
        Action<D, string> SetEditProps = null;
        Action<D> LoadEditProps = null;
        Action<int> SetChildCollection = null;
        Predicate<D> IsPropertyEdited = null;
        Func<D> CreateDataSet = null;

        private D _newDataSet = null;

        public bool IsInputEnabled
        {
            get { return (SelectedItem != null || _newDataSet != null); }
        }

        private bool validationEnabled()
        {
            return IsInputEnabled;
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }

            set
            {
                SetProperty(ref _selectedIndex, value);
                if(SetChildCollection != null  && value != -1) SetChildCollection(value);
            }
        }

        private D _selectedItem;
        public D SelectedItem
        {
            get
            { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
                if (value != null)
                {
                    SetEditProps(value, "");
                }
                else
                {
                    SetEditProps(null, "");
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsInputEnabled");
                _newDataSet = null;
            }
        }

        private void AddDataSet()
        {
            SetEditProps(_newDataSet, "xxx");
            _newDataSet = CreateDataSet();
            SetEditProps(_newDataSet, "");
            DeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("IsInputEnabled");
        }

        private bool canAddDataSet()
        {
            return DataSets != null;
        }

        private void DeleteDataSet()
        {
            DataSets.Remove(DataSets[_selectedIndex]);
            if (DataSets.Count > 0)
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
                LoadEditProps(DataSets[_selectedIndex]);
                DataSets[_selectedIndex].State = ObjectState.Dirty;
            }
            else
            {
                LoadEditProps(_newDataSet);
                DataSets.Add(_newDataSet);
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
                return IsPropertyEdited(DataSets[_selectedIndex]);
            }
            return true;
        }

        private void cancelUpdate()
        {
            SetEditProps(null, "xxx");
            _newDataSet = null;
            if (DataSets.Count > 0)
            {
                if (_selectedIndex == -1) SelectedIndex = 0;
                SetEditProps(DataSets[_selectedIndex], "");
                DataSets[_selectedIndex].State = ObjectState.Updated;
            }
            else
                SetEditProps(null, "");
            RaisePropertyChanged("IsInputEnabled");
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool canCancel()
        {
            if (_newDataSet != null) return true;
            if (SelectedIndex > -1)
            {
                return IsPropertyEdited(DataSets[_selectedIndex]);
            }
            return false;
        }

        static public void createColumns(System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
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
