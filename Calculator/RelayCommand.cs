using System;
using System.Windows.Input;

namespace SimpleCalculator
{
    internal class RelayCommand : ICommand
    {
        private readonly Action<Object> execute;
        private readonly Func<Object, bool> canExecute;
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<Object> Execute, Func<object, bool> CanExecute = null)
        {
            this.execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
            this.canExecute = CanExecute;
        }

        public bool CanExecute(object parameter) => canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => execute(parameter);

    }
}