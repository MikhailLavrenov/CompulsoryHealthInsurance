using System;
using System.Windows.Input;

namespace PatientsFomsRepository.Infrastructure
    {
    class RelayCommand : ICommand
        {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
            {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
            }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
            {
            this.execute = execute;
            this.canExecute = canExecute;
            }

        public RelayCommand(Action<object> execute)
            {
            this.execute = execute;
            canExecute = null;
            }

        public bool CanExecute(object parameter)
            {
            return canExecute == null ? true : canExecute(parameter);
            }

        public void Execute(object parameter)
            {
            execute(parameter);
            }
        }
    }
