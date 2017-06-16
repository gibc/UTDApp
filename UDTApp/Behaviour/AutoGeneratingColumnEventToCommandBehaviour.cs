using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UDTApp.Behaviour
{
    public static class AutoGeneratingColumnEventToCommandBehaviour
    {
        public static readonly DependencyProperty CommandProperty =
    DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(AutoGeneratingColumnEventToCommandBehaviour),
        new PropertyMetadata(
            null,
            CommandPropertyChanged));

        public static void SetCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject o)
        {
            return o.GetValue(CommandProperty) as ICommand;
        }

        private static void CommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;
            if (dataGrid != null)
            {
                if (e.OldValue != null)
                {
                    dataGrid.AutoGeneratingColumn -= OnAutoGeneratingColumn;
                }
                if (e.NewValue != null)
                {
                    dataGrid.AutoGeneratingColumn += OnAutoGeneratingColumn;
                }
            }
        }

        private static void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var dependencyObject = sender as DependencyObject;
            if (dependencyObject != null)
            {
                var command = dependencyObject.GetValue(CommandProperty) as ICommand;
                if (command != null && command.CanExecute(e))
                {
                    command.Execute(e);
                }
            }
        }
    }
}
