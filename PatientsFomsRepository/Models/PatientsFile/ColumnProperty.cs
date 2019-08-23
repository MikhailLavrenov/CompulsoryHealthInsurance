using PatientsFomsRepository.Infrastructure;
using System;

namespace PatientsFomsRepository.Models
{

    /// <summary>
    /// Аттрибуты столбца файла пациентов
    /// </summary>
    [Serializable]
    public class ColumnProperty : BindableBase
    {
        #region Поля
        private string name;
        private string altName;
        private bool hide;
        private bool delete;
        #endregion

        #region Свойства
        public string Name { get => name; set => SetProperty(ref name, value); }
        public string AltName { get => altName; set => SetProperty(ref altName, value); }
        public bool Hide { get => hide; set => SetProperty(ref hide, value); }
        public bool Delete { get => delete; set => SetProperty(ref delete, value); }
        #endregion

        #region Методы
        // Валидация свойств
        protected override void Validate(string propertyName)
        {
            var message1 = "Значение не может быть пустым";

            switch (propertyName)
            {
                case nameof(Name):
                    if (string.IsNullOrEmpty(Name))
                        AddError(message1, propertyName);
                    else
                        RemoveError(message1, propertyName);
                    break;

                case nameof(AltName):
                    if (string.IsNullOrEmpty(AltName))
                        AddError(message1, propertyName);
                    else
                        RemoveError(message1, propertyName);
                    break;
            }
        }
        #endregion
    }
}

