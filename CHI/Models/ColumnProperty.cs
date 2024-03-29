﻿using CHI.Infrastructure;
using CHI.Services;
using System;

namespace CHI.Models
{

    /// <summary>
    /// Аттрибуты столбца файла пациентов
    /// </summary>
    [Serializable]
    public class ColumnProperty : DomainObject, IColumnProperties
    {
        private string name;
        private string altName;
        private bool hide;
        private bool delete;


        public string Name { get => name; set => SetProperty(ref name, value); }
        public string AltName { get => altName; set => SetProperty(ref altName, value); }
        public bool Hide { get => hide; set => SetProperty(ref hide, value); }
        public bool Delete { get => delete; set => SetProperty(ref delete, value); }


        public ColumnProperty(string name, string altName)
        {
            Name = name;
            AltName = altName;
            Hide = false;
            Delete = false;
        }

        public ColumnProperty()
            : this(null, null)
        {
        }


        public override void Validate(string propertyName = null)
        {
            if (propertyName == nameof(Name) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(Name), Name);

            if (propertyName == nameof(AltName) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(AltName), AltName);
        }

        public bool NameOrAltNameIsEqual(string text)
            => Name.Equals(text, StringComparison.OrdinalIgnoreCase) || AltName.Equals(text, StringComparison.OrdinalIgnoreCase);
    }
}

