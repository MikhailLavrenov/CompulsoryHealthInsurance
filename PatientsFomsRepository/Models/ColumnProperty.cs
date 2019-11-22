using CHI.Services.AttachedPatients;
using PatientsFomsRepository.Infrastructure;
using System;

namespace PatientsFomsRepository.Models
{

    /// <summary>
    /// Аттрибуты столбца файла пациентов
    /// </summary>
    [Serializable]
    public class ColumnProperties : DomainObject, IColumnProperties
    {
        #region Поля
        private string name;
        private string altName;
        private bool hide;
        private bool delete;
        #endregion

        #region Констукторы
        public ColumnProperties()
        : this(null, null)
        {
        }
        public ColumnProperties(string name, string altName)
        {
            Name = name;
            AltName = altName;
            Hide = false;
            Delete = false;
        }
        #endregion

        #region Свойства
        public string Name { get => name; set => SetProperty(ref name, value); }
        public string AltName { get => altName; set => SetProperty(ref altName, value); }
        public bool Hide { get => hide; set => SetProperty(ref hide, value); }
        public bool Delete { get => delete; set => SetProperty(ref delete, value); }
        #endregion

        #region Методы
        // Валидация свойств
        public override void Validate(string propertyName = null)
        {
            if (propertyName == nameof(Name) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(Name), Name);

            if (propertyName == nameof(AltName) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(AltName), AltName);
        }
        #endregion
    }
}

