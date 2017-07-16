using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class PageOneViewModel : ValidatableBindableBase
    {
        Type masterType = typeof(DataSet);

        public PageOneViewModel()
        {
            _dataSetList = new DataSetList();

            MasterGrid = new UDTButtonGrid<DataSet>(DataSetList.Sets);
            MasterGrid.SetEditProps = SetEditProps;
            MasterGrid.LoadEditProps = LoadEditProps;
            MasterGrid.SelectionIndexChange = SetChildCollection;
            MasterGrid.IsPropertyEdited = IsPropertyEdited;
            MasterGrid.CreateDataSet = CreateDataSet;
            MasterGrid.ParentHasErrors = parentHasError;

            DataSets = new ObservableCollection<DataItem>();
            DetailGrid = new UDTDataGrid<DataItem>(DataSets);

        }

        private DataSetList _dataSetList;

        public UDTButtonGrid<DataSet> MasterGrid { get; set; }
        public UDTDataGrid<DataItem> DetailGrid { get; set; }

        private bool parentHasError()
        {
            return HasErrors;
        }

        private void SetEditProps(DataSet dataSet, string value)
        {
            Name = value;
            Description = value;
            if (dataSet != null)
            {
                Name = dataSet.Name;
                Description = dataSet.Description;
            }
        }    
        
        private void LoadEditProps(DataSet dataSet)
        {
            dataSet.Name = Name;
            dataSet.Description = Description;
        }   
       
        private void SetChildCollection(int selectedIndex)
        {
            DetailGrid.DataSets = MasterGrid.DataSets[selectedIndex].DataItems;
        }   

        private bool IsPropertyEdited(DataSet dataSet)
        {
            return MasterGrid.DataSets[MasterGrid.SelectedIndex].Name != Name ||
                MasterGrid.DataSets[MasterGrid.SelectedIndex].Description != Description;
        }

        private DataSet CreateDataSet()
        {
            return new DataSet("", "", new ObservableCollection<DataItem>(), new ObservableCollection<DataSetRelation>());
        }

        ObservableCollection<DataItem> _dataSets = null;
        public ObservableCollection<DataItem> DataSets
        {
            get { return _dataSets; }
            set { SetProperty(ref _dataSets, value); }
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
                MasterGrid.SaveCommand.RaiseCanExecuteChanged();
                MasterGrid.CancelCommand.RaiseCanExecuteChanged();
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
                MasterGrid.SaveCommand.RaiseCanExecuteChanged();
                MasterGrid.CancelCommand.RaiseCanExecuteChanged();
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
                MasterGrid.SaveCommand.RaiseCanExecuteChanged();
                MasterGrid.CancelCommand.RaiseCanExecuteChanged();
            }
        }


    }
}
