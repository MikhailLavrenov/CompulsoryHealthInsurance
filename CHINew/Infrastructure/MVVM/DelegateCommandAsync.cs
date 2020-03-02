using Prism.Commands;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Реализация <see cref="ICommand"/> без параметров,  <see cref="Execute()"/> выполняется асинхронно."/>.
    /// </summary>
    /// <see cref="DelegateCommandBase"/>
    /// <see cref="DelegateCommand{T}"/>
    public class DelegateCommandAsync : DelegateCommandBase
    {
        #region Поля              
        private bool isExecuting;
        private readonly Action executeMethod;
        private Func<bool> canExecuteMethod;

        private static string delegatesCannotBeNullErrorMessage = "executeMethod и canExecuteMethod не могут быть null.";
        #endregion

        #region Свойства
        private bool IsExecuting
        {
            get => isExecuting;
            set
            {
                isExecuting = value;
                RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Creates a new instance of <see cref="DelegateCommand"/> with the <see cref="Action"/> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute(object)"/> is called.</param>
        public DelegateCommandAsync(Action executeMethod)
            : this(executeMethod, () => true)
        {
        }
        /// <summary>
        /// Creates a new instance of <see cref="DelegateCommand"/> with the <see cref="Action"/> to invoke on execution
        /// and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{TResult}"/> to invoke when <see cref="ICommand.CanExecute"/> is called</param>
        public DelegateCommandAsync(Action executeMethod, Func<bool> canExecuteMethod)
            : base()
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(delegatesCannotBeNullErrorMessage);

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }
        #endregion

        #region Методы
        ///<summary>
        /// Выполняет команду асинхронно.
        ///</summary>
        public async void Execute()
        {
            IsExecuting = true;
            await Task.Run(() => executeMethod());
            IsExecuting = false;

        }
        /// <summary>
        /// Определяет может ли команда быть выполнена.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command can execute,otherwise returns <see langword="false"/>.</returns>
        public bool CanExecute()
        {
            if (IsExecuting)
                return false;

            return canExecuteMethod();
        }
        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parameter</param>
        protected override void Execute(object parameter)
        {
            Execute();
        }
        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns><see langword="true"/> if the Command Can Execute, otherwise <see langword="false" /></returns>
        protected override bool CanExecute(object parameter)
        {
            return CanExecute();
        }
        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression. Example: ObservesProperty(() => PropertyName).</param>
        /// <returns>The current instance of DelegateCommand</returns>
        public DelegateCommandAsync ObservesProperty<T>(Expression<Func<T>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }
        /// <summary>
        /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute(() => PropertyName).</param>
        /// <returns>The current instance of DelegateCommand</returns>
        public DelegateCommandAsync ObservesCanExecute(Expression<Func<bool>> canExecuteExpression)
        {
            canExecuteMethod = canExecuteExpression.Compile();
            ObservesPropertyInternal(canExecuteExpression);
            return this;
        }
        #endregion
    }
}
