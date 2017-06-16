using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UDTApp.ViewModels
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;
        private readonly Delegate _canExecute;

        public DelegateCommand(Action action, Delegate canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return (bool)_canExecute.DynamicInvoke();
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged { add { } remove { } }
#pragma warning restore 67

    }
}
