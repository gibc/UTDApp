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
    public class DecimalBox : NumberBoxBase
    {
        public static readonly DependencyProperty DecimalValueProperty =
             DependencyProperty.Register("DecimalValue", typeof(Decimal?), typeof(DecimalBox),
             new UIPropertyMetadata((Decimal?)null, new PropertyChangedCallback(OnDecimalValuePropertyChange)),
             null);

        static void OnDecimalValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DecimalBox decimalBox = src as DecimalBox;
            Decimal? newValue = (Decimal?)args.NewValue;
            if (decimalBox.txtBox == null) return;
            if (newValue != decimalBox.parsedNumber)
            {
                if (newValue == null )
                {
                    decimalBox.numberText.setPrompt(decimalBox.fromatProvider.prompt);
                    decimalBox.updateTextBox();
                    return;
                }

                string numTxt = "";
                if (decimalBox.TextFormat == FormatType.Decimal)
                {
                    numTxt = string.Format("{0}", newValue);
                }
                else if (decimalBox.TextFormat == FormatType.Currency)
                { 
                    numTxt = string.Format("{0:n2}", newValue);
                }
                else if (decimalBox.TextFormat == FormatType.Percent)
                {
                    numTxt = string.Format("{0:n2}", 100*newValue);
                }
                decimalBox.numberText.clear();
                decimalBox.numberText.insertString(numTxt);
                decimalBox.updateTextBox();
            }
        }

        public Decimal? DecimalValue
        {
            get { return (Decimal?)GetValue(DecimalValueProperty); }
            set { SetValue(DecimalValueProperty, value); }
        }

        public static readonly DependencyProperty TextFormatProperty =
             DependencyProperty.Register("TextFormat", typeof(FormatType), typeof(DecimalBox),
             new UIPropertyMetadata(new PropertyChangedCallback(OnMaskTextFormatPropertyChange)),
             null);

        static void OnMaskTextFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DecimalBox decimalBox = src as DecimalBox;
            FormatType newType = (FormatType)args.NewValue;
            if (newType == FormatType.Decimal)
                decimalBox.fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            else if (newType == FormatType.Currency)
                decimalBox.fromatProvider = new CurrencyFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            else if (newType == FormatType.Percent)
                decimalBox.fromatProvider = new PercentFromatProvider(Decimal.MaxValue, Decimal.MinValue);
        }

        public FormatType TextFormat
        {
            get { return (FormatType)GetValue(TextFormatProperty); }
            set { SetValue(TextFormatProperty, value); }
        }

        public static readonly DependencyProperty DefaultValueProperty =
             DependencyProperty.Register("DefaultValue", typeof(Decimal?), typeof(DecimalBox),
             new UIPropertyMetadata(new PropertyChangedCallback(OnDefaultValuePropertyChange)),
             null);

        static void OnDefaultValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DecimalBox decimalBox = src as DecimalBox;
            Decimal? newValue = (Decimal?)args.NewValue;
            if (decimalBox.txtBox == null) return;
            if (decimalBox.DecimalValue == null && newValue != null && newValue != decimalBox.parsedNumber)
            {
                decimalBox.DecimalValue = newValue;
            }
       }

        public Decimal? DefaultValue
        {
            get { return (Decimal?)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        static DecimalBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalBox), new FrameworkPropertyMetadata(typeof(DecimalBox)));
        }

        public DecimalBox()
        {
            //DecimalValue = 0;
        }

        override protected void ApplyTemplateComplete()
        {
            Decimal? defalutVal = DefaultValue;
            Decimal? decimalVal = DecimalValue;
            if (decimalVal == null && defalutVal != null && defalutVal != parsedNumber)
            {
                DecimalValue = defalutVal;
            }
            else if (decimalVal != null && decimalVal != parsedNumber) 
            { 
                DecimalValue = null;
                DecimalValue = decimalVal;
            }
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
            if (parsedNumber != DecimalValue)
                DecimalValue = parsedNumber;
        }


    }
}
