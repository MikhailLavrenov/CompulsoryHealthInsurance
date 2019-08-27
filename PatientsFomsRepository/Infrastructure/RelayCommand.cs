using System;
using System.Threading.Tasks;
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
        private bool isExecuting;
        #endregion

        #region Properties
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool IsExecuting
        {
            get => isExecuting;
            set
            {
                isExecuting = value;
                CommandManager.InvalidateRequerySuggested();
            }
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
            if (IsExecuting)
                return false;

            if (canExecute == null)
                return true;

            return canExecute(parameter);
        }
        public async void Execute(object parameter)
        {
            IsExecuting = true;
            await Task.Run(()=> execute(parameter));
            IsExecuting = false;
        }
        #endregion
    }
}
