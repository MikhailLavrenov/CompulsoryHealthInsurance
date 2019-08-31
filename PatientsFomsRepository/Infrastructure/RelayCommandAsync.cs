using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Упрощает создание команд,  асинхронное выполнение метода Execute
    /// </summary>
    public class RelayCommandAsync : RelayCommand
    {
        #region Поля
        private bool isExecuting;
        #endregion

        #region Свойства
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

        #region Конструкторы
        public RelayCommandAsync(Action<object> execute, Func<object, bool> canExecute) : base(execute, canExecute)
        {
        }
        public RelayCommandAsync(Action<object> execute) : base(execute)
        {
        }
        #endregion

        #region Методы
        public override bool CanExecute(object parameter)
        {
            if (IsExecuting)
                return false;

            if (canExecute == null)
                return true;

            return canExecute(parameter);
        }
        public override async void Execute(object parameter)
        {
            IsExecuting = true;
            await Task.Run(() => execute(parameter));
            IsExecuting = false;
        }
        #endregion

    }
}
