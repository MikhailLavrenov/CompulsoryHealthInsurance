using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Упрощает создание команд
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields
        protected Action<object> execute;
        protected Func<object, bool> canExecute;

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
        public virtual bool CanExecute(object parameter)
        {
            if (canExecute == null)
                return true;

            return canExecute(parameter);
        }
        public virtual void Execute(object parameter)
        {
            execute(parameter);            
        }
        #endregion
    }
}
