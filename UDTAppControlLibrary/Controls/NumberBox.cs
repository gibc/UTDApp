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
    public class NumberBox : NumberBoxBase
    {
        public static readonly DependencyProperty NumberValueProperty =
             DependencyProperty.Register("NumberValue", typeof(Int32?), typeof(NumberBox),
             new UIPropertyMetadata(new PropertyChangedCallback(NumberValuePropertyChange)),
             null);

        static void NumberValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            NumberBox numberBox = src as NumberBox;
            Int32? newNumber = (Int32?)args.NewValue;
            if (numberBox.txtBox == null) return;
            if (newNumber != numberBox.parsedNumber)
            {
                if (newNumber == null)
                {
                    if(numberBox.NumberDefault == null)
                    { 
                        numberBox.numberText.setPrompt(numberBox.fromatProvider.prompt);
                        numberBox.updateTextBox();
                        return;
                    }
                    else
                    {
                        numberBox.NumberValue = numberBox.NumberDefault;
                        Task.Run(() => numberBox.updateSource(NumberValueProperty));
                        return;
                    }
                }

                string numTxt = numberBox.fromatProvider.getNumberText(newNumber);

                numberBox.numberText.clear();
                numberBox.numberText.insertString(numTxt);
                numberBox.updateTextBox();
            }
        }

        public Int32? NumberValue
        {
            get { return (Int32?)GetValue(NumberValueProperty); }
            set { SetValue(NumberValueProperty, value); }
        }

        public static readonly DependencyProperty NumberDefaultProperty =
             DependencyProperty.Register("NumberDefault", typeof(Int32?), typeof(NumberBox),
             new UIPropertyMetadata(new PropertyChangedCallback(NumberDefaultPropertyChange)),
             null);

        static void NumberDefaultPropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            NumberBox numberBox = src as NumberBox;
            Int32? newDefault = (Int32?)args.NewValue;
            if (numberBox.txtBox == null) return;
            if (numberBox.NumberDefault != null && numberBox.NumberValue == null)
            {
                numberBox.NumberValue = numberBox.NumberDefault;
                Task.Run(() => numberBox.updateSource(NumberValueProperty));
            }
        }

        public Int32? NumberDefault
        {
            get { return (Int32?)GetValue(NumberDefaultProperty); }
            set { SetValue(NumberDefaultProperty, value); }
        }

        #region MinMax
        public static readonly DependencyProperty MaxValueProperty =
         DependencyProperty.Register("MaxValue", typeof(Int32), typeof(NumberBox),
         new UIPropertyMetadata(Int32.MaxValue,
             new PropertyChangedCallback(OnMaxValuePropertyChange),
             null));

        static void OnMaxValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            NumberBox numberBox = src as NumberBox;
            Int32 newValue = (Int32)args.NewValue;
            if (numberBox.fromatProvider != null)
            {
                numberBox.fromatProvider.numberMax = newValue;
            }
        }

        public Int32 MaxValue
        {
            get { return (Int32)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
         DependencyProperty.Register("MinValue", typeof(Int32), typeof(NumberBox),
         new UIPropertyMetadata(Int32.MinValue,
             new PropertyChangedCallback(OnMinValuePropertyChange),
             null));

        static void OnMinValuePropertyChange(DependencyObject src, DependencyPropertyChangedEventArgs args)
        {
            NumberBox numberBox = src as NumberBox;
            Int32 newValue = (Int32)args.NewValue;
            if (numberBox.fromatProvider != null)
            {
                numberBox.fromatProvider.numberMin = newValue;
            }
        }

        public Int32 MinValue
        {
            get { return (Int32)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        #endregion

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public NumberBox()
        {
            fromatProvider = new NumberFromatProvider(MaxValue, MinValue);
        }

        override protected void ApplyTemplateComplete()
        {
            Int32? defalutVal = NumberDefault;
            Int32? numberVal = NumberValue;
            if (numberVal == null && defalutVal != null)
            {
                NumberDefault = null;
                NumberDefault = defalutVal;
            }
            else if (numberVal != null && numberVal != parsedNumber)
            {
                NumberValue = null;
                NumberValue = numberVal;
            }
        }


        Int32? parsedNumber = null;
        override protected void setParsedNumber(dynamic value)
        {
            if (fromatProvider.isMax)
            {
                messageBox.Text = "Maximum value.";
                messagePopup.IsOpen = true;
            }
            else if (fromatProvider.isMin)
            {
                messageBox.Text = "Minimum value.";
                messagePopup.IsOpen = true;
            }
            else messagePopup.IsOpen = false;

            parsedNumber = value;
            if (parsedNumber != NumberValue)
                NumberValue = parsedNumber;
        }

   }
}
