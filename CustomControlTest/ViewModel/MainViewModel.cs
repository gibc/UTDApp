using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomControlTest.ViewModel
{
    public class MainViewModel : ValidatableBindableBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public MainViewModel()
        {
            mask = "00/00/0000";
            maskedText = "02/11/2007";
        }

        public string _maskedText = "";
        public string maskedText
        {
            get { return _maskedText; }
            set 
            { 
                SetProperty(ref _maskedText, value);
                NotifyPropertyChanged("maskedText");
            }
        }

        public string _mask = "";
        public string mask
        {
            get { return _mask; }
            set 
            { 
                SetProperty(ref _mask, value);
                NotifyPropertyChanged("mask");
            }
        }

        public Int32? _numberValue = null;
        //public Int32? _numberValue = 54321;
        //[CustomValidation(typeof(MainViewModel), "CheckNullRequired")]
        public Int32? numberValue
        {
            get 
            {
                setError(_numberValue);
                return _numberValue; 
            }
            set
            {
                SetProperty(ref _numberValue, value);
                setError(_numberValue);
            }
        }

        private void setError(Int32? value)
        {
            if (value == null)
            {
                List<string> errLst = new List<string>();
                errLst.Add("Number entry is required.");
                SetErrors(() => this.numberValue, errLst);
            }
            else
                SetErrors(() => this.numberValue, new List<string>());
        }

        //public Decimal? _decimalValue = 123.456m;
        public Decimal? _decimalValue = null;
        public Decimal? decimalValue
        {
            get { return _decimalValue; }
            set
            {
                SetProperty(ref _decimalValue, value);
            }
        }

        public Decimal? _decimalDefault = 7.89m;
        public Decimal? decimalDefault
        {
            get { return _decimalDefault; }
            set
            {
                SetProperty(ref _decimalDefault, value);
            }
        }

        //public DateTime? _dateValue = new DateTime(1999, 9, 9);
        public DateTime? _dateValue = null;
        public DateTime? dateValue
        {
            get { return _dateValue; }
            set
            {
                SetProperty(ref _dateValue, value);
            }
        }

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckNullRequired(string name, ValidationContext context)
        {
            MainViewModel dataObj = context.ObjectInstance as MainViewModel;
            if (dataObj != null)
            {
                if (dataObj.numberValue == null)
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Number is required.");
                }
           }

            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }
    }


}
