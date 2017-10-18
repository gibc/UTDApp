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
        public static readonly DependencyProperty TextFormatProperty =
             DependencyProperty.Register("TextFormat", typeof(FormatType), typeof(NumberBox),
             new UIPropertyMetadata(new PropertyChangedCallback(OnMaskTextFormatPropertyChange)),
             null);

        static void OnMaskTextFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            NumberBox maskedDecimal = src as NumberBox;
            FormatType newType = (FormatType)args.NewValue;
            maskedDecimal.numberProvider = new MaskedNumberProvider<Decimal?>(newType, Decimal.MaxValue, Decimal.MinValue);
        }

        public FormatType TextFormat
        {
            get { return (FormatType)GetValue(TextFormatProperty); }
            set { SetValue(TextFormatProperty, value); }
        }

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

            PreFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.pre;
            PostFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.post;

            numberText.setPrompt(numberProvider.fromatProvider.prompt);
            updateTextBox();

            base.OnApplyTemplate();
        }

        private Decimal? _parsedNumber = null;
        private Decimal? parsedNumber
        {
            get { return _parsedNumber; }
            set 
            {

                if (value == null)
                {
                    PreFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.pre;
                    PostFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.post;
                }
                else if (_parsedNumber == null)
                {
                    if(value >= 0)
                    { 
                        PreFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.pre;
                        PostFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.post;
                    }
                    else
                    {
                        PreFormatBox.Text = numberProvider.fromatProvider.negativeNumberSymbol.pre;
                        PostFormatBox.Text = numberProvider.fromatProvider.negativeNumberSymbol.post;
                    }
                }
                else if (value < 0 && _parsedNumber >= 0)
                {
                    PreFormatBox.Text = numberProvider.fromatProvider.negativeNumberSymbol.pre;
                    PostFormatBox.Text = numberProvider.fromatProvider.negativeNumberSymbol.post;
                }
                else if (value >= 0 && _parsedNumber < 0)
                {
                    PreFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.pre;
                    PostFormatBox.Text = numberProvider.fromatProvider.positiveNumberSymbol.post;
                }

                _parsedNumber = value; 
            }
        }

        private void previewKeyDownEvent(object src, KeyEventArgs arg)
        {

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
                    else if (numberText.currentChar == '.' || numberText.currentChar == ',')
                    {
                        numberText.selectionStart = numberText.selectionStart + 1;
                        txtBox.SelectionStart = numberText.selectionStart;
                        return;
                    }
                    else numberText.deleteChar();
                }
            }

            if(arg.Handled)
            {
                if (numberText.numberString == "" || numberText.numberString == "." ||
                    numberText.numberString == "-" || numberText.numberString == "+")
                    numberText.setPrompt(numberProvider.fromatProvider.prompt);

                updateTextBox();
            }

           
            base.OnPreviewKeyDown(arg);

            //if (numberText.numberString == "" || numberText.numberString == ".")
            //    numberText.setPrompt(numberProvider.fromatProvider.prompt);

            //updateTextBox();
        }

        private bool replacePromptText(TextCompositionEventArgs arg, char c)
        {
            if (numberProvider.fromatProvider.type == FormatType.Currency)
            {
                numberText.insertString(".00");
                numberText.selectionStart = 0;

                if (c == '+' || c == '-') numberText.insertChar(c);
                if (Char.IsDigit(c)) numberText.insertChar(c);
                arg.Handled = true;
                updateTextBox();
                return true;
            }
            return false;
        }

        private bool isMaxDecimalDigits(TextCompositionEventArgs arg)
        {
            if (numberProvider.fromatProvider.type == FormatType.Currency)
            {
                int ptOffset = numberText.numberString.IndexOf('.');
                if (numberText.selectionStart > ptOffset + 2)
                {
                    arg.Handled = true;
                    return true;
                }
            }
            return false;
        }

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {

            char c = arg.Text[0];
            if((Char.IsDigit(c) || c == '+' || c == '-'|| c == '.'))
            {
                if(numberText.promptVisble)
                {
                    numberText.clear();
                    if (replacePromptText(arg, c)) return;
                    //if (numberProvider.fromatProvider.type == FormatType.Currency)
                    //{
                    //    numberText.insertString(".00");
                    //    numberText.selectionStart = 0;
                    
                    //    if (c == '+' || c == '-') numberText.insertChar(c);
                    //    if (Char.IsDigit(c)) numberText.insertChar(c);
                    //    arg.Handled = true;
                    //    updateTextBox(); 
                    //    return;
                    //}
                }

                //if (numberProvider.fromatProvider.type == FormatType.Currency)
                //{
                //    int ptOffset = numberText.numberString.IndexOf('.');
                //    if (numberText.selectionStart > ptOffset + 2)
                //    { 
                //        arg.Handled = true;
                //        return;
                //    }
                //}

                if (isMaxDecimalDigits(arg)) return;

                if(Char.IsDigit(c)) numberText.insertChar(c);

                if(c == '.' && numberProvider.fromatProvider.type != FormatType.Currency) 
                {
                    int ptOffset = numberText.numberString.IndexOf(c);
                    if(ptOffset >= 0)
                    {
                        numberText.deleteChar(ptOffset);
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
            if (!numberText.promptVisble)
            { 
                numberText.addCommas();
                if (numberProvider.fromatProvider.type == FormatType.Currency)
                    numberText.limitDecimaDigits(2);
            }
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;
                parsedNumber = numberProvider.fromatProvider.parseNumber(numberText.numberString);
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


        private void onLoad(object sender, RoutedEventArgs e) 
        {
            PreFormatBox.Text = ">>";
            PostFormatBox.Text = "<<";
        }

        //private void updateTextBox
    }
}
