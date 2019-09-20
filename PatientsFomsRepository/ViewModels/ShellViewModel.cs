using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using System;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : DomainObject
    {
        #region Поля
        #endregion

        #region Свойства 
        public IMainRegionService MainRegionService { get; set; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel(IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;

            ShowViewCommand = new DelegateCommand<Type>(x => MainRegionService.RequestNavigate(x.Name));
        }
        #endregion

        #region Методы
        #endregion
    }
}
