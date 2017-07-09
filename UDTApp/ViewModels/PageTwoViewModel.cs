using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class PageTwoViewModel : ValidatableBindableBase
    {
        public PageTwoViewModel()
        {
            _dataSetList = new DataSetList();

            DataSets = new ObservableCollection<DataItem>();
            DetailGrid = new UDTButtonGrid<DataItem>(null); 
            DetailGrid.SetEditProps = SetEditProps;
            DetailGrid.LoadEditProps = LoadEditProps;
            DetailGrid.IsPropertyEdited = IsPropertyEdited;
            DetailGrid.CreateDataSet = CreateDataSet;


            MasterGrid = new UDTDataGrid<DataSet>(DataSetList.Sets);
            MasterGrid.SelectionIndexChange = SetChildCollection;

        }

        private DataSetList _dataSetList;

        public UDTButtonGrid<DataItem> DetailGrid { get; set; }
        public UDTDataGrid<DataSet> MasterGrid { get; set; }

        private void SetEditProps(DataItem dataSet, string value)
        {
            ChildName = value;
            Type = value;
            if (dataSet != null)
            {
                ChildName = dataSet.Name;
                Type = dataSet.Type.ToString();
            }
        }

        private void LoadEditProps(DataItem dataSet)
        {
            dataSet.Name = ChildName;
            dataSet.Type = Convert.ToInt32(Type);
        }

        private void SetChildCollection(int selectedIndex)
        {
            DetailGrid.DataSets = MasterGrid.DataSets[selectedIndex].DataItems;
        }

        private bool IsPropertyEdited(DataItem dataSet)
        {
            return DetailGrid.DataSets[DetailGrid.SelectedIndex].Name != ChildName ||
                DetailGrid.DataSets[DetailGrid.SelectedIndex].Type.ToString() != Type;
        }

        private DataItem CreateDataSet()
        {
            return new DataItem("", 1);
        }

        ObservableCollection<DataItem> _dataSets = null;
        public ObservableCollection<DataItem> DataSets
        {
            get { return _dataSets; }
            set { SetProperty(ref _dataSets, value); }
        }


        private string _name;
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Name must be between 4 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        public string ChildName
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty(ref _name, value);
                DetailGrid.SaveCommand.RaiseCanExecuteChanged();
                DetailGrid.CancelCommand.RaiseCanExecuteChanged();
            }
        }

        private string _type;
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                SetProperty(ref _type, value);
                DetailGrid.SaveCommand.RaiseCanExecuteChanged();
                DetailGrid.CancelCommand.RaiseCanExecuteChanged();
            }
        }

    }
}
