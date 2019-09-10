using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Views;
using Prism.Commands;
using Prism.Regions;
using System;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Поля
        IRegionManager regionManager;
        #endregion

        #region Свойства 
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel()
        {
        }

        public ShellViewModel(IRegionManager regionManager)
        {
            ShowViewCommand = new DelegateCommand<Type>(ShowViewExecute); 

            this.regionManager = regionManager;
        }
        #endregion

        #region Методы
        private void ShowViewExecute (Type parameter)
        {
            regionManager.RequestNavigate(RegionNames.MainRegion, parameter.Name);
        }
        #endregion
    }
}
