using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
    public enum DateTimeFormat { Date_Only = 1, Date_12_HourTime, Date_24_HourTime};

    public class DateBox : NumberBoxBase
    {
        public static readonly DependencyProperty DateTimeFormatProperty =
         DependencyProperty.Register("DateTimeFormat", typeof(DateTimeFormat), typeof(DateBox),
         new UIPropertyMetadata(new PropertyChangedCallback(DateTimeFormatPropertyChange)),
         null);

        static void DateTimeFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            //DecimalBox maskedDecimal = src as DecimalBox;
            //FormatType newType = (FormatType)args.NewValue;
            //if (newType == FormatType.Decimal)
            //    maskedDecimal.fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            //else if (newType == FormatType.Currency)
            //    maskedDecimal.fromatProvider = new CurrencyFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            //else if (newType == FormatType.Percent)
            //    maskedDecimal.fromatProvider = new PercentFromatProvider(Decimal.MaxValue, Decimal.MinValue);
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
            fromatProvider = new DateTimeProvider(DateTime.MaxValue, DateTime.MinValue);
        }

        DateTime? parsedNumber = null;
        override protected void setParsedNumber(dynamic value)
        {
            // TBD:  set bound number property if changed
            parsedNumber = value;
        }

        protected override void updateTextBox()
        {
            txtBox.SelectionChanged -= new RoutedEventHandler(selectionChange);
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;
                if (numberText.month != null &&
                    numberText.day != null &&
                    numberText.year != null)
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
                    if (numberText.previousChar == '/')
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
                    else if (numberText.currentChar == '/')
                    {
                        if (numberText.selectionStart < 9)
                            numberText.selectionStart = numberText.selectionStart + 1;
                        txtBox.SelectionStart = numberText.selectionStart;
                        return;
                    }
                    else
                    {
                        numberText.repalceChar(' ', numberText.selectionStart);
                        if (numberText.selectionStart < 9)
                            numberText.selectionStart = numberText.selectionStart + 1;
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
                    fromatProvider.replacePromptText(numberText, null, ' ', DateFormat);
                }
                fromatProvider.insertDateSperator(numberText);
                updateTextBox();
            }

            if (arg.Key == Key.Return)
            {
                if (numberText.promptVisble)
                {
                    fromatProvider.replacePromptText(numberText, null, ' ', DateFormat);
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
            if ((Char.IsDigit(c) || c == '/' || c == '-'))
            {
                if (numberText.promptVisble)
                {
                    arg.Handled = true;
                    if (Char.IsDigit(c))
                    {
                        fromatProvider.replacePromptText(numberText, arg, c, DateFormat);
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

           }

            arg.Handled = true;

            updateTextBox();
        }

    }
}

