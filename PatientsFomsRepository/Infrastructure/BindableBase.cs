using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PatientsFomsRepository.Infrastructure
    {
    /// <summary>
    /// Упрощает установку свойству значения с уведомлением об измении значении
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
        {
        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        protected void OnPropertyChanged([CallerMemberName]string prop = "")
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = "")
            {
            if (EqualityComparer<T>.Default.Equals(field, value) == false)
                {
                field = value;
                OnPropertyChanged(propertyName);
                }
            }
        #endregion
        }
    }
