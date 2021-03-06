﻿using Prism.Commands;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Реализация <see cref="ICommand"/> параметризованная местом заполнения типа T, <see cref="Execute(T)"/> выполняется асинхронно."/>.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <remarks>
    /// The constructor deliberately prevents the use of value types.
    /// Because ICommand takes an object, having a value type for T would cause unexpected behavior when CanExecute(null) is called during XAML initialization for command bindings.
    /// Using default(T) was considered and rejected as a solution because the implementor would not be able to distinguish between a valid and defaulted values.
    /// <para/>
    /// Instead, callers should support a value type by using a nullable value type and checking the HasValue property before using the Value property.
    /// <example>
    ///     <code>
    /// public MyClass()
    /// {
    ///     this.submitCommand = new DelegateCommand&lt;int?&gt;(this.Submit, this.CanSubmit);
    /// }
    /// 
    /// private bool CanSubmit(int? customerId)
    /// {
    ///     return (customerId.HasValue &amp;&amp; customers.Contains(customerId.Value));
    /// }
    ///     </code>
    /// </example>
    /// </remarks>
    public class DelegateCommandAsync<T> : DelegateCommandBase
    {
        #region Поля
        private bool isExecuting;
        private readonly Action<T> executeMethod;
        private Func<T, bool> canExecuteMethod;

        private static string delegatesCannotBeNullErrorMessage = "executeMethod и canExecuteMethod не могут быть null.";
        private static string invalidGenericTypeErrorMessage = "Параметр места заполнения типа T не является ссылочным типом.";
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
        /// Initializes a new instance of <see cref="DelegateCommandAsync{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><see cref="CanExecute(T)"/> will always return true.</remarks>
        public DelegateCommandAsync(Action<T> executeMethod)
            : this(executeMethod, (o) => true)
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommandAsync{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <exception cref="ArgumentNullException">When both <paramref name="executeMethod"/> and <paramref name="canExecuteMethod"/> are <see langword="null" />.</exception>
        public DelegateCommandAsync(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base()
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(delegatesCannotBeNullErrorMessage);

            TypeInfo genericTypeInfo = typeof(T).GetTypeInfo();

            // DelegateCommand allows reference types.  
            // note: Nullable<> is a struct so we cannot use a class constraint.
            if (genericTypeInfo.IsValueType)
            {
                if ((!genericTypeInfo.IsGenericType) || (!typeof(Nullable<>).GetTypeInfo().IsAssignableFrom(genericTypeInfo.GetGenericTypeDefinition().GetTypeInfo())))
                    throw new InvalidCastException(invalidGenericTypeErrorMessage);
            }

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }
        #endregion

        #region Методы
        ///<summary>
        ///Выполняет команду асинхронно.
        ///</summary>
        ///<param name="parameter">Data used by the command.</param>
        public async void Execute(T parameter)
        {
            IsExecuting = true;
            await Task.Run(() => executeMethod(parameter));
            IsExecuting = false;

        }
        ///<summary>
        ///Определяет может ли команда быть выполнена.
        ///</summary>
        ///<param name="parameter">Data used by the command to determine if it can execute.</param>
        ///<returns>
        ///<see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        ///</returns>
        public bool CanExecute(T parameter)
        {
            if (IsExecuting)
                return false;

            return canExecuteMethod(parameter);
        }
        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parameter</param>
        protected override void Execute(object parameter)
        {
            Execute((T)parameter);
        }
        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns><see langword="true"/> if the Command Can Execute, otherwise <see langword="false" /></returns>
        protected override bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }
        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <typeparam name="TType">The type of the return value of the method that this delegate encapulates</typeparam>
        /// <param name="propertyExpression">The property expression. Example: ObservesProperty(() => PropertyName).</param>
        /// <returns>The current instance of DelegateCommand</returns>
        public DelegateCommandAsync<T> ObservesProperty<TType>(Expression<Func<TType>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }
        /// <summary>
        /// Observes a property that is used to determine if this command can execute, and if it implements INotifyPropertyChanged it will automatically call DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <param name="canExecuteExpression">The property expression. Example: ObservesCanExecute(() => PropertyName).</param>
        /// <returns>The current instance of DelegateCommand</returns>
        public DelegateCommandAsync<T> ObservesCanExecute(Expression<Func<bool>> canExecuteExpression)
        {
            Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(canExecuteExpression.Body, Expression.Parameter(typeof(T), "o"));
            canExecuteMethod = expression.Compile();
            ObservesPropertyInternal(canExecuteExpression);
            return this;
        }
        #endregion

    }

}
