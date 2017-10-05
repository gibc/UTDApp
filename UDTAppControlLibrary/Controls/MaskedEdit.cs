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
    public class MaskedEdit : TextBox
    {
        public static readonly DependencyProperty MaskedTextProperty =
    DependencyProperty.Register("MaskedText", typeof(string), typeof(MaskedEdit), new UIPropertyMetadata(null));

        public string MaskedText
        {
            get { return (string)GetValue(MaskedTextProperty); }
            set { SetValue(MaskedTextProperty, value); }
        }

        static MaskedEdit()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaskedEdit), new FrameworkPropertyMetadata(typeof(MaskedEdit)));
        }

        public MaskedEdit()
        {
            PreviewKeyDown += new KeyEventHandler(previewKeyDownEvent);
            TextChanged += new TextChangedEventHandler(textChangedEvent);
            PreviewTextInput += new TextCompositionEventHandler(previewTextInput);
        }

        //private string _MaskedText = null;
        //public string MaskedText 
        //{
        //    get { return _MaskedText; }
        //    set { _MaskedText = value; }
        //}

        public string _mask = "";
        public string Mask 
        {
            get { return _mask; }
            set 
            { 
                _mask = value;
                maskedTextProvider = new MaskedTextProvider(value);
                Text = maskedTextProvider.ToDisplayString();
            }
        }

        MaskedTextProvider maskedTextProvider = null;

        private void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            if (arg.Key == Key.Space)
            {
                arg.Handled = true;
            }
            if (arg.Key == Key.Back)
            {
                if(CaretIndex > 0) CaretIndex--;
                arg.Handled = true;
            }
            if (arg.Key == Key.Delete)
            {
                arg.Handled = true;
            }
            base.OnPreviewKeyDown(arg);
        }
        private void textChangedEvent(object src, TextChangedEventArgs arg)
        {
            string curtxt = Text;
        }
        private void previewTextInput(object src, TextCompositionEventArgs arg)
        {
            int caretPos = CaretIndex;
            MaskedTextResultHint hint;
            if (maskedTextProvider.VerifyChar(arg.Text[0], CaretIndex, out hint))
            {
                maskedTextProvider.Replace(arg.Text[0], CaretIndex);
                Text = maskedTextProvider.ToDisplayString();
                //CaretIndex = caretPos + 1; 
                //arg.Handled = true;
            }
            else if (hint.ToString() != "NonEditPosition")
            {
                //CaretIndex = caretPos + 1;
                //arg.Handled = true;
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


}
