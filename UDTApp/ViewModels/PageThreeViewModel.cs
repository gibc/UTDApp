using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UDTApp.Models;

namespace UDTApp.ViewModels
{

    public class PageThreeViewModel : PageOneViewModel
    {

        public PageThreeViewModel()
        {
            _p = new SubPageThree(this);
            IsMasterVisible = false;
            IsDetailVisible = true;
            IsRelationVisible = true;
            IsItemVisible = false;

        }

        override public bool IsInputEnabled
        {
            get { return (_childSelectedItem != null || _newDataSet != null); }
        }

        //private ObservableCollection<DataSetRelation> _dataRelaionItems;
        //public ObservableCollection<DataSetRelation> DataRelationItems
        ////public dynamic DataItems
        //{
        //    get { return _dataRelaionItems; }
        //    set
        //    {
        //        SetProperty(ref _dataRelaionItems, value);
        //        AddCommand.RaiseCanExecuteChanged();
        //        DeleteCommand.RaiseCanExecuteChanged();
        //    }
        //}

        override public int SelectedIndex
        {
            get { return base.SelectedIndex; }
            set 
            { 
                base.SelectedIndex = value;
                ChildOptions = null;
            }
        }

        private int _childSelectedIndex = -1;
        public int ChildSelectedIndex
        {
            get { return _childSelectedIndex; }
            set
            {
                SetProperty(ref _childSelectedIndex, value);
                RaisePropertyChanged("ChildSelectedItem");
            }
        }

        public ObservableCollection<String> getChildOptions(string parent, string child)
        {
            //var parentDataSet = DataSetList.Sets[SelectedIndex].Name;
            string childDataSet = child; // value.ChildDateSet;
            List<string> excludeDataSets = DataSetList.Sets[SelectedIndex].DataSetRelations
                .Where(y => y.ChildDateSet != childDataSet).
                Select(x => x.ChildDateSet).ToList();
            excludeDataSets.Add(parent);

            List<string> allDataSets = DataSetList.Sets.Select(x => x.Name).ToList();
            List<string> result = allDataSets.Except(excludeDataSets).ToList();
            if (child.Length == 0)
                result.Add(child);

            return new ObservableCollection<String>(result);
        }

        private DataSetRelation _childSelectedItem;
        public DataSetRelation ChildSelectedItem
        {
            get
            { return _childSelectedItem; }
            set
            {
                SetProperty(ref _childSelectedItem, value);
                if (value != null)
                {
                    var parentDataSet = DataSetList.Sets[SelectedIndex].Name;

                    ChildOptions = getChildOptions(parentDataSet, value.ChildDateSet);
                    SelectedChild = value.ChildDateSet;
                }
                else
                {
                    SelectedChild = "";
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsInputEnabled");
                _newDataSet = null;
            }
        }

        private int _comboIndex = -1;
        public int ComboIndex {
            get { return _comboIndex; }
            set 
            { 
                SetProperty(ref _comboIndex, value);
                SaveCommand.RaiseCanExecuteChanged();
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
                if(SelectedIndex > -1)
                    _parentName = DataSets[SelectedIndex].Name;
                return _parentName;
            }
            //set
            //{
            //    SetProperty(ref _parentName, value);
            //    SaveCommand.RaiseCanExecuteChanged();
            //    CancelCommand.RaiseCanExecuteChanged();
            //}
        }

        private string _selectedChild = null;
        [CustomValidation(typeof(PageThreeViewModel), "CheckChildDataSet")]
        public string SelectedChild
        {
            get { return _selectedChild;  }
            set 
            { 
                SetProperty(ref _selectedChild, value);
                SaveCommand.RaiseCanExecuteChanged();
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

        private string _childName;
        [Required]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        public string ChildDataSet
        {
            get
            {
                return _childName;
            }
            set
            {
                SetProperty(ref _childName, value);
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
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
