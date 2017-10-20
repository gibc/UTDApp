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
    public class NumberBoxBase : ContentControl
    {
        public NumberBoxBase() 
        {
        }

        protected TextBlock PreFormatBox;
        protected TextBlock PostFormatBox;
        protected TextBox txtBox;
        protected NumberText numberText = new NumberText();
        protected DcimalFromatProvider fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue) as DcimalFromatProvider;
        
        public override void OnApplyTemplate()
        {
            PreFormatBox = Template.FindName("preFormat", this) as TextBlock;
            txtBox = Template.FindName("textBox", this) as TextBox;
            PostFormatBox = this.GetTemplateChild("postFormat") as TextBlock;

            txtBox.PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            txtBox.PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
            txtBox.SelectionChanged += new RoutedEventHandler(selectionChange);

            PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
            PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;

            numberText.setPrompt(fromatProvider.prompt);
            updateTextBox();

            base.OnApplyTemplate();
        }

        virtual protected void setParsedNumber(dynamic value)
        { }

        private void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            arg.Handled = arg.Key == Key.Delete || arg.Key == Key.Back;
            if (!numberText.promptVisble)
            {
                if (arg.Key == Key.Back)
                {
                    if (numberText.previousChar == '.')
                    {
                        numberText.selectionStart = numberText.selectionStart - 1;
                    }
                    else
                    {
                        if (numberText.selectionStart > 0)
                        {
                            numberText.selectionStart = numberText.selectionStart - 1;
                            numberText.deleteChar();
                        }
                    }
                }

                if (arg.Key == Key.Delete)
                {
                    if (numberText.selectionLength > 0)
                    {
                        fromatProvider.deleteSelection(numberText);
                    }
                    else if (numberText.currentChar == '.' || numberText.currentChar == ',')
                    {
                        numberText.selectionStart = numberText.selectionStart + 1;
                        txtBox.SelectionStart = numberText.selectionStart;
                        return;
                    }
                    else numberText.deleteChar();
                }
            }

            if (arg.Handled)
            {
                if (numberText.numberString == "" || numberText.numberString == "." ||
                    numberText.numberString == "-" || numberText.numberString == "+")
                    numberText.setPrompt(fromatProvider.prompt);

                updateTextBox();
            }

            base.OnPreviewKeyDown(arg);

        }

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            char c = arg.Text[0];
            if ((Char.IsDigit(c) || c == '+' || c == '-' || c == '.'))
            {
                if (numberText.promptVisble)
                {
                    fromatProvider.replacePromptText(numberText, arg, c);
                    if (arg.Handled)
                    {
                        updateTextBox();
                        return;
                    }
                }

                fromatProvider.deleteSelection(numberText);

                if (Char.IsDigit(c)) fromatProvider.insertDigit(numberText, c);

                if (c == '+' || c == '-' && numberText.selectionStart == 0)
                {
                    fromatProvider.insertSign(numberText, c);
                }

                if (c == '.')
                {
                   fromatProvider.insertDecimal(numberText, c);
                }
            }

            arg.Handled = true;

            updateTextBox();

        }

        private void updateTextBox()
        {
            txtBox.SelectionChanged -= new RoutedEventHandler(selectionChange);
            fromatProvider.fromatNumberText(numberText);
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;
                setParsedNumber( fromatProvider.parseNumber(numberText.numberString) );
            }
            txtBox.SelectionLength = numberText.selectionLength;
            txtBox.SelectionStart = numberText.selectionStart;
            txtBox.SelectionChanged += new RoutedEventHandler(selectionChange);
        }

        private void selectionChange(object src, RoutedEventArgs arg)
        {
            numberText.selectionStart = txtBox.SelectionStart;
            numberText.selectionLength = txtBox.SelectionLength;
        }

    }
}
