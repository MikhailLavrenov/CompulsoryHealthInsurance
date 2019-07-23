using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PatientsFomsRepository.Models
    {
    public abstract class  BaseModel : INotifyPropertyChanged
        {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string prop = "")
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }

        protected void SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = "")
            {
            if (EqualityComparer<T>.Default.Equals(field, value) == false)
                {
                field = value;
                OnPropertyChanged(propertyName));
                }
            }
        }
    }
