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
    public class NumberBox : ContentControl
    {
        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        private TextBlock PreFormatBox;
        private TextBlock PostFormatBox;
        private TextBox txtBox;
        private NumberText numberText = new NumberText();
        private MaskedNumberProvider<Decimal?> numberProvider = new MaskedNumberProvider<Decimal?>(FormatType.Decimal, Decimal.MaxValue, Decimal.MinValue);
        public override void OnApplyTemplate()
        {
            PreFormatBox = Template.FindName("preFormat", this) as TextBlock;
            txtBox = Template.FindName("textBox", this) as TextBox;
            PostFormatBox = this.GetTemplateChild("postFormat") as TextBlock;

            txtBox.PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            txtBox.PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
            txtBox.SelectionChanged += new RoutedEventHandler(selectionChange);
 
            numberText.setPrompt(numberProvider.fromatProvider.prompt);
            updateTextBox();

            base.OnApplyTemplate();
        }

        private void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            //numberProvider.fromatProvider.previewKeyDown(txtBox, arg);
            //base.OnPreviewKeyDown(arg);

            if (!numberText.promptVisble)
            {
                if (arg.Key == Key.Back)
                {
                    arg.Handled = true;
                    if (numberText.previousChar == '.')
                    {
                        numberText.selectionStart = numberText.selectionStart - 1;
                    }
                    else
                    {
                        if(numberText.selectionStart > 0)
                        { 
                            numberText.selectionStart = numberText.selectionStart - 1;
                            numberText.deleteChar(); 
                        }
                    }
                }

                if (arg.Key == Key.Delete)
                {
                    arg.Handled = true;
                    if(numberText.selectionLength > 0)
                    {
                        numberText.deleteString();
                    }
                    else if (numberText.currentChar == '.')
                    {
                        numberText.selectionStart = numberText.selectionStart + 1;
                    }
                    else numberText.deleteChar();
                }
            }

           
            base.OnPreviewKeyDown(arg);

            updateTextBox();
        }

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            //numberProvider.previewText(txtBox, arg);

            char c = arg.Text[0];
            if((Char.IsDigit(c) || c == '+' || c == '-'|| c == '.'))
            {
                if(numberText.promptVisble)
                {
                    numberText.clear();
                }
                if(Char.IsDigit(c)) numberText.insertChar(c);
                if(c == '.') 
                {
                    int ptOffset = numberText.numberString.IndexOf(c);
                    if(ptOffset > 0)
                    {
                        numberText.numberString.Replace(".", "");
                        if(ptOffset < numberText.selectionStart)
                        {
                            numberText.selectionStart = numberText.selectionStart - 1;
                        }
                    }
                    numberText.insertChar(c);
                }
                if(c == '+' || c == '-' && numberText.selectionStart == 0)
                {
                    numberText.insertChar(c);
                }
            }

            arg.Handled = true;

            updateTextBox();

        }

        private void updateTextBox()
        {
            txtBox.SelectionChanged -= new RoutedEventHandler(selectionChange);
            if (!numberText.promptVisble) numberText.addCommas();
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;
            }
            txtBox.CaretIndex = numberText.selectionStart;
            txtBox.SelectionLength = numberText.selectionLength;
            txtBox.SelectionChanged += new RoutedEventHandler(selectionChange);
        }

        private void selectionChange(object src, RoutedEventArgs arg)
        {
            numberText.selectionStart = txtBox.SelectionStart;
            numberText.selectionLength = txtBox.SelectionLength;
        }


        private void onLoad(object sender, RoutedEventArgs e) 
        {
            PreFormatBox.Text = ">>";
            PostFormatBox.Text = "<<";
        }

        //private void updateTextBox
    }
}
