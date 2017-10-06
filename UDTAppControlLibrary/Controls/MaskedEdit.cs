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

        MaskedTextProvider maskedTextProvider = null;

        virtual protected void previewKeyDownEvent(object src, KeyEventArgs arg)
        {
            if (arg.Key == Key.Space)
            {
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


}
