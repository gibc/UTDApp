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
    public class DecimalBox : NumberBoxBase//  ContentControl
    {
        public static readonly DependencyProperty TextFormatProperty =
             DependencyProperty.Register("TextFormat", typeof(FormatType), typeof(DecimalBox),
             new UIPropertyMetadata(new PropertyChangedCallback(OnMaskTextFormatPropertyChange)),
             null);

        static void OnMaskTextFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DecimalBox maskedDecimal = src as DecimalBox;
            FormatType newType = (FormatType)args.NewValue;
            if (newType == FormatType.Decimal)
                maskedDecimal.fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            else if (newType == FormatType.Currency)
                maskedDecimal.fromatProvider = new CurrencyFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            else if (newType == FormatType.Percent)
                maskedDecimal.fromatProvider = new PercentFromatProvider(Decimal.MaxValue, Decimal.MinValue);
        }

        public FormatType TextFormat
        {
            get { return (FormatType)GetValue(TextFormatProperty); }
            set { SetValue(TextFormatProperty, value); }
        }

        static DecimalBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalBox), new FrameworkPropertyMetadata(typeof(DecimalBox)));
        }

        Decimal? parsedNumber = null;
        override protected void setParsedNumber(dynamic value)
        {
            if (value == null)
            {
                PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
                PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;
            }
            else if (parsedNumber == null)
            {
                if (value >= 0)
                {
                    PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
                    PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;
                }
                else
                {
                    PreFormatBox.Text = fromatProvider.negativeNumberSymbol.pre;
                    PostFormatBox.Text = fromatProvider.negativeNumberSymbol.post;
                }
            }
            else if (value < 0 && parsedNumber >= 0)
            {
                PreFormatBox.Text = fromatProvider.negativeNumberSymbol.pre;
                PostFormatBox.Text = fromatProvider.negativeNumberSymbol.post;
            }
            else if (value >= 0 && parsedNumber < 0)
            {
                PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
                PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;
            }

            parsedNumber = value;
        }

        /*
        private TextBlock PreFormatBox;
        private TextBlock PostFormatBox;
        private TextBox txtBox;
        private NumberText numberText = new NumberText();
        private DcimalFromatProvider fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue) as DcimalFromatProvider;
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

        private Decimal? _parsedNumber = null;
        private Decimal? parsedNumber
        {
            get { return _parsedNumber; }
            set 
            {

                if (value == null)
                {
                    PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
                    PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;
                }
                else if (_parsedNumber == null)
                {
                    if(value >= 0)
                    {
                        PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
                        PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;
                    }
                    else
                    {
                        PreFormatBox.Text = fromatProvider.negativeNumberSymbol.pre;
                        PostFormatBox.Text = fromatProvider.negativeNumberSymbol.post;
                    }
                }
                else if (value < 0 && _parsedNumber >= 0)
                {
                    PreFormatBox.Text = fromatProvider.negativeNumberSymbol.pre;
                    PostFormatBox.Text = fromatProvider.negativeNumberSymbol.post;
                }
                else if (value >= 0 && _parsedNumber < 0)
                {
                    PreFormatBox.Text = fromatProvider.positiveNumberSymbol.pre;
                    PostFormatBox.Text = fromatProvider.positiveNumberSymbol.post;
                }

                _parsedNumber = value; 
            }
        }

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
                        //if (!deleteSelectionWithPt())
                        //    numberText.deleteString();
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

            if(arg.Handled)
            {
                if (numberText.numberString == "" || numberText.numberString == "." ||
                    numberText.numberString == "-" || numberText.numberString == "+")
                    numberText.setPrompt(fromatProvider.prompt);

                updateTextBox();
            }
     
            base.OnPreviewKeyDown(arg);

        }

        //private bool replacePromptText(TextCompositionEventArgs arg, char c)
        //{
        //    if (fromatProvider.type == FormatType.Currency)
        //    {
        //        numberText.insertString(".00");
        //        numberText.selectionStart = 0;

        //        if (c == '+' || c == '-') numberText.insertChar(c);
        //        if (Char.IsDigit(c)) numberText.insertChar(c);
        //        arg.Handled = true;
        //        updateTextBox();
        //        return true;
        //    }
        //    return false;
        //}

        //private bool isMaxDecimalDigits(TextCompositionEventArgs arg)
        //{
        //    if (fromatProvider.type == FormatType.Currency)
        //    {
        //        int ptOffset = numberText.numberString.IndexOf('.');
        //        if (numberText.selectionStart > ptOffset + 2)
        //        {
        //            arg.Handled = true;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private bool allowDecimalInsert(TextCompositionEventArgs arg)
        //{
        //    if(fromatProvider.type == FormatType.Currency)
        //    { 
        //        arg.Handled = true;
        //        updateTextBox();
        //        return false;
        //    }
        //    return true;
        //}

        //private bool deleteSelectionWithPt()
        //{
        //    if(fromatProvider.type == FormatType.Currency)
        //    {
        //        int offset = numberText.numberString.IndexOf('.');
        //        if (offset >= numberText.selectionStart && 
        //            offset <= numberText.selectionStart + numberText.selectionLength)
        //        {
        //            numberText.deleteString();
        //            numberText.insertChar('.');
        //            numberText.selectionStart--;
        //            return true;
        //        }
        //        else return false;
        //    }
        //    return false;
        //}

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {

            char c = arg.Text[0];
            if((Char.IsDigit(c) || c == '+' || c == '-'|| c == '.'))
            {
                if(numberText.promptVisble)
                {
                    //numberText.clear();
                    //if (replacePromptText(arg, c)) return;
                    fromatProvider.replacePromptText(numberText, arg, c);
                    if (arg.Handled)
                    {
                        updateTextBox();
                        return; 
                    }
                }

                fromatProvider.deleteSelection(numberText);

                //if (isMaxDecimalDigits(arg)) return;

                if (Char.IsDigit(c)) fromatProvider.insertDigit(numberText, c);//numberText.insertChar(c);

                if(c == '+' || c == '-' && numberText.selectionStart == 0)
                {
                    //numberText.insertChar(c);
                    fromatProvider.insertSign(numberText, c);
                }

                //if (!allowDecimalInsert(arg)) return;

                if (c == '.')
                {
                    //int ptOffset = numberText.numberString.IndexOf(c);
                    //if (ptOffset >= 0)
                    //{
                    //    numberText.deleteChar(ptOffset);
                    //}
                    //numberText.insertChar(c);
                    fromatProvider.insertDecimal(numberText, c);
                }
            }

            arg.Handled = true;

            updateTextBox();

        }

        private void updateTextBox()
        {
            txtBox.SelectionChanged -= new RoutedEventHandler(selectionChange);
            //if (!numberText.promptVisble)
            //{ 
            //    numberText.addCommas();
            //    if (fromatProvider.type == FormatType.Currency)
            //        numberText.limitDecimaDigits(2);
            //}
            fromatProvider.fromatNumberText(numberText);
            if (txtBox.Text != numberText.numberString)
            {
                txtBox.Text = numberText.numberString;
                parsedNumber = fromatProvider.parseNumber(numberText.numberString);
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
       */
    }
}
