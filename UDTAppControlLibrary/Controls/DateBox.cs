using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UDTAppControlLibrary.Controls.UDTAppControlLibrary.Controls;

namespace UDTAppControlLibrary.Controls
{
    public enum DateTimeFormat { Date_Only = 1, Date_12_HourTime, Date_24_HourTime};
    public class DateIndex
    {
        public const int month = 0; public const int day = 3; public const int year = 6;
        public const int hour = 11; public const int minute = 14; public const int meridiem = 17;
    }
    public class DateBox : NumberBoxBase
    {
        public static readonly DependencyProperty DateTimeFormatProperty =
         DependencyProperty.Register("DateTimeFormat", typeof(DateTimeFormat), typeof(DateBox),
         new UIPropertyMetadata(new PropertyChangedCallback(DateTimeFormatPropertyChange)),
         null);

        static void DateTimeFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DateBox dateBox = src as DateBox;
            DateTimeFormat newType = (DateTimeFormat)args.NewValue;
            dateBox.fromatProvider = new DateTimeProvider(newType, DateTime.MaxValue, DateTime.MinValue);
        }

        public DateTimeFormat DateFormat
        {
            get { return (DateTimeFormat)GetValue(DateTimeFormatProperty); }
            set { SetValue(DateTimeFormatProperty, value); }
        }

        static DateBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateBox), new FrameworkPropertyMetadata(typeof(DateBox)));
        }

        public DateBox()
        {
            fromatProvider = new DateTimeProvider(DateTimeFormat.Date_Only, DateTime.MaxValue, DateTime.MinValue);
        }

        DateTime? parsedNumber = null;
        override protected void setParsedNumber(dynamic value)
        {
            // TBD:  set bound number property if changed
            parsedNumber = value;
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
                if (dateComplete)
                    setParsedNumber(fromatProvider.parseNumber(numberText.numberString));
                else
                    setParsedNumber(null);
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
                            numberText.selectionStart = numberText.selectionStart + 1;
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

