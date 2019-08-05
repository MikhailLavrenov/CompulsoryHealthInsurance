using PatientsFomsRepository.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.ViewModels
{
    class PatientsFileViewModel : BindableBase, IViewModel
    {
        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public RelayCommand ProcessFileCommand { get; }
        #endregion

        #region Конструкторы
        public PatientsFileViewModel()
        {
            ShortCaption = "Обработать файл пациентов";
            FullCaption = "Найти и подставить полные ФИО пациентов в файл";
            ProcessFileCommand = new RelayCommand(x => throw new NotImplementedException("еще не реализовано"));


        }
        #endregion

        #region Методы
        #endregion
    }
}
