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
    public class DateBox : NumberBoxBase
    {

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
            arg.Handled = arg.Key == Key.Delete || arg.Key == Key.Back || arg.Key == Key.Space;
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

                if(arg.Key == Key.Space)
                {
                    fromatProvider.insertDateSperator(numberText);
                    updateTextBox();
                }

                if (numberText.numberString == "  /  /    ")
                { 
                    numberText.setPrompt(fromatProvider.prompt);
                    updateTextBox();
                }
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

           }

            arg.Handled = true;

            updateTextBox();
        }

    }
}

