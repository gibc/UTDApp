using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;
using UDTApp.SchemaModels;
using UDTApp.ViewModels;

namespace UDTApp.EditControlViewModels
{
    public class TextViewModel : UDTDataBoxBase
    {
        public TextViewModel(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
            maxLength = Int32.MaxValue;
            minLength = Int32.MinValue;
            editProps = item.editProps;
            UDTTextEditProps props = item.editProps as UDTTextEditProps;
            if (props.maxLength != null)
                maxLength = (Int32)props.maxLength;
            if (props.minLength != null)
                minLength = (Int32)props.minLength;
            if (props.defaultText != null)
                defaultText = props.defaultText;
        }

        private string _textValue = "";
        public string textValue
        {
            get { return _textValue; }
            set
            {
                //if (value.Length < minLength || value.Length > maxLength)
                //    value = _textValue;
                SetProperty(ref _textValue, value);
                textChanged(value);
            }
        }

        private string _defaultText = null;
        public string defaultText
        {
            get { return _defaultText; }
            set
            {
                SetProperty(ref _defaultText, value);
            }
        }

        private Int32 _maxLength = Int32.MaxValue;
        public Int32 maxLength
        {
            get { return _maxLength; }
            set { SetProperty(ref _maxLength, value); }
        }

        private Int32 _minLength = Int32.MinValue;
        public Int32 minLength
        {
            get { return _minLength; }
            set { SetProperty(ref _minLength, value); }
        }

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                if (editProps.defaultText != null)
                {
                    textValue = (string)editProps.defaultText;
                }
                else textValue = null;
            }
            else if (udtItem.GetType() == typeof(UDTTxtItem))
            {
                string strVal = (string)row[colName];
                textValue = strVal;
            }
        }

        private void textChanged(string strVal)
        {
            if (strVal == null) row[colName] = DBNull.Value;
            if (editProps.required && string.IsNullOrEmpty(strVal))
            {
                List<string> errLst = new List<string>();
                errLst.Add("Text entry is required.");
                SetErrors(() => this.textValue, errLst);
            }
            else if(strVal.Length < editProps.minLength)
            {
                List<string> errLst = new List<string>();
                string errMsg = string.Format("Min {0} characters required.", editProps.minLength);
                errLst.Add(errMsg);
                SetErrors(() => this.textValue, errLst);
            }
            else if (strVal.Length > editProps.maxLength)
            {
                List<string> errLst = new List<string>();
                string errMsg = string.Format("Max {0} characters allowed.", editProps.maxLength);
                errLst.Add(errMsg);
                SetErrors(() => this.textValue, errLst);
            }
            else
            {
                SetErrors(() => this.textValue, new List<string>());
            }
            validationChanged(HasErrors);
            if (!HasErrors)
            {
                if (row[colName] == DBNull.Value)
                {
                    row[colName] = strVal;
                }
                else
                {
                    if (udtItem.TypeName == UDTTypeName.Text)
                    {
                        string currentVal = (string)row[colName];
                        if (currentVal != strVal)
                            row[colName] = strVal;
                    }
                }

            }
        }
    }
}
