using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UDTAppControlLibrary.Behaviour
{
    public static class InputBehaviour
    {
        public static bool GetIsAlphaOnly(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAlphaOnlyProperty);
        }

        public static void SetIsAlphaOnly(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAlphaOnlyProperty, value);
        }

        public static readonly DependencyProperty IsAlphaOnlyProperty =
          DependencyProperty.RegisterAttached("IsAlphaOnly",
          typeof(bool), typeof(InputBehaviour),
          new UIPropertyMetadata(false, OnIsAlphaOnlyChanged));

        private static void OnIsAlphaOnlyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // ignoring error checking
            TextBox textBox = (TextBox)sender;
            bool isAlphaOnly = (bool)(e.NewValue);

            if (isAlphaOnly)
                textBox.PreviewTextInput += BlockNonAlphaCharacters;
            else
                textBox.PreviewTextInput -= BlockNonAlphaCharacters;
        }

        private static void BlockNonAlphaCharacters(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
                if (!Char.IsLetter(ch))
                    e.Handled = true;
        }
    }

}
