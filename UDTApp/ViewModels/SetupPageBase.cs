using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class SetupPageBase : ValidatableBindableBase
    {

        private DataSetList _dataSetList;
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public SetupPageBase()
        {
            _dataSetList = new DataSetList();
            AddCommand = new DelegateCommand(AddDataSet);
            DeleteCommand = new DelegateCommand(DeletDataSet, canDelete);
            SaveCommand = new DelegateCommand(SaveUpdate, canSave);
            CancelCommand = new DelegateCommand(cancelUpdate, canCancel);
            CreateColumnsCommand = new DelegateCommand<DataGridAutoGeneratingColumnEventArgs>(createColumns);
            ValidationEnabled = validationEnabled;
        }

        private bool validationEnabled()
        {
            return IsInputEnabled;
        }

        public bool IsInputEnabled
        {
            get { return (_selectedItem != null || _newDataSet != null); }
        }

        public bool IsMasterVisible { get; set; }
        public bool IsDetailVisible { get {return !IsMasterVisible; }}


        public ObservableCollection<DataSet> DataSets
        {
            get { return _dataSetList.Sets; }
            set { _dataSetList.Sets = value; }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                SetProperty(ref _selectedIndex, value);
                RaisePropertyChanged("SelectedItem");
                if (_selectedIndex > -1)
                    DataItems = DataSets[_selectedIndex].DataItems;
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
        {
            get { return _dataItems; }
            set
            {
                SetProperty(ref _dataItems, value);
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

        private bool messageVisable = false;
        public bool MessageVisable
        {
            get { return messageVisable; }
            set { SetProperty(ref messageVisable, value); }
        }

        private DataSet _newDataSet = null;
        private void AddDataSet()
        {
            // below required to force validation error if input fields already empty
            Name = "xxx";
            Description = "xxx";
            _newDataSet = new DataSet("", "", new ObservableCollection<DataItem>());
            Name = _newDataSet.Name;
            Description = _newDataSet.Description;
            DeleteCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("IsInputEnabled");
        }

        private void DeletDataSet()
        {
            DataSets.Remove(SelectedItem);
            if (DataSets.Count > 0)
                SelectedIndex = 0;
        }

        bool canDelete()
        {
            if (_newDataSet != null) return false;
            return _selectedIndex > -1;
        }

        private void SaveUpdate()
        {
            if (_newDataSet == null)
            {
                DataSets[_selectedIndex].Name = _name;
                DataSets[_selectedIndex].Description = _description;
            }
            else
            {
                _newDataSet.Name = _name;
                _newDataSet.Description = _description;
                DataSets.Add(_newDataSet);
                SelectedItem = _newDataSet;
                _newDataSet = null;
            }
        }

        private bool canSave()
        {
            if (HasErrors) return false;
            if (_selectedIndex == -1 && _newDataSet == null) return false;
            if (_selectedIndex > -1)
            {
                return (DataSets[_selectedIndex].Name != Name || DataSets[_selectedIndex].Description != Description);
            }
            return true;
        }

        private void cancelUpdate()
        {
            Name = "";
            Description = "";
            _newDataSet = null;
            if (DataSets.Count > 0)
            {
                if (SelectedIndex == -1) SelectedIndex = 0;
                Name = DataSets[SelectedIndex].Name;
                Description = DataSets[SelectedIndex].Description;
            }
            RaisePropertyChanged("IsInputEnabled");
        }

        private bool canCancel()
        {
            if (_newDataSet != null) return true;
            if (_selectedIndex > -1)
            {
                return (DataSets[_selectedIndex].Name != Name || DataSets[_selectedIndex].Description != Description);
            }
            return false;
        }

        private void createColumns(DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.Name.Contains("ObservableCollection") || e.Column.Header.ToString().Contains("ID"))
            {
                e.Column.Visibility = Visibility.Hidden;
                //e.Column = GetAccountColumn();
            }
        }

        private DataGridTemplateColumn GetAccountColumn()
        {
            // Create The Column
            DataGridTemplateColumn accountColumn = new DataGridTemplateColumn();
            accountColumn.Header = "Account";

            Binding bind = new Binding("Account");
            bind.Mode = BindingMode.OneWay;

            // Create the TextBlock
            FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, bind);
            DataTemplate textTemplate = new DataTemplate();
            textTemplate.VisualTree = textFactory;

            // Create the ComboBox
            Binding itemSourceBind = new Binding("Path=DataContext.DataSets, RelativeSource={RelativeSource AncestorType={x:Type Window}"); //{Binding Path=DataContext.Owners, RelativeSource={RelativeSource AncestorType={x:Type Window}}
            itemSourceBind.Mode = BindingMode.OneWay;
            Binding selectedItem = new Binding("DataItem");

            FrameworkElementFactory comboFactory = new FrameworkElementFactory(typeof(ComboBox));
            comboFactory.SetValue(ComboBox.IsTextSearchEnabledProperty, true);
            comboFactory.SetValue(ComboBox.ItemsSourceProperty, new Binding("Path=DataItems"));
            comboFactory.SetBinding(ComboBox.SelectedItemProperty, selectedItem);

            DataTemplate comboTemplate = new DataTemplate();
            comboTemplate.VisualTree = comboFactory;

            // Set the Templates to the Column
            //accountColumn.CellTemplate = textTemplate;
            accountColumn.CellTemplate = comboTemplate;
            //accountColumn.CellEditingTemplate = comboTemplate;

            return accountColumn;
        }
    }
}

