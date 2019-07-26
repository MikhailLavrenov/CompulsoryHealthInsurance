using PatientsFomsRepository.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Models
    {

    /// <summary>
    /// Аттрибуты столбца файла пациентов
    /// </summary>
    [Serializable]
    public class ColumnProperties : BindableBase
        {
        #region Fields
        private string name;
        private string altName;
        private bool hide;
        private bool delete;
        #endregion

        #region Properties
        public string Name { get => name; set => SetProperty(ref name, value); }
        public string AltName { get => altName; set => SetProperty(ref altName, value); }
        public bool Hide { get => hide; set => SetProperty(ref hide, value); }
        public bool Delete { get => delete; set => SetProperty(ref delete, value); }
        #endregion
        }
    }
