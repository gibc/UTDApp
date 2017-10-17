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
    // MaskedTextBox : bindTextBox : TextBox : BindableBase
    //
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
            //if (newVal != null)
                maskedNumber.maskedNumberProvider.displayText =
                    maskedNumber.maskedNumberProvider.fromatProvider.formatNumber(newVal, maskedNumber.Text);
            //else
            //    maskedNumber.maskedNumberProvider.displayText = 
            //        maskedNumber.maskedNumberProvider.prompt;

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
            PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            TextChanged += new TextChangedEventHandler(textChanged);
            PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
            maskedNumberProvider = new MaskedNumberProvider<Int32?>(FormatType.Interger, Int32.MaxValue, Int32.MinValue);
        }

        private MaskedNumberProvider<Int32?> maskedNumberProvider;

        private void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            maskedNumberProvider.fromatProvider.previewKeyDown(this, arg);
            base.OnPreviewKeyDown(arg);
        }

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            maskedNumberProvider.previewText(this, arg);
        }

        private void textChanged(object src, TextChangedEventArgs arg)
        {
            maskedNumberProvider.textChanged(this, arg);
            if (maskedNumberProvider.numberComplete)
                MaskedNumber = maskedNumberProvider.fromatProvider.parseNumber(maskedNumberProvider.displayText);
        }
    }

    public class MaskedDecimalBox : TextBox, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public static readonly DependencyProperty TextFormatProperty =
            DependencyProperty.Register("TextFormat", typeof(FormatType), typeof(MaskedDecimalBox),
            new UIPropertyMetadata(new PropertyChangedCallback(OnMaskTextFormatPropertyChange)),
            null);

        static void OnMaskTextFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            MaskedDecimalBox maskedDecimal = src as MaskedDecimalBox;
            FormatType newType = (FormatType)args.NewValue;
            maskedDecimal.maskedDecimalProvider = new MaskedNumberProvider<Decimal?>(newType, Decimal.MaxValue, Decimal.MinValue);
        }

        public FormatType TextFormat
        {
            get { return (FormatType)GetValue(TextFormatProperty); }
            set { SetValue(TextFormatProperty, value); }
        }

        public static readonly DependencyProperty MaskedDecimalProperty =
            DependencyProperty.Register("MaskedDecimal", typeof(Decimal?), typeof(MaskedDecimalBox),
            new UIPropertyMetadata(new PropertyChangedCallback(OnMaskDecimalPropertyChange)),
            new ValidateValueCallback(maskDecimalValidateCallback));

        static private bool maskDecimalValidateCallback(object arg)
        {
            return true;
        }

        static void OnMaskDecimalPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            MaskedDecimalBox maskedDecimal = src as MaskedDecimalBox;
            Decimal? newVal = (Decimal?)args.NewValue;
            Decimal? oldVal = (Decimal?)args.OldValue;
            if (maskedDecimal.maskedDecimalProvider == null)
            {
                maskedDecimal.maskedDecimalProvider =
                    new MaskedNumberProvider<Decimal?>(maskedDecimal.TextFormat, Decimal.MaxValue, Decimal.MinValue);
            }
            if (maskedDecimal.maskedDecimalProvider != null && newVal != oldVal)
            { 
                maskedDecimal.maskedDecimalProvider.displayText =
                    maskedDecimal.maskedDecimalProvider.fromatProvider.formatNumber(newVal, maskedDecimal.Text);

                //int caretTmp = maskedDecimal.CaretIndex;
                //if (maskedDecimal.Text.Length != maskedDecimal.maskedDecimalProvider.displayText.Length)
                //{
                //    caretTmp +=
                //        maskedDecimal.maskedDecimalProvider.displayText.Length - maskedDecimal.Text.Length;
                //}
                int caretTmp = maskedDecimal.maskedDecimalProvider.fromatProvider.getCaretPos
                           (maskedDecimal.CaretIndex, 
                           maskedDecimal.Text.Length,
                           maskedDecimal.maskedDecimalProvider.displayText.Length);

                maskedDecimal.Text = maskedDecimal.maskedDecimalProvider.displayText;
                maskedDecimal.CaretIndex = caretTmp;
            }
        }

        public Decimal? MaskedDecimal
        {
            get { return (Decimal?)GetValue(MaskedDecimalProperty); }
            set { SetValue(MaskedDecimalProperty, value); }
        }

        static MaskedDecimalBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaskedDecimalBox), new FrameworkPropertyMetadata(typeof(MaskedDecimalBox)));
        }


        public MaskedDecimalBox()
        {
            PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            TextChanged += new TextChangedEventHandler(textChanged);
            PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
            SelectionChanged += new RoutedEventHandler(selChange);
            headFormat = "@";
            //maskedDecimalProvider = new MaskedNumberProvider<Decimal?>(FormatType.Decimal, Decimal.MaxValue, Decimal.MinValue);
        }


        public string headFormat
        {
            get { return "$"; }
            set { NotifyPropertyChanged("headFormat"); }
        }

        private MaskedNumberProvider<Decimal?> maskedDecimalProvider = null;

        private bool InSelChange = false;
        private void selChange(object src, RoutedEventArgs arg)
        {
            if (!InSelChange)
            {
                InSelChange = true;
                TextBox textBox = src as TextBox;
                SelectionStart =
                    maskedDecimalProvider.fromatProvider.adjustCaret(SelectionStart, textBox.Text);
                InSelChange = false;
            }
        }

        private void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            maskedDecimalProvider.fromatProvider.previewKeyDown(this, arg);
            base.OnPreviewKeyDown(arg);
        }

        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            InTextChangeEvent = true;
            maskedDecimalProvider.previewText(this, arg);
            InTextChangeEvent = false;
        }

        private bool InTextChangeEvent = false;
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!InTextChangeEvent)
                base.OnTextChanged(e);
        }

        private void textChanged(object src, TextChangedEventArgs arg)
        {
            InTextChangeEvent = true;
            maskedDecimalProvider.textChanged(this, arg);
            if (maskedDecimalProvider.numberComplete)
                MaskedDecimal = maskedDecimalProvider.fromatProvider.parseNumber(maskedDecimalProvider.displayText);
            InTextChangeEvent = false;
        }
    }

}
