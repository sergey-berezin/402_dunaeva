using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ViewModel
{
    class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;
        private readonly IUIServices svc;

        public RelayCommand(IUIServices svc, Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.svc = svc;
        }

        public event EventHandler CanExecuteChanged
        {
            add { svc.RequerySuggested += value; }
            remove { svc.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => canExecute == null ? true : canExecute(parameter);

        public void Execute(object parameter) => execute?.Invoke(parameter);
    }
}
