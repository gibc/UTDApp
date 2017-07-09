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

        public UDTDataGrid(ObservableCollection<D> dataSets)
        {
            CreateColumnsCommand = new DelegateCommand<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(createColumns);
            DataSets = dataSets;
        }

        public Action<int> SelectionIndexChange { get; set; }
        private void selectionIndexChange(int selectionIndex)
        {
            if (SelectionIndexChange != null  && selectionIndex != -1) 
                SelectionIndexChange(selectionIndex);
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }

            set
            {
                SetProperty(ref _selectedIndex, value);
                selectionIndexChange(value);
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
