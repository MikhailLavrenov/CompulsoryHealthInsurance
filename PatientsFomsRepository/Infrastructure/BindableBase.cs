using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Упрощает присваивание свойству значения с уведомлением об измении и валидацией значения.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Поля
        //Хранить все ошибки экземпляра класса
        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        #endregion

        #region Свойства
        public bool HasErrors { get => errors.Count > 0; }
        protected string IsNullOrEmptyErrorMessage = "Значение не может быть пустым";
        protected string ConnectionErrorMessage = "Не удалось подключиться";
        protected string LessOneErrorMessage = "Значение не может быть меньше 1";
        protected string UriFormatErrorMessage = "Не верный формат URI";
        #endregion

        #region События
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        #endregion

        #region Методы

        //INotifyPropertyChanged       
        protected void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        //Установить значение свойства
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value) == false)
            {                
                field = value;
                OnPropertyChanged(propertyName);
                Validate(propertyName);
            }
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
        //Выполняет валидацию. Должен быть переопределен в производном классе для автоматического вызова при изменении свойств.
        public virtual void Validate(string propertyName)
        { }
        //Валидация cвойства типа string на null или пусто
        protected void ValidateIsNullOrEmptyString(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
                AddError(IsNullOrEmptyErrorMessage, propertyName);
            else
                RemoveError(IsNullOrEmptyErrorMessage, propertyName);
        }
        //Добавить сообщение об ошибке в значении свойства
        protected void AddError(string errormessage, [CallerMemberName]string propertyName = "")
        {
            if (errors.ContainsKey(propertyName) == false)
                errors[propertyName] = new List<string>();

            if (errors[propertyName].Contains(errormessage) == false)
            {
                errors[propertyName].Add(errormessage);
                OnErrorsChanged(propertyName);
            }
        }
        //Удалить сообщение об ошибке в значении свойства
        protected void RemoveError(string errormessage, [CallerMemberName]string propertyName = "")
        {
            if (errors.ContainsKey(propertyName) && errors[propertyName].Contains(errormessage))
            {
                errors[propertyName].Remove(errormessage);

                if (errors[propertyName].Count == 0)
                    errors.Remove(propertyName);

                OnErrorsChanged(propertyName);
            }
        }
        //Удалить все сообщения об ошибке  в значении свойства
        protected void RemoveErrors([CallerMemberName]string propertyName = "")
        {
            if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }               
        #endregion









    }
}
