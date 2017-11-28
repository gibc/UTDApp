using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UDTApp.Models;
using UDTApp.ViewModels;
using UDTAppControlLibrary.Controls;
using System.Windows.Controls;

namespace UDTApp.EditControlViewModels
{
    public class NumberViewModel : UDTDataBoxBase
    {
        public NumberViewModel(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
            maxValue = Int32.MaxValue;
            minValue = Int32.MinValue;
            editProps = item.editProps;
            UDTIntEditProps props = item.editProps as UDTIntEditProps;
            if (props.maxValue != null)
                maxValue = (Int32)props.maxValue;
            if (props.minValue != null)
                minValue = (Int32)props.minValue;
        }

        // bind to NumberBox control in data template
        private Int32? _number = null;        
        public Int32? number 
        {
            get { return _number; }
            set 
            { 
                SetProperty(ref _number, value); 
                numberChanged(value);
            }
        }

        private Int32 _maxValue = Int32.MaxValue;
        public Int32 maxValue
        {
            get { return _maxValue; }
            set { SetProperty(ref _maxValue, value); }
        }

        private Int32 _minValue = Int32.MinValue;
        public Int32 minValue
        {
            get { return _minValue; }
            set { SetProperty(ref _minValue, value); }
        }

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                if (editProps.defaultValue != null)
                {
                    number = (Int32?)editProps.defaultValue;
                }
                else number = null;
            }
            else if (udtItem.GetType() == typeof(UDTIntItem))
            {
                int intVal = (int)row[colName];
                number = intVal;
            }
        }


        private void numberChanged(Int32? intNum)
        {
            if (intNum == null) row[colName] = DBNull.Value;
            if (editProps.required && intNum == null)
            {
                List<string> errLst = new List<string>();
                errLst.Add("Number entry is required.");
                SetErrors(() => this.number, errLst);
            }
            else
            {
                SetErrors(() => this.number, new List<string>());
            }
            validationChanged(HasErrors);
            if (!HasErrors)
            {
                if (row[colName] == DBNull.Value)
                {
                    row[colName] = intNum;
                }
                else
                {
                    if (udtItem.TypeName == UDTTypeName.Number)
                    {
                        int currentVal = (int)row[colName];
                        if (currentVal != (int)intNum)
                            row[colName] = intNum;
                    }
                }

            }
        }

    }
}
