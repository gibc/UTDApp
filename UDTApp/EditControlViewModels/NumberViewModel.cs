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
            editProps = item.editProps;
            decimal maxNum = Decimal.MaxValue;
            decimal minNum = Decimal.MinValue;
            if (editProps.maxPicker.number != null)
                maxNum = (Decimal)editProps.maxPicker.number;
            if (editProps.minPicker.number != null)
                minNum = (Decimal)editProps.minPicker.number;
            //NumberPickerType pickType = NumberPickerType.Integer;
            //if (item.TypeName == UDTTypeName.Real)
            //    pickType = NumberPickerType.Decimal;
            //numberEntryBox = new UDTNumberEntry
            //    (item.Name,
            //    maxNum,
            //    minNum,
            //    pickType,
            //    numberChanged);
            //numberBox = new NumberBox();
            //Binding myBinding = new Binding("NumberValue");
            //myBinding.Source = number;
            //numberBox.SetBinding(NumberBox.NumberValueProperty, myBinding);
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

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                if (editProps.defaultPicker.number != null)
                {
                    number = (Int32?)editProps.defaultPicker.number;
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
                    else if (udtItem.TypeName == UDTTypeName.Real)
                    {
                        decimal currentVal = (decimal)row[colName];
                        if (currentVal != (decimal)intNum)
                            row[colName] = intNum;
                    }
                }

            }
        }

    }
}
