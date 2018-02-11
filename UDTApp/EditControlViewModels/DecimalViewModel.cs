using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;
using UDTApp.SchemaModels;
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
            maxValue = Decimal.MaxValue;
            minValue = Decimal.MinValue;
            if (props.maxValue != null)
                maxValue = (Decimal)props.maxValue;
            if (props.minValue != null)
                minValue = (Decimal)props.minValue;
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

        private Decimal _maxValue = Decimal.MaxValue;
        public Decimal maxValue
        {
            get { return _maxValue; }
            set { SetProperty(ref _maxValue, value); }
        }

        private Decimal _minValue = Decimal.MinValue;
        public Decimal minValue
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
                    decimalNumber = (Decimal?)editProps.defaultValue;
                }
                else decimalNumber = null;
            }
            else if (udtItem.GetType() == typeof(UDTDecimalItem))
            {
                Decimal? decVal = (Decimal?)row[colName];
                //var decVal = row[colName];
                decimalNumber = decVal;
            }
        }


        private void numberChanged(Decimal? decNum)
        {
            if (decNum == null) row[colName] = DBNull.Value;
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
                    if(decNum == null)
                        row[colName] = DBNull.Value;
                    else
                        row[colName] = decNum;
                }
                else
                {
                    if (udtItem.TypeName == UDTTypeName.Real)
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
