using CHI.Application.Infrastructure;
using CHI.Application.Models;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Linq;
using System.Text;

namespace CHI.Application.ViewModels
{
    class AttachedPatientsSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private IRegionManager regionManager;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public AttachedPatientsSettingsViewModel(IMainRegionService mainRegionService, IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            MainRegionService = mainRegionService;
            MainRegionService.Header = "Настройки загрузки прикрепленных пациентов";
            ShowViewCommand = new DelegateCommand<Type>(ShowViewExecute);
        }
        #endregion

        #region Методы     
        private void ShowViewExecute(Type view)
        {
            regionManager.RequestNavigate(RegionNames.AttachedPatientsSettingsRegion, view.Name);
            MainRegionService.SetCompleteStatus(string.Empty);
        }
        #endregion
    }
}
;