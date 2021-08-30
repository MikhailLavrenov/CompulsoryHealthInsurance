using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Упрощает присваивание свойству значения с уведомлением об измении и валидацией значения.
    /// </summary>
    public abstract class DomainObject : BindableBase, INotifyDataErrorInfo
    {
        //Хранит все ошибки экземпляра класса
        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();


        public bool HasErrors { get => errors.Count > 0; }


        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;


        //BindableBase
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            Validate(args.PropertyName);
        }

        //INotifyDataErrorInfo
        public void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !errors.ContainsKey(propertyName))
                return null;
            return errors[propertyName];
        }

        public bool ContainsErrorMessage(string propertyName, string errorMessage)
        {
            if (errors.ContainsKey(propertyName) && errors[propertyName].Contains(errorMessage))
                return true;
            else
                return false;
        }

        //Выполняет валидацию. Должен быть переопределен в производном классе для автоматического вызова при изменении свойств.
        public virtual void Validate(string propertyName)
        { }

        //Валидация cвойства типа string на null или пусто
        protected void ValidateIsNullOrEmptyString(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
                AddError(ErrorMessages.IsNullOrEmpty, propertyName);
            else
                RemoveError(ErrorMessages.IsNullOrEmpty, propertyName);
        }

        //Добавить сообщение об ошибке в значении свойства
        public void AddError(string errorMessage, [CallerMemberName]string propertyName = "")
        {
            if (errors.ContainsKey(propertyName) == false)
                errors[propertyName] = new List<string>();

            if (errors[propertyName].Contains(errorMessage) == false)
            {
                errors[propertyName].Add(errorMessage);
                OnErrorsChanged(propertyName);
            }
        }

        //Удалить сообщение об ошибке в значении свойства
        public void RemoveError(string errorMessage, [CallerMemberName]string propertyName = "")
        {
            if (errors.ContainsKey(propertyName) && errors[propertyName].Contains(errorMessage))
            {
                errors[propertyName].Remove(errorMessage);

                if (errors[propertyName].Count == 0)
                    errors.Remove(propertyName);

                OnErrorsChanged(propertyName);
            }
        }

        //Удалить сообщение об ошибке во всех свойствах
        public void RemoveErrorsMessage(string errormessage)
        {
            var propertyNames = errors.Keys.ToList();

            for (int i = 0; i < propertyNames.Count; i++)
            {
                var propertyName = propertyNames[i];
                var propertyErrorMessages = errors[propertyName];

                if (propertyErrorMessages?.Contains(errormessage) == true)
                {
                    if (propertyErrorMessages.Count == 1)
                        errors.Remove(propertyName);
                    else
                        propertyErrorMessages.Remove(errormessage);

                    OnErrorsChanged(propertyName);
                }
            }
        }

        //Удалить все сообщения об ошибке  в значении свойства
        public void RemoveErrors([CallerMemberName]string propertyName = "")
        {
            if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
    }
}
