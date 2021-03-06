﻿using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace UDTApp.ViewModels.DataEntryControls
{

    public class UDTNumberEntry : UDTNumberPicker
    {
        public UDTNumberEntry(string _name, decimal _numMax, decimal _numMin,
            NumberPickerType _pickerType = NumberPickerType.Integer,
            Action<decimal?> _numberChanged = null) :
            base(_name, _numMax, _numMin, _pickerType, _numberChanged)
        {

        }
    }

    public enum NumberPickerType { Integer, Decimal, Text, Date }
    public class UDTNumberPicker : ValidatableBindableBase
    {
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> UpCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> DownCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> FastUpCommand { get; set; }
        [XmlIgnoreAttribute]
        public DelegateCommand<EventArgs> FastDownCommand { get; set; }

        private UDTNumberPicker() 
        {
            UpCommand = new DelegateCommand<EventArgs>(upBtnClk);
            DownCommand = new DelegateCommand<EventArgs>(downBtnClk);
            FastUpCommand = new DelegateCommand<EventArgs>(fastUpBtnClk);
            FastDownCommand = new DelegateCommand<EventArgs>(fastDownBtnClk);
        }

        public UDTNumberPicker(string _name, decimal _numMax, decimal _numMin,
            NumberPickerType _pickerType = NumberPickerType.Integer,
            Action<decimal?> _numberChanged = null) : this()
        {
            name = _name;
            numberChanged = _numberChanged;
            numMax = _numMax;
            numMin = _numMin;
            pickerType = _pickerType;
        }

        public string name { get; set; }

        private decimal? _number = null;
        public decimal? number
        {
            get { return _number; }
            set
            {
                SetProperty(ref _number, value);
                txtNumber = getNumText(number);
                if (numberChanged != null) numberChanged(_number);
            }
        }

        private string getNumText(decimal? num)
        {
            string numTxt = "";
            if (num == null) return numTxt;
            if (pickerType == NumberPickerType.Integer)
                numTxt = string.Format("{0:n0}", number);
            else if (pickerType == NumberPickerType.Decimal)
            {
                numTxt = string.Format("{0}", number);
                if (_txtNumber.Length > 0 && _txtNumber.Last() == '.')
                    return numTxt + '.';
                else return numTxt;
            }
            return numTxt;
        }

        private string _txtNumber = "";
        public string txtNumber
        {
            get
            {
                if (!textParsed) return _txtNumber;
                else if (number == null) return "";
                return getNumText(number);
            }
            set
            {
                SetProperty(ref _txtNumber, filterDigits(value));
                textParsed = false;
                if (!containsOnlyZeros(_txtNumber))
                {
                    textParsed = true;
                    parseNumber(_txtNumber);
                    if (numberChanged != null) numberChanged(_number);
                }
            }
        }

        private bool textParsed = false;
        private bool containsOnlyZeros(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            bool retVal = true;
            foreach (char c in val)
            {
                if (!(c == '0' || c == '.' || c == '-'))
                {
                    return false;
                }
            }
            return retVal;
        }

        private void parseNumber(string txtNum)
        {
            if (string.IsNullOrEmpty(txtNum))
                _number = null;
            else if (pickerType == NumberPickerType.Integer)
            {
                int num;
                if (Int32.TryParse(txtNum, out num))
                    _number = num;
                else if (txtNum[0] == '-')
                {
                    _number = numMin;
                }
                else
                {
                    _number = numMax;
                }
            }
            else if (pickerType == NumberPickerType.Decimal)
            {
                Decimal val;
                if (Decimal.TryParse(txtNum, out val))
                {
                    _number = val;
                    return;
                }
                else if (txtNum[0] == '-')
                {
                    _number = numMin;
                }
                else
                {
                    _number = numMax;
                }
            }
            if (_number > numMax)
                _number = numMax;
            else if (_number < numMin)
                _number = numMin;
        }

        private string filterDigits(string txt)
        {
            string outTxt = "";
            if (string.IsNullOrEmpty(txt))
                return outTxt;
            if (pickerType == NumberPickerType.Integer)
            {
                foreach (char c in txt)
                {
                    if (txt.First() == c)
                    {
                        if (Char.IsDigit(c) || c == '+' || c == '-')
                            outTxt += c;
                    }

                    else if (Char.IsDigit(c))
                    {
                        outTxt += c;
                    }
                }
            }
            else
            {
                bool haveDecPt = false;
                foreach (char c in txt)
                {
                    if (txt.First() == c)
                    {
                        if (Char.IsDigit(c) || c == '+' || c == '-' || c == '.')
                            outTxt += c;
                    }
                    else if (Char.IsDigit(c))
                    {
                        outTxt += c;
                    }
                    else if (c == '.' && !haveDecPt)
                    {
                        haveDecPt = true;
                        outTxt += c;
                    }
                }
            }
            //if (string.IsNullOrEmpty(outTxt))
            //    return "0";
            return outTxt;
        }

        //private bool _notUsed = true;
        //public bool notUsed
        //{
        //    get { return _notUsed; }
        //    set
        //    {
        //        SetProperty(ref _notUsed, value);
        //    }
        //}

        [XmlIgnoreAttribute]
        public Action<decimal?> numberChanged { get; set; }

        private void upBtnClk(EventArgs args)
        {
            if (number == null) number = 0;
            if (number < numMax)
            {
                number++;
            }
            else number = numMax;
        }
        private void downBtnClk(EventArgs args)
        {
            if (number == null) number = 0;
            if (number > numMin)
            {
                number--;
            }
            else number = numMin;
        }
        private void fastUpBtnClk(EventArgs args)
        {
            if (number == null) number = 0;
            if (number < numMax - 100)
            {
                number = number + 100;
            }
            else number = numMax;
        }
        private void fastDownBtnClk(EventArgs args)
        {
            if (number == null) number = 0;
            if (number > numMin + 100)
            {
                number = number - 100;
            }
            else number = numMin;
        }

        //private void notUsedClk(RoutedEventArgs args)
        //{
        //    if (notUsed)
        //        number = null;
        //    else if (number == null)
        //        number = 0;
        //}

        private decimal numMin = Decimal.MinValue;
        private decimal numMax = Decimal.MaxValue;
        //NumberPickerType pickerType = NumberPickerType.Integer;
        protected NumberPickerType pickerType = NumberPickerType.Integer;
    }
}
