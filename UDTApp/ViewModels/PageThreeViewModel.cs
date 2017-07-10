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
    public class PageThreeViewModel : ValidatableBindableBase
    {
        public PageThreeViewModel()
        {
            _dataSetList = new DataSetList();

            DataSets = new ObservableCollection<DataSetRelation>();
            DetailGrid = new UDTButtonGrid<DataSetRelation>(null); 
            DetailGrid.SetEditProps = SetEditProps;
            DetailGrid.LoadEditProps = LoadEditProps;
            DetailGrid.IsPropertyEdited = IsPropertyEdited;
            DetailGrid.CreateDataSet = CreateDataSet;
            DetailGrid.CanAddDataSet = CanAddDataSet;

            MasterGrid = new UDTDataGrid<DataSet>(DataSetList.Sets);
            MasterGrid.SelectionIndexChange = SetChildCollection;
        }

        private DataSetList _dataSetList;

        public UDTButtonGrid<DataSetRelation> DetailGrid { get; set; }
        public UDTDataGrid<DataSet> MasterGrid { get; set; }

        private bool CanAddDataSet(Func<bool> baseMethod)
        {
            if (!baseMethod()) return false;
            //return DetailGrid.DataSets != null && ChildOptions != null && ChildOptions.Count > 1;
            return ChildOptions.Count > 1;
        }

        private void SetEditProps(DataSetRelation dataSet, string value)
        {
            ParentDataSet = value;
            //SelectedChild = value;
            if (dataSet != null)
            {
                ParentDataSet = dataSet.ParentDateSet;

                var parentDataSet = DataSetList.Sets[MasterGrid.SelectedIndex].Name;

                ChildOptions = getChildOptions(parentDataSet, dataSet.ChildDateSet);
                SelectedChild = dataSet.ChildDateSet;
            }
        }

        private void LoadEditProps(DataSetRelation dataSet)
        {
            dataSet.ParentDateSet = ParentDataSet;
            dataSet.ChildDateSet = SelectedChild;
        }

        private void SetChildCollection(int selectedIndex)
        {
            DetailGrid.DataSets = MasterGrid.DataSets[selectedIndex].DataSetRelations;
            if (DetailGrid.DataSets.Count > 0)
                DetailGrid.SelectedIndex = 0;
        }

        private bool IsPropertyEdited(DataSetRelation dataSet)
        {
            return DetailGrid.DataSets[DetailGrid.SelectedIndex].ParentDateSet != ParentDataSet ||
                DetailGrid.DataSets[DetailGrid.SelectedIndex].ChildDateSet != SelectedChild;
        }

        private DataSetRelation CreateDataSet()
        {
            return new DataSetRelation("", "");
        }

        ObservableCollection<DataSetRelation> _dataSets = null;
        public ObservableCollection<DataSetRelation> DataSets
        {
            get { return _dataSets; }
            set { SetProperty(ref _dataSets, value); }
        }

        private ObservableCollection<String> getChildOptions(string parent, string child)
        {
            //var parentDataSet = DataSetList.Sets[SelectedIndex].Name;
            string childDataSet = child; // value.ChildDateSet;
            List<string> excludeDataSets = DataSetList.Sets[MasterGrid.SelectedIndex].DataSetRelations
                .Where(y => y.ChildDateSet != childDataSet).
                Select(x => x.ChildDateSet).ToList();
            excludeDataSets.Add(parent);

            List<string> allDataSets = DataSetList.Sets.Select(x => x.Name).ToList();
            List<string> result = allDataSets.Except(excludeDataSets).ToList();
            if (child.Length == 0)
                result.Add(child);

            return new ObservableCollection<String>(result);
        }

 
        private int _comboIndex = -1;
        public int ComboIndex
        {
            get { return _comboIndex; }
            set
            {
                SetProperty(ref _comboIndex, value);
                DetailGrid.SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _parentName;
        [Required]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        public string ParentDataSet
        {
            get
            {
                if (MasterGrid.SelectedIndex > -1)
                    _parentName = MasterGrid.DataSets[MasterGrid.SelectedIndex].Name;
                return _parentName;
            }
            set
            {
                SetProperty(ref _parentName, value);
                DetailGrid.SaveCommand.RaiseCanExecuteChanged();
                DetailGrid.CancelCommand.RaiseCanExecuteChanged();
            }
        }

        private string _selectedChild = null;
        [CustomValidation(typeof(PageThreeViewModel), "CheckChildDataSet")]
        public string SelectedChild
        {
            get { return _selectedChild; }
            set
            {
                SetProperty(ref _selectedChild, value);
                DetailGrid.SaveCommand.RaiseCanExecuteChanged();
                DetailGrid.CancelCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<String> _childOptions = null;
        public ObservableCollection<String> ChildOptions
        {
            get
            {
                return _childOptions;
            }
            set
            {
                SetProperty(ref _childOptions, value);
            }
        }

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckChildDataSet(string childDataSet, ValidationContext context)
        {
            if (childDataSet != null && childDataSet.Length == 0)
                return new System.ComponentModel.DataAnnotations.ValidationResult("Please select a child dataset from the drop-down list.");
            else return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

    }
}
