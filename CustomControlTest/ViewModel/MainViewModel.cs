using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public Int32? _maskNumber = 12345;
        public Int32? maskNumber
        {
            get { return _maskNumber; }
            set
            {
                SetProperty(ref _maskNumber, value);
                NotifyPropertyChanged("maskNumber");
            }
        }

        public Decimal? _maskDecimal = 123.456m;
        public Decimal? maskDecimal
        {
            get { return _maskDecimal; }
            set
            {
                SetProperty(ref _maskDecimal, value);
                NotifyPropertyChanged("maskDecimal");
            }
        }
    }
}
