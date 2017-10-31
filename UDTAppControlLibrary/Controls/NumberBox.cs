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
                    numberBox.numberText.setPrompt(numberBox.fromatProvider.prompt);
                    numberBox.updateTextBox();
                    return;
                }

                string numTxt = "";
                numTxt = string.Format("{0}", newNumber);

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
            }
        }

        public Int32? NumberDefault
        {
            get { return (Int32?)GetValue(NumberDefaultProperty); }
            set { SetValue(NumberDefaultProperty, value); }
        }



        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public NumberBox()
        {
            fromatProvider = new NumberFromatProvider(Int32.MaxValue, Int32.MinValue);
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
            parsedNumber = value;
            if (parsedNumber != NumberValue)
                NumberValue = parsedNumber;
        }

   }
}
