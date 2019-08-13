using PatientsFomsRepository.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {

    /// <summary>
    /// Аттрибуты столбца файла пациентов
    /// </summary>
    [Serializable]
    public class ColumnProperty : BindableBase, IDataErrorInfo
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

        //IDataErrorInfo
        [XmlIgnore] public string Error { get; private set; }
        [XmlIgnore]
        public string this[string columnName]
        {
            get
            {
                string error = "";

                if (columnName == nameof(Name))
                    if (string.IsNullOrEmpty(Name))
                        error = "Название столбца не может быть пустым";

                if (columnName == nameof(AltName))
                    if (string.IsNullOrEmpty(AltName))
                        error = "Понятное название столбца не может быть пустым";

                Error = error;
                return error;
            }
        }
        #endregion
    }
    }
