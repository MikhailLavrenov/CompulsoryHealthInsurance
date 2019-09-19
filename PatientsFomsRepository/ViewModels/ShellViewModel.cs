using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using System;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : DomainObject
    {
        #region Поля
        private readonly IRegionManager regionManager;
        #endregion

        #region Свойства 
        public IMainRegionService MainRegionService { get; set; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel(IRegionManager regionManager, IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;
            this.regionManager = regionManager;

            ShowViewCommand = new DelegateCommand<Type>(ShowViewExecute);
        }
        #endregion

        #region Методы
        private void ShowViewExecute(Type parameter)
        {
            MainRegionService.Status = "";
            regionManager.RequestNavigate(RegionNames.MainRegion, parameter.Name);
        }
        #endregion
    }
}
