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
    public class UDTDataGrid<D> : ValidatableBindableBase where D : ModelBase
    {
        ObservableCollection<D> _dataSets = null;
        public ObservableCollection<D> DataSets 
        {
            get { return _dataSets; } 
            set
            {
                SetProperty(ref _dataSets, value);
            }
        }

        public DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs> CreateColumnsCommand { get; set; }

        public UDTDataGrid(ObservableCollection<D> dataSets,
            Action<int> setChildCollection
            )
            {
                SetChildCollection = setChildCollection;
                CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);
                DataSets = dataSets;
            }

        Action<int> SetChildCollection = null;

        //public bool IsInputEnabled
        //{
        //    get { return (SelectedItem != null || _newDataSet != null); }
        //}

        //private bool validationEnabled()
        //{
        //    return IsInputEnabled;
        //}

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }

            set
            {
                SetProperty(ref _selectedIndex, value);
                if (SetChildCollection != null && value != -1) SetChildCollection(value);
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
                    //SetEditProps(value, "");
                }
                else
                {
                    //SetEditProps(null, "");
                }
                //DeleteCommand.RaiseCanExecuteChanged();
                //CancelCommand.RaiseCanExecuteChanged();
                //RaisePropertyChanged("IsInputEnabled");
                //_newDataSet = null;
            }
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
