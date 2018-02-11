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
    public class DateViewModel : UDTDataBoxBase
    {
        public DateViewModel(string _colName, UDTBase item, Action<bool> _validationChanged)
        {
            colName = _colName;
            udtItem = item;
            validationChanged = _validationChanged;
            editProps = item.editProps;
            dateEditProps = item.editProps as UDTDateEditProps;
            dateDefault = dateEditProps.defaultDate;
            dateFormat = dateEditProps.dateFormat;
            if (dateEditProps.maxDate != null)
                maxValue = editProps.maxDate;
            if (dateEditProps.minDate != null)
                minValue = editProps.minDate;
        }

        // bind to DateBox control in data template
        private DateTime? _dateNumber = null;
        public DateTime? dateNumber
        {
            get { return _dateNumber; }
            set
            {
                SetProperty(ref _dateNumber, value);
                numberChanged(value);
            }
        }

        private DateTime _maxValue = DateTime.MaxValue;
        public DateTime maxValue
        {
            get { return _maxValue; }
            set { SetProperty(ref _maxValue, value); }
        }

        private DateTime _minValue = DateTime.MinValue;
        public DateTime minValue
        {
            get { return _minValue; }
            set { SetProperty(ref _minValue, value); }
        }

        private DateTimeDefault _dateDefault = DateTimeDefault.None;
        public DateTimeDefault dateDefault
        {
            get { return _dateDefault; }
            set { SetProperty(ref _dateDefault, value); }
        }

        private DateTimeFormat _dateFormat = DateTimeFormat.Date_Only;
        public DateTimeFormat dateFormat
        {
            get { return _dateFormat; }
            set { SetProperty(ref _dateFormat, value); }
        }

        override protected void setColumn()
        {
            if (row[colName] == DBNull.Value)
            {
                if (dateEditProps.defaultDate != DateTimeDefault.None)
                {
                    //_dateNumber = (DateTime?)editProps.defaultPicker.number;
                }
                else dateNumber = null;
            }
            else if (udtItem.GetType() == typeof(UDTDateItem))
            {
                DateTime? dateVal = (DateTime?)row[colName];
                _dateNumber = dateVal;
            }
        }

        private UDTDateEditProps dateEditProps = null;

        private void numberChanged(DateTime? dateNum)
        {
            if (dateNum == null) row[colName] = DBNull.Value;
            if (editProps.required && dateNum == null)
            {
                List<string> errLst = new List<string>();
                errLst.Add("Date entry is required.");
                SetErrors(() => this.dateNumber, errLst);
            }
            else
            {
                SetErrors(() => this.dateNumber, new List<string>());
            }
            validationChanged(HasErrors);
            if (!HasErrors)
            {
                if (row[colName] == DBNull.Value)
                {
                    if(dateNum == null)
                        row[colName] = DBNull.Value;
                    else
                        row[colName] = dateNum;
                }
                else if (udtItem.TypeName == UDTTypeName.Date)
                {
                    DateTime? currentVal = (DateTime?)row[colName];
                    if (currentVal != (DateTime?)dateNum)
                        row[colName] = dateNum;
                }
            }
        }
    }
}
