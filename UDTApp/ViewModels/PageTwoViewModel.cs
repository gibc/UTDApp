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
    public class PageTwoViewModel : PageOneViewModel
    {

        public PageTwoViewModel()
        {
            //_p = new SubPageTwo(this);

            IsMasterVisible = false;
            IsDetailVisible = true;
            IsRelationVisible = false;
            IsItemVisible = true;
        }

        override public bool IsInputEnabled
        {
            get { return (_childSelectedItem != null || _newDataSet != null); }
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

        private DataItem _childSelectedItem;
        public DataItem ChildSelectedItem
        {
            get
            { return _childSelectedItem; }
            set
            {
                SetProperty(ref _childSelectedItem, value);
                if (value != null)
                {
                    ChildName = value.Name;
                    Type = value.Type.ToString();
                }
                else
                {
                    ChildName = "";
                    Type = "";
                }
                DeleteCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged("IsInputEnabled");
                _newDataSet = null;
            }
        }

        private string _name;
        [Required]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 15 characters.")]
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
