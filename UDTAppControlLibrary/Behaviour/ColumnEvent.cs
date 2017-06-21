using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace UDTAppControlLibrary.Behaviour
{
    public class MyInteraction : DependencyObject
    {

        public static MyInteraction GetMyProperty(DependencyObject obj)
        {
            return (MyInteraction)obj.GetValue(MyPropertyProperty);
        }

        public static void SetMyProperty(DependencyObject obj, MyInteraction value)
        {
            obj.SetValue(MyPropertyProperty, value);
        }

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.RegisterAttached("MyProperty", typeof(MyInteraction), typeof(MyInteraction), new UIPropertyMetadata(null, new PropertyChangedCallback((d, a) =>
            {
                var datagrid = d as DataGrid;
                datagrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(datagrid_AutoGeneratingColumn);
            })));

        static void datagrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
        }
    }
}
