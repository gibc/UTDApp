using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UDTAppControlLibrary.Controls
{
    public enum DecimalFormatType { Percent = 1, Currency, Decimal };

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
                if(newValue == null)
                { 
                    if (decimalBox.DefaultValue == null)
                    {
                        decimalBox.numberText.setPrompt(decimalBox.fromatProvider.prompt);
                        decimalBox.updateTextBox();
                        return;
                    }
                    else
                    {
                        decimalBox.DecimalValue = decimalBox.DefaultValue;
                        Task.Run(() => decimalBox.updateSource(DecimalValueProperty));
                        return;
                    }
                }

                //string numTxt = "";
                //if (decimalBox.TextFormat == DecimalFormatType.Decimal)
                //{
                //    numTxt = string.Format("{0}", newValue);
                //}
                //else if (decimalBox.TextFormat == DecimalFormatType.Currency)
                //{ 
                //    numTxt = string.Format("{0:n2}", newValue);
                //}
                //else if (decimalBox.TextFormat == DecimalFormatType.Percent)
                //{
                //    numTxt = string.Format("{0:n2}", 100*newValue);
                //}
                string numTxt = decimalBox.fromatProvider.getNumberText(newValue);
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
             DependencyProperty.Register("TextFormat", typeof(DecimalFormatType), typeof(DecimalBox),
             new UIPropertyMetadata(new PropertyChangedCallback(OnMaskTextFormatPropertyChange)),
             null);

        static void OnMaskTextFormatPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            DecimalBox decimalBox = src as DecimalBox;
            DecimalFormatType newType = (DecimalFormatType)args.NewValue;
            if (newType == DecimalFormatType.Decimal)
                decimalBox.fromatProvider = new DcimalFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            else if (newType == DecimalFormatType.Currency)
                decimalBox.fromatProvider = new CurrencyFromatProvider(Decimal.MaxValue, Decimal.MinValue);
            else if (newType == DecimalFormatType.Percent)
                decimalBox.fromatProvider = new PercentFromatProvider(Decimal.MaxValue, Decimal.MinValue);
        }

        public DecimalFormatType TextFormat
        {
            get { return (DecimalFormatType)GetValue(TextFormatProperty); }
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
            if (decimalBox.DefaultValue != null && decimalBox.DecimalValue == null)
            {
                decimalBox.DecimalValue = decimalBox.DefaultValue;
                Task.Run(() => decimalBox.updateSource(DecimalValueProperty));
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
            if (decimalVal == null && defalutVal != null /*&& defalutVal != parsedNumber*/)
            {
                //DecimalValue = defalutVal;
                DefaultValue = null;
                DefaultValue = defalutVal;
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


            if (fromatProvider.isMax)
            {
                messageBox.Text = "Maximum allowed value.";
                //messageBox.Text = "value.";
                messagePopup.IsOpen = true;
            }
            else if (fromatProvider.isMin)
            {
                messageBox.Text = "Minimum allowed value.";
                messagePopup.IsOpen = true;
            }
            else messagePopup.IsOpen = false;

            parsedNumber = value;
            if (parsedNumber != DecimalValue)
                DecimalValue = parsedNumber;
        }

    }
}
