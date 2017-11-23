using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace UDTAppControlLibrary.Controls
{
    public class NumberBoxBase : ContentControl
    {
        public NumberBoxBase() 
        {
           selChange = new RoutedEventHandler(selectionChange);
        }

        public TextBlock messageBox;
        public Popup messagePopup;
        protected TextBlock PreFormatBox;
        protected TextBlock PostFormatBox;
        protected TextBox txtBox;
        protected NumberText numberText = new NumberText();
        protected DcimalFromatProvider fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue) as DcimalFromatProvider;

        protected void updateSource(DependencyProperty dp)
        {
            BindingExpression exp = GetBindingExpression(dp);
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (exp != null) exp.UpdateSource();
                    //exp.UpdateSource();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        public override void OnApplyTemplate()
        {
            PreFormatBox = Template.FindName("preFormat", this) as TextBlock;
            txtBox = Template.FindName("textBox", this) as TextBox;
            PostFormatBox = this.GetTemplateChild("postFormat") as TextBlock;

            txtBox.PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            txtBox.PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
            //txtBox.SelectionChanged += new RoutedEventHandler(selectionChange);
            txtBox.SelectionChanged += selChange;
            eventCount++;

            PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
            PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;

            numberText.setPrompt(fromatProvider.prompt);

            messagePopup = new Popup();
            messagePopup.VerticalOffset = 18;
            messagePopup.HorizontalOffset = 5;
            messagePopup.Placement = PlacementMode.Center;
            messagePopup.AllowsTransparency = true;
            messagePopup.PlacementTarget = txtBox;

            messageBox = new TextBlock();
            messageBox.Background = Brushes.Transparent;
            messageBox.Foreground = Brushes.DarkOrange;
            messagePopup.Child = messageBox;

            ApplyTemplateComplete();
            updateTextBox();

            base.OnApplyTemplate();
        }

        virtual protected void setParsedNumber(dynamic value)
        { }

        virtual protected void ApplyTemplateComplete()
        { }

        virtual protected void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            arg.Handled = arg.Key == Key.Delete || arg.Key == Key.Back || arg.Key == Key.Space;
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

        virtual protected void previewTextInput(object src, TextCompositionEventArgs arg)
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

        event RoutedEventHandler selChange;
        int eventCount = 0;
        virtual protected void updateTextBox()
        {
            txtBox.SelectionChanged -= selChange;
            eventCount--;
            fromatProvider.fromatNumberText(numberText);
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;
                setParsedNumber( fromatProvider.parseNumber(numberText.numberString) );
            }
            txtBox.SelectionLength = numberText.selectionLength;
            txtBox.SelectionStart = numberText.selectionStart;
            if (eventCount == 0)
            { 
                txtBox.SelectionChanged += selChange;
                eventCount++;
            }

        }

        protected void selectionChange(object src, RoutedEventArgs arg)
        {
            //if (noSelChange) return;
            numberText.selectionStart = txtBox.SelectionStart;
            numberText.selectionLength = txtBox.SelectionLength;
        }

 

    }
}
