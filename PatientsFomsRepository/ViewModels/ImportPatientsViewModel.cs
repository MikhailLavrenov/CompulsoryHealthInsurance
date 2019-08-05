using PatientsFomsRepository.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.ViewModels
{
    class ImportPatientsViewModel : BindableBase, IViewModel
    {
        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public RelayCommand ImportCommand{get; }
        public RelayCommand GetExampleCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
            ShortCaption = "Пополнить БД";
            FullCaption = "Пополнить БД из файла известными ФИО";
            ImportCommand = new RelayCommand(x =>  throw new NotImplementedException("еще не реализовано"));
            GetExampleCommand = new RelayCommand(x => throw new NotImplementedException("еще не реализовано"));

        }
        #endregion

        #region Методы
        #endregion
    }
}
