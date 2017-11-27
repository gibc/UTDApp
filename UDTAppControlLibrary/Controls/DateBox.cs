using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UDTAppControlLibrary.Controls.UDTAppControlLibrary.Controls;

namespace UDTAppControlLibrary.Controls
{
    public enum DateTimeFormat { Date_Only = 1, Date_12_HourTime, Date_24_HourTime};
    public enum DateTimeDefault { CurrentDay = 1, CurrentWeek, CurrentMonth, CurrentYear, None }
    public static class TimeDefault
    {
        static public DateTime? DateTimeValue(DateTimeDefault defaultTime)
        {
            DateTime? dateEntry = DateTime.Now;
            if (defaultTime == DateTimeDefault.CurrentDay)
            {
                dateEntry = DateTime.Now;
            }
            else if (defaultTime == DateTimeDefault.CurrentWeek)
            {
                //DateTime now = DateTime.Now;
                //int pastSunday = 6 - (int)now.DayOfWeek;
                //dateEntry = new DateTime(now.Year, now.Month, now.Day - pastSunday);

                dateEntry = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            }
            else if (defaultTime == DateTimeDefault.CurrentMonth)
            {
                DateTime now = DateTime.Now;
                dateEntry = new DateTime(now.Year, now.Month, 1);
            }
            else if (defaultTime == DateTimeDefault.CurrentYear)
            {
                DateTime now = DateTime.Now;
                dateEntry = new DateTime(now.Year, 1, 1);
            }
            else if (defaultTime == DateTimeDefault.None)
            {
                dateEntry = null;
            }
            return dateEntry;
        }
    }

    public class DateIndex
    {
        public const int month = 0; public const int day = 3; public const int year = 6;
        public const int hour = 11; public const int minute = 14; public const int meridiem = 17;
    }
    public class DateBox : NumberBoxBase
    {
        public static readonly DependencyProperty DateTimeValueProperty =
         DependencyProperty.Register("DateTimeValue", typeof(DateTime?), typeof(DateBox),
         new UIPropertyMetadata
             ((DateTime?)null, new PropertyChangedCallback(DateTimeValuePropertyChange)/*,
             new CoerceValueCallback(CoerceCurrentReading)*/),
         null);

        //private static object CoerceCurrentReading(DependencyObject d, object value)
        //{
        //    DateBox dateBox = (DateBox)d;
        //    DateTime? current = (DateTime?)value;
        //    if(current == null && dateBox.DateTimeDefault != DateTimeDefault.None)
        //    {
        //        current = TimeDefault.DateTimeValue(dateBox.DateTimeDefault);
        //        //BindingExpression exp = dateBox.GetBindingExpression(DateBox.DateTimeValueProperty);
        //        //exp.UpdateSource();

        //    }
        //    return current;
        //}

        static void DateTimeValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DateBox dateBox = src as DateBox;
            DateTime? newDate = (DateTime?)args.NewValue;
            if (dateBox.txtBox == null) return;
            if (dateBox.DateFormat == 0) return;
            if (newDate != dateBox.parsedNumber /*|| (newDate == null && !dateBox.numberText.promptVisble)*/)
            {

                if (newDate == null)
                {
                    if (dateBox.DateTimeDefault != DateTimeDefault.None)
                    {
                        dateBox.DateTimeValue = TimeDefault.DateTimeValue(dateBox.DateTimeDefault);
                        var t = Task.Run(() => dateBox.updateSource(DateTimeValueProperty));
                        return;
                    }
                    else
                    { 
                        dateBox.numberText.setPrompt(dateBox.fromatProvider.prompt);
                        dateBox.updateTextBox();
                        return;
                    }
                }

                string numTxt = dateBox.fromatProvider.getNumberText(newDate);

                dateBox.numberText.clear();
                dateBox.numberText.insertString(numTxt);
                dateBox.updateTextBox();
            }
        }

        public DateTime? DateTimeValue
        {
            get { return (DateTime?)GetValue(DateTimeValueProperty); }
            set { SetValue(DateTimeValueProperty, value); }
        }

        public static readonly DependencyProperty DateFormatProperty =
         DependencyProperty.Register("DateFormat", typeof(DateTimeFormat), typeof(DateBox),
         new UIPropertyMetadata(new PropertyChangedCallback(DateTimeFormatPropertyChange)),
         null);

        static void DateTimeFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DateBox dateBox = src as DateBox;
            DateTimeFormat newType = (DateTimeFormat)args.NewValue;
            dateBox.fromatProvider = new DateTimeProvider(newType, dateBox.MaxValue, dateBox.MinValue);
        }

        public DateTimeFormat DateFormat
        {
            get { return (DateTimeFormat)GetValue(DateFormatProperty); }
            set { SetValue(DateFormatProperty, value); }
        }

        public static readonly DependencyProperty DateTimeDefaultProperty =
             DependencyProperty.Register("DateTimeDefault", typeof(DateTimeDefault), typeof(DateBox),
             new UIPropertyMetadata(DateTimeDefault.None, new PropertyChangedCallback(DateTimeDefaultPropertyChange)),
             null);

        static void DateTimeDefaultPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DateBox dateBox = src as DateBox;
            DateTimeDefault newDefault = (DateTimeDefault)args.NewValue;
            if (dateBox.txtBox == null) return;
            if (dateBox.DateTimeDefault != DateTimeDefault.None && dateBox.DateTimeValue == null)
            {
                DateTime? dateEntry = TimeDefault.DateTimeValue(dateBox.DateTimeDefault);
                dateBox.DateTimeValue = dateEntry;
                Task.Run(() => dateBox.updateSource(DateTimeValueProperty));
            }
        }

        public DateTimeDefault DateTimeDefault
        {
            get { return (DateTimeDefault)GetValue(DateTimeDefaultProperty); }
            set { SetValue(DateTimeDefaultProperty, value); }
        }

        #region MinMax
        public static readonly DependencyProperty MaxValueProperty =
         DependencyProperty.Register("MaxValue", typeof(DateTime), typeof(DateBox),
         new UIPropertyMetadata(DateTime.MaxValue,
             new PropertyChangedCallback(OnMaxValuePropertyChange),
             null));

        static void OnMaxValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DateBox dateBox = src as DateBox;
            DateTime newValue = (DateTime)args.NewValue;
            if (dateBox.fromatProvider != null)
            {
                dateBox.fromatProvider.numberMax = newValue;
            }
        }

        public DateTime MaxValue
        {
            get { return (DateTime)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
         DependencyProperty.Register("MinValue", typeof(DateTime), typeof(DateBox),
         new UIPropertyMetadata(DateTime.MinValue,
             new PropertyChangedCallback(OnMinValuePropertyChange),
             null));

        static void OnMinValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DateBox dateBox = src as DateBox;
            DateTime newValue = (DateTime)args.NewValue;
            if (dateBox.fromatProvider != null)
            {
                dateBox.fromatProvider.numberMin = newValue;
            }
        }

        public DateTime MinValue
        {
            get { return (DateTime)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        #endregion

        static DateBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateBox), new FrameworkPropertyMetadata(typeof(DateBox)));
        }

        public DateBox()
        {
            //fromatProvider = new DateTimeProvider(DateTimeFormat.Date_Only, DateTime.MaxValue, DateTime.MinValue);
        }

        override protected void ApplyTemplateComplete()
        {
            DateTimeDefault defalutVal = DateTimeDefault;
            DateTime? dateVal = DateTimeValue;
            DateTimeFormat dateFmt = DateFormat;
            if (dateVal == null && defalutVal != DateTimeDefault.None)
            {
                DateTimeDefault = DateTimeDefault.None;
                DateTimeDefault = defalutVal;
            }
            else if (dateVal != null && dateVal != parsedNumber)
            {
                DateTimeValue = null;
                DateTimeValue = dateVal;
            }
        }


        DateTime? parsedNumber = null;
        override protected void setParsedNumber(dynamic value)
        {
            if (fromatProvider.isMax)
            {
                messageBox.Text = "Maximum date.";
                messagePopup.IsOpen = true;
            }
            else if (fromatProvider.isMin)
            {
                messageBox.Text = "Minimum date.";
                messagePopup.IsOpen = true;
            }
            else messagePopup.IsOpen = false;

            parsedNumber = value;
            if (parsedNumber != DateTimeValue)
                DateTimeValue = parsedNumber;
        }

        private bool dateComplete
        {
            get 
            {
                bool dateComplete = numberText.month != null &&
                    numberText.day != null &&
                    numberText.year != null;
                if(DateFormat == DateTimeFormat.Date_Only)
                {
                    return dateComplete;
                }
                if (DateFormat == DateTimeFormat.Date_12_HourTime)
                {
                    return dateComplete &&
                    numberText.hour != null &&
                    numberText.minute != null &&
                    numberText.meridiem != null;
                }
                if (DateFormat == DateTimeFormat.Date_24_HourTime)
                {
                    return dateComplete &&
                    numberText.hour != null &&
                    numberText.minute != null;
                }
                return false;
            }
        }
        protected override void updateTextBox()
        {
            txtBox.SelectionChanged -= new RoutedEventHandler(selectionChange);
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;

                if(!dateComplete && !numberText.promptVisble)
                {
                    txtBox.Foreground = Brushes.DarkOrange;
                    txtBox.FontWeight = FontWeights.Bold;
                }
                else
                {
                    txtBox.FontWeight = FontWeights.Normal;
                    txtBox.Foreground = Brushes.Black;
                }

                if (dateComplete)
                {
                    //setParsedNumber(fromatProvider.parseNumber(numberText.numberString));
                    dynamic number = fromatProvider.parseNumber(numberText.numberString);
                    if (fromatProvider.isMax || fromatProvider.isMin)
                    {
                        numberText.clear();
                        numberText.insertString(fromatProvider.getNumberText(number));
                    }
                    txtBox.Text = numberText.numberString;
                    setParsedNumber(number);
                }
                else
                {
                    setParsedNumber(null);
                }
            }
            txtBox.SelectionLength = numberText.selectionLength;
            txtBox.SelectionStart = numberText.selectionStart;
            txtBox.SelectionChanged += new RoutedEventHandler(selectionChange);
        }

        protected override void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            arg.Handled = arg.Key == Key.Delete || arg.Key == Key.Back
                || arg.Key == Key.Space || arg.Key == Key.Return;
            if (!numberText.promptVisble)
            {
                if (arg.Key == Key.Back)
                {
                    if (numberText.previousChar == '/' || numberText.previousChar == ':'
                        || numberText.previousChar == 'M' || numberText.previousChar == '\n')
                    {
                        numberText.selectionStart = numberText.selectionStart - 1;
                    }
                    else
                    {
                        if (numberText.selectionStart > 0)
                        {
                            numberText.repalceChar(' ', numberText.selectionStart-1);
                        }
                    }
                    updateTextBox();
                }

                if (arg.Key == Key.Delete)
                {
                    if (numberText.selectionLength > 0)
                    {
                        fromatProvider.deleteSelection(numberText);
                    }
                    else if (numberText.currentChar == '/' || numberText.currentChar == ':'
                        || numberText.currentChar == 'M' || numberText.currentChar == '\n')
                    {
                        if (numberText.selectionStart < DateIndex.meridiem)
                            numberText.selectionStart = numberText.selectionStart + 1;
                        txtBox.SelectionStart = numberText.selectionStart;
                        return;
                    }
                    else
                    {
                        numberText.repalceChar(' ', numberText.selectionStart);
                        if (numberText.selectionStart < DateIndex.meridiem)
                            //numberText.selectionStart = numberText.selectionStart + 1;
                            numberText.selectionStart = numberText.selectionStart;
                        if (numberText.currentChar == '\n') numberText.selectionStart++;
                    }
                    updateTextBox();
                }

                if (numberText.numberString == "  /  /    ")
                { 
                    numberText.setPrompt(fromatProvider.prompt);
                    updateTextBox();
                }
            }

            if (arg.Key == Key.Space)
            {
                if (numberText.promptVisble)
                {
                    fromatProvider.replacePromptText(numberText, null, ' ');
                }
                fromatProvider.insertDateSperator(numberText);
                updateTextBox();
            }

            if (arg.Key == Key.Return)
            {
                if (numberText.promptVisble)
                {
                    fromatProvider.replacePromptText(numberText, null, ' ');
                }
                numberText.clear();
                if (DateFormat == DateTimeFormat.Date_Only)
                    numberText.insertString(DateTime.Now.ToShortDateString());
                if (DateFormat == DateTimeFormat.Date_12_HourTime)
                    numberText.insertString(DateTime.Now.ToShortDateString() + "\n" + DateTime.Now.ToString("hh:mm:tt"));
                if (DateFormat == DateTimeFormat.Date_24_HourTime)
                    numberText.insertString(DateTime.Now.ToShortDateString() + "\n" + DateTime.Now.ToString("HH:mm:tt"));
                updateTextBox();
            }


        }

        protected override void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            char c = arg.Text[0];
            if (Char.IsDigit(c) || c == '/' || c == '-' || 
                c == 'a' || c == 'p' || c == 'A' || c == 'P')
            {
                if (numberText.promptVisble)
                {
                    arg.Handled = true;
                    if (Char.IsDigit(c))
                    {
                        fromatProvider.replacePromptText(numberText, arg, c);
                        if (arg.Handled)
                        {
                            updateTextBox();
                        }
                    }
                    return; 
                }

                fromatProvider.deleteSelection(numberText);

                if (Char.IsDigit(c)) fromatProvider.insertDigit(numberText, c);
                if (c == '/' || c == '-') fromatProvider.insertDateSperator(numberText);
                if (c == 'a' || c == 'p' || c == 'A' || c == 'P') 
                    fromatProvider.insertLetter(numberText, c);

           }

            arg.Handled = true;

            updateTextBox();
        }

    }
}

