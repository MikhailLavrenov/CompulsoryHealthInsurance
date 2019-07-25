using System;
using System.Windows.Input;

namespace PatientsFomsRepository.Infrastructure
    {
    /// <summary>
    /// Упрощает создание команд
    /// </summary>
    public class RelayCommand : ICommand
        {
        #region Fields
        private Action<object> execute;
        private Func<object, bool> canExecute;
        #endregion

        #region Properties
        public event EventHandler CanExecuteChanged
            {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
            }
        #endregion

        #region Creators
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
        #endregion

        #region Methods
        public bool CanExecute(object parameter)
            {
            return canExecute == null ? true : canExecute(parameter);
            }
        public void Execute(object parameter)
            {
            execute(parameter);
            }
        #endregion
        }
    }
