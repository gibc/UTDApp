using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UDTAppControlLibrary.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:UDTAppControlLibrary.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:UDTAppControlLibrary.Controls;assembly=UDTAppControlLibrary.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:MaskedEdit/>
    ///
    /// </summary>
    public class MaskedTextBox : TextBox
    {
        public static readonly DependencyProperty MaskedTextProperty =
            DependencyProperty.Register("MaskedText", typeof(string), typeof(MaskedTextBox),
            new UIPropertyMetadata(new PropertyChangedCallback(OnMaskTextPropertyChange)),
            new ValidateValueCallback(maskTextValidateCallback));

        static private bool maskTextValidateCallback(object arg)
        {
            return true;
        }

        static void OnMaskTextPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            MaskedTextBox maskedEdit = src as MaskedTextBox;
            if (maskedEdit.maskedTextProvider != null && !maskedEdit.maskedTextProvider.MaskCompleted)
            {
                maskedEdit.maskedTextProvider.Replace(maskedEdit.MaskedText, 0);
                maskedEdit.Text = maskedEdit.maskedTextProvider.ToDisplayString();
                maskedEdit.CaretIndex = 0;
            }
            else if (maskedEdit.maskedTextProvider == null)
                maskedEdit.Text = maskedEdit.MaskedText; 
        }

        public string MaskedText
        {
            get { return (string)GetValue(MaskedTextProperty); }
            set { SetValue(MaskedTextProperty, value); }
        }

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask", typeof(string), typeof(MaskedTextBox),
            new UIPropertyMetadata(new PropertyChangedCallback(OnMaskPropertyChange)),
            new ValidateValueCallback(maskValidateCallback));

        static private bool maskValidateCallback(object arg)
        {
            return true;
        }

        static void OnMaskPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            MaskedTextBox maskedEdit = src as MaskedTextBox;
            maskedEdit.maskedTextProvider = new MaskedTextProvider((string)args.NewValue);
            maskedEdit.maskedTextProvider.ResetOnSpace = false;
            if(!string.IsNullOrEmpty(maskedEdit.MaskedText))
            {
                maskedEdit.maskedTextProvider.Replace(maskedEdit.MaskedText, 0);
                maskedEdit.Text = maskedEdit.maskedTextProvider.ToDisplayString();
                maskedEdit.CaretIndex = 0;
            }
            else
            {
                maskedEdit.Text = maskedEdit.maskedTextProvider.ToDisplayString();
                maskedEdit.CaretIndex = 0;
            }
        }

        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value);}
        }


        static MaskedTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaskedTextBox), new FrameworkPropertyMetadata(typeof(MaskedTextBox)));
        }

        public MaskedTextBox()
        {
            PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            TextChanged += new TextChangedEventHandler(textChangedEvent);
            PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
        }

        protected MaskedTextProvider maskedTextProvider = null;

        virtual protected void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            if (arg.Key == Key.Space)
            {
                //if (CaretIndex > 0)
                {
                    if (maskedTextProvider.Replace(' ', CaretIndex))
                    {
                        int tmpCaretIndex = CaretIndex;
                        Text = maskedTextProvider.ToDisplayString();
                        CaretIndex = tmpCaretIndex + 1;
                    }
                } 
                arg.Handled = true;
            }
            if (arg.Key == Key.Back)
            {
                if(CaretIndex > 0)
                {
                    if (maskedTextProvider.Replace(maskedTextProvider.PromptChar, CaretIndex - 1))
                    {
                        int tmpCaretIndex = CaretIndex;
                        Text = maskedTextProvider.ToDisplayString();
                        CaretIndex = tmpCaretIndex - 1;
                    }
                }
                arg.Handled = true;
            }
            if (arg.Key == Key.Delete)
            {
                if(SelectionLength > 0)
                {
                    int tmpCaretIndex = CaretIndex;
                    for (int i = SelectionStart; i < SelectionStart+SelectionLength; i++)
                    {
                        maskedTextProvider.Replace(maskedTextProvider.PromptChar, i);
                    }
                    Text = maskedTextProvider.ToDisplayString();
                    CaretIndex = tmpCaretIndex;
                }
                arg.Handled = true;
            }
            base.OnPreviewKeyDown(arg);
        }

        virtual protected void textChangedEvent(object src, TextChangedEventArgs arg)
        {
            string curtxt = Text;
        }

        virtual protected void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            int caretPos = CaretIndex;
            MaskedTextResultHint hint;
            if (maskedTextProvider.VerifyChar(arg.Text[0], CaretIndex, out hint))
            {
                maskedTextProvider.Replace(arg.Text[0], CaretIndex);
                Text = maskedTextProvider.ToDisplayString();
            }

            arg.Handled = true;
            Text = maskedTextProvider.ToDisplayString();
            if ((int)hint < 0 && (int)hint > -54)
                CaretIndex = caretPos;
            else
                CaretIndex = caretPos + 1;

            if (maskedTextProvider.MaskCompleted)
                MaskedText = maskedTextProvider.ToDisplayString();
        }

    }

    public class MaskedNumberBox : TextBox
    {
        public static readonly DependencyProperty MaskedNumberProperty =
            DependencyProperty.Register("MaskedNumber", typeof(Int32?), typeof(MaskedNumberBox),
            new UIPropertyMetadata(new PropertyChangedCallback(OnMaskNumberPropertyChange)),
            new ValidateValueCallback(maskNumberValidateCallback));

        static private bool maskNumberValidateCallback(object arg)
        {
            return true;
        }

        static void OnMaskNumberPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            MaskedNumberBox maskedNumber = src as MaskedNumberBox;
            Int32? newVal = (Int32?)args.NewValue;
            if (newVal != null)
                maskedNumber.maskedNumberProvider.displayText =
                    maskedNumber.maskedNumberProvider.getNumberText(newVal);
            else
                maskedNumber.maskedNumberProvider.displayText = 
                    maskedNumber.maskedNumberProvider.prompt;

            int caretTmp = maskedNumber.CaretIndex;
            maskedNumber.Text = maskedNumber.maskedNumberProvider.displayText;
            maskedNumber.CaretIndex = caretTmp;
        }

        public Int32? MaskedNumber
        {
            get { return (Int32?)GetValue(MaskedNumberProperty); }
            set { SetValue(MaskedNumberProperty, value); }
        }

        static MaskedNumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaskedNumberBox), new FrameworkPropertyMetadata(typeof(MaskedNumberBox)));
        }


        public MaskedNumberBox()
        {
            TextChanged += new TextChangedEventHandler(textChanged);
            PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
            maskedNumberProvider = new MaskedNumberProvider();
            maskedNumberProvider.prompt = "Enter a Number.";
        }

        private MaskedNumberProvider maskedNumberProvider;

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            if(!maskedNumberProvider.acceptChar(CaretIndex, arg.Text[0], Text))
            { 
                arg.Handled = true;
            }
            else if(Text == maskedNumberProvider.prompt)
            { 
                arg.Handled = true;
                Text = arg.Text;
            }

        }

        private void textChanged(object src, TextChangedEventArgs arg)
        {
            arg.Handled = true;
            int caretTmp = CaretIndex;
            maskedNumberProvider.setDisplayText(Text);
            this.FontWeight = FontWeights.Normal;
            if (maskedNumberProvider.displayText == maskedNumberProvider.prompt)
            {
                this.FontWeight = FontWeights.UltraLight;
            }

            Text = maskedNumberProvider.displayText;
            CaretIndex = caretTmp;

            if (maskedNumberProvider.numberComplete)
                MaskedNumber = (Int32)maskedNumberProvider.parseNumber(maskedNumberProvider.displayText);
        }
    }

    public class MaskedDecimalProvider : MaskedNumberProvider
    {
        public MaskedDecimalProvider() { }

        public Decimal? parseDecimal(string txtNum)
        {
            Decimal? number = null;
            if (string.IsNullOrEmpty(txtNum))
                return number;

            Decimal num;
            txtNum = txtNum.Replace(",", "");

            if (Decimal.TryParse(txtNum, out num))
            {
                number = num;
            }
            else if (txtNum[0] == '-')
            {
                number = decimalMin;
            }
            else
            {
                number = decimalMax;
            }

            if (number > decimalMax)
                number = decimalMax;
            else if (number < decimalMin)
                number = decimalMin;

            return number;
        }

        public override bool acceptChar(int postion, char c, string context)
        {
            bool haveDecPt = context.Contains(".");
            if (postion == 0)
            {
                return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.' && !haveDecPt));
            }
            else
            {
                return (Char.IsDigit(c) || (c == '.' && !haveDecPt));
            }           
        }


        public string getDecimalText(Decimal? num)
        {
            string numTxt = "";
            if (num == null) return numTxt;

            numTxt = string.Format("{0}", num);
            if (displayText.Length > 0 && displayText.Last() == '.')
                return numTxt + '.';
            else return numTxt;

        }
    }

    public class MaskedNumberProvider
    {
        public MaskedNumberProvider()
        {

        }

        private bool _numberComplete = false;
        public bool numberComplete
        {
            get { return _numberComplete; }
            set { _numberComplete = value; }
        }

        private string _prompt = "";
        public string prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }

        public void setDisplayText(string ctrlText)
        {
            numberComplete = false;
            if (string.IsNullOrEmpty(ctrlText))
            {
                displayText = prompt;
            }
            else
            {
                displayText = ctrlText;
                numberComplete = canParse(displayText);
            }
        }

        private string _displayText = "";
        public string displayText
        {
            get { return _displayText; }
            set { _displayText = value; }
        }


        protected Int32? numberMin = Int32.MinValue;
        protected Int32? numberMax = Int32.MaxValue;
        protected Decimal? decimalMin = Decimal.MinValue;
        protected Decimal? decimalMax = Decimal.MaxValue;

        protected virtual bool canParse(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            if (val == prompt) return false;
            bool retVal = false;
            foreach (char c in val)
            {
                if (!(c == '0' || c == '.' || c == '-'))
                {
                    return true;
                }
            }
            return retVal;
        }



        public Int32? parseNumber(string txtNum)
        {
            Int32? number = null;
            if (string.IsNullOrEmpty(txtNum))
                return number;

            int num;
            txtNum = txtNum.Replace(",", "");
            if (Int32.TryParse(txtNum, out num))
                number = num;
            else if (txtNum[0] == '-')
            {
                number = numberMin;
            }
            else
            {
                number = numberMax;
            }

            if (number > numberMax)
                number = numberMax;
            else if (number < numberMin)
                number = numberMin;

            return number;
        }

        public virtual bool acceptChar(int postion, char c, string context)
        {
            if(postion == 0)
            {
                return (c != '0' && Char.IsDigit(c) || c == '+' || c == '-' );
            }
            else
            {
                return Char.IsDigit(c);
            }
        }

        public virtual string getNumberText(Int32? num)
        {
            if (num == null) return "";
            return string.Format("{0:n0}", num);
        }

    }

}
