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
        ObservableCollection<D> _dataSets = null;
        public ObservableCollection<D> DataSets
        {
            get { return _dataSets; }
            set
            {
                SetProperty(ref _dataSets, value);
                if(AddCommand != null) AddCommand.RaiseCanExecuteChanged();
            }
        }        
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public UDTButtonGrid(ObservableCollection<D> dataSets)
        {
            DataSets = dataSets;

            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);

            AddCommand = new DelegateCommand(AddDataSet, canAddDataSet);
            DeleteCommand = new DelegateCommand(DeleteDataSet, canDelete);
            SaveCommand = new DelegateCommand(SaveUpdate, canSave);
            CancelCommand = new DelegateCommand(cancelUpdate, canCancel);

            ValidationEnabled = validationEnabled;


        }

        //Action<D, string> SetEditProps = null;
        public Action<D, string> SetEditProps { private get; set; }
        private void setEditProps(D dataSet, string value)
        {
            if (SetEditProps != null) SetEditProps(dataSet, value);
        }

        //Action<D> LoadEditProps = null;
        public Action<D> LoadEditProps { private get; set; }
        private void loadEditProps(D dataSet)
        {
            if (LoadEditProps != null) LoadEditProps(dataSet);
        }

        //Action<int> SetChildCollection = null;
        public Action<int> SelectionIndexChange { private get; set; }
        private void selectionIndexChange(int selectionIndex)
        {
            if (SelectionIndexChange != null && selectionIndex != -1)
                SelectionIndexChange(selectionIndex);
        }

        public Predicate<D> IsPropertyEdited { private get; set; }
        private bool isPropertyEdited(D dataSet)
        {
            if(IsPropertyEdited != null) return IsPropertyEdited(dataSet);
            return false;
        }

        public Func<D> CreateDataSet { private get; set; }
        private D createDataSet()
        {
            if (CreateDataSet != null) return CreateDataSet();
            return null;
        }

        private D _newDataSet = null;
        private D _deletedDataSet = null;

        private bool _isEnabled = false;
        public bool IsInputEnabled
        {
            get { return _isEnabled; }
            set 
            {
                SetProperty(ref _isEnabled, value);
            }
        }
        private bool isInputEnabled{ get {return (SelectedItem != null || _newDataSet != null); }}

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
                //if(SetChildCollection != null  && value != -1) SetChildCollection(value);
                selectionIndexChange(value);
                AddCommand.RaiseCanExecuteChanged();
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
                    setEditProps(value, "");
                }
                else
                {
                    setEditProps(null, "");
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                IsInputEnabled = isInputEnabled;
                _newDataSet = null;
            }
        }

        private void AddDataSet()
        {
            setEditProps(_newDataSet, "xxx");
            _newDataSet = createDataSet();
            setEditProps(_newDataSet, "");
            DeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            AddCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            IsInputEnabled = isInputEnabled;
        }

        public Func<Func<bool>, bool> CanAddDataSet { private get; set; }
        private bool canAddDataSet()
        {
            if (CanAddDataSet != null)
            {
                //Func<bool> baseMethod = _canAddDataSet;
                //return CanAddDataSet(baseMethod);
                return CanAddDataSet(() => _canAddDataSet());
            }
            return _canAddDataSet();
        }

        private bool _canAddDataSet()
        {
            return DataSets != null && _newDataSet == null && _deletedDataSet == null;
        }

        private void DeleteDataSet()
        {
            _deletedDataSet = DataSets[_selectedIndex];
            DataSets.Remove(DataSets[_selectedIndex]);
            if (DataSets.Count > 0)
                SelectedIndex = 0;
        }

        bool canDelete()
        {
            if (_newDataSet != null || _deletedDataSet != null) return false;
            return SelectedIndex > -1;
        }

        private void SaveUpdate()
        {
            if (_newDataSet == null && _deletedDataSet == null)
            {
                loadEditProps(DataSets[_selectedIndex]);
                if (DataSets[_selectedIndex].State != ObjectState.New) 
                    DataSets[_selectedIndex].State = ObjectState.Dirty;
            }
            else if (_newDataSet != null)
            {
                loadEditProps(_newDataSet);
                DataSets.Add(_newDataSet);
                SelectedItem = _newDataSet;
                _newDataSet = null;
            }
            else if (_deletedDataSet != null)
            {
                DataSetList.DeleteRecord(_deletedDataSet);
                _deletedDataSet = null;
            }

            CancelCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            AddCommand.RaiseCanExecuteChanged();
        }

        private bool canSave()
        {
            if (HasErrors) return false;
            if(!ValidatableBindableBase.IsValid) return false;
            if (_newDataSet != null || _deletedDataSet != null) return true;
            if (SelectedIndex > -1 )
            {
                return isPropertyEdited(DataSets[_selectedIndex]);
            }
            return false;
        }

        private void cancelUpdate()
        {
            setEditProps(null, "xxx");
            _newDataSet = null;
            if(_deletedDataSet != null)
            {
                DataSets.Add(_deletedDataSet);
                SelectedItem = _deletedDataSet;
                _deletedDataSet = null;
            }
            else if (DataSets.Count > 0)
            {
                if (_selectedIndex == -1) SelectedIndex = 0;
                setEditProps(DataSets[_selectedIndex], "");
                if (DataSets[_selectedIndex].State!= ObjectState.New) 
                    DataSets[_selectedIndex].State = ObjectState.Updated;
            }
            else
                setEditProps(null, "");
            IsInputEnabled = isInputEnabled;
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool canCancel()
        {
            if (_newDataSet != null || _deletedDataSet != null) return true;
            if (SelectedIndex > -1)
            {
                return isPropertyEdited(DataSets[_selectedIndex]);
            }
            return false;
        }

        public void createColumns(System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
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
