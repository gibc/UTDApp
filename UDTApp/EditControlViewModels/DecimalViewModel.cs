using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;
using UDTApp.ViewModels;
using UDTAppControlLibrary.Controls;

namespace UDTApp.EditControlViewModels
{
    public class DecimalViewModel : UDTDataBoxBase
    {
        public DecimalViewModel(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
            editProps = item.editProps;
            UDTDecimalEditProps props = item.editProps as UDTDecimalEditProps;
            fromatType = props.decimalFormat;
            decimal maxNum = Decimal.MaxValue;
            decimal minNum = Decimal.MinValue;
            if (editProps.maxPicker.number != null)
                maxNum = (Decimal)editProps.maxPicker.number;
            if (editProps.minPicker.number != null)
                minNum = (Decimal)editProps.minPicker.number;
        }

        // bind to DecimalBox control in data template
        private Decimal? _decimalNumber = null;        
        public Decimal? decimalNumber 
        {
            get { return _decimalNumber; }
            set 
            { 
                SetProperty(ref _decimalNumber, value); 
                numberChanged(value);
            }
        }

        private DecimalFormatType _fromatType = DecimalFormatType.Decimal;
        public DecimalFormatType fromatType
        {
            get { return _fromatType; }
            set { SetProperty(ref _fromatType, value); }
        }

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                if (editProps.defaultPicker.number != null)
                {
                    decimalNumber = (Decimal?)editProps.defaultPicker.number;
                }
                else decimalNumber = null;
            }
            else if (udtItem.GetType() == typeof(UDTDecimalItem))
            {
                Decimal? decVal = (Decimal?)row[colName];
                decimalNumber = decVal;
            }
        }


        private void numberChanged(Decimal? decNum)
        {
            if (editProps.required && decNum == null)
            {
                List<string> errLst = new List<string>();
                errLst.Add("Decmial entry is required.");
                SetErrors(() => this.decimalNumber, errLst);
            }
            else
            {
                SetErrors(() => this.decimalNumber, new List<string>());
            }
            validationChanged(HasErrors);
            if (!HasErrors)
            {
                if (row[colName] == DBNull.Value)
                {
                    row[colName] = decNum;
                }
                else
                {
                    if (udtItem.TypeName == UDTTypeName.Number)
                    {
                        int currentVal = (int)row[colName];
                        if (currentVal != (int)decNum)
                            row[colName] = decNum;
                    }
                    else if (udtItem.TypeName == UDTTypeName.Real)
                    {
                        Decimal? currentVal = (Decimal?)row[colName];
                        if (currentVal != (Decimal?)decNum)
                            row[colName] = decNum;
                    }
                }

            }
        }
   }
}
