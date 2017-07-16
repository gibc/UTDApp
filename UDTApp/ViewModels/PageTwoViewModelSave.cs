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
    public class PageTwoViewModelSave : ViewModelBase<DataItem, DataItem>
    {
        public PageTwoViewModelSave()
        {
            IsMasterVisible = false;
            IsDetailVisible = true;
            IsRelationVisible = false;
            IsItemVisible = true;
        }

        override public bool IsPropertyEdited
        {
            get
            {
                if (ChildSelectedIndex == -1) return false;
                return DataItems[ChildSelectedIndex].Name != ChildName ||
                    DataItems[ChildSelectedIndex].Type.ToString() != Type;
            }
        }

        override public void SetTextProps(DataItem dataSet, string value = "")
        {
            ChildName = value;
            Type = value;
            if (dataSet != null)
            {
                ChildName = dataSet.Name;
                Type = dataSet.Type.ToString();
            }
        }

        override public void SetMasterTextProps(DataSet dataSet, string value = "") { }

        override public void SetChildTextProps(DataItem dataSet, string value = "") 
        {
            SetTextProps(dataSet, value);
        }

 
        override public void LoadTextProps(DataItem dataSet)
        {
            dataSet.Name = ChildName;
            dataSet.Type = Convert.ToInt32(Type);
        }
        override public DataItem CreateNewDataSet()
        {
            // returns C
            return new DataItem();
        }
        override public ObservableCollection<DataItem> GetSelectedCol(int selectedIndex)
        {
            // takes nothing, returns D
            //return EditedCol[selectedIndex].DataItems;
            return DataSets[selectedIndex].DataItems;
        }
        override public ObservableCollection<DataItem> EditedCol
        {
            // returns C
            get { return DataItems; }
        }

        override public int EditedIndex { get { return ChildSelectedIndex; } set { ChildSelectedIndex = value; } }

        override public bool IsInputEnabled
        {
            get { return (ChildSelectedItem != null || _newDataSet != null); }
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
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
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
                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

    }
}
