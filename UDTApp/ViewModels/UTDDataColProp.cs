using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class UTDDataColProp : DependencyObject
    {
        public static readonly DependencyProperty
            UDTDataColProperty =
            DependencyProperty.RegisterAttached(
                  "UDTDataCol", typeof(ObservableCollection<UDTData>), typeof(UTDDataColProp),
            new PropertyMetadata(default(ObservableCollection<UDTData>)));

        public static ObservableCollection<UDTData> GetDataCol(
            DependencyObject d)
        {
            return (ObservableCollection<UDTData>)d.GetValue(UDTDataColProperty);
        }
        public static void SetDataCol(
            DependencyObject d, ObservableCollection<UDTData> value)
        {
            d.SetValue(UDTDataColProperty, value);
        }
    }

    public class Ex : DependencyObject
    {
        public static readonly DependencyProperty
            SecurityIdProperty =
            DependencyProperty.RegisterAttached(
                  "SecurityId", typeof(ObservableCollection<UDTBase>), typeof(Ex),
            new PropertyMetadata(default(ObservableCollection<UDTBase>)));

        public static ObservableCollection<UDTBase> GetSecurityId(
            DependencyObject d)
        {
            return (ObservableCollection<UDTBase>)d.GetValue(SecurityIdProperty);
        }
        public static void SetSecurityId(
            DependencyObject d, ObservableCollection<UDTBase> value)
        {
            d.SetValue(SecurityIdProperty, value);
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
