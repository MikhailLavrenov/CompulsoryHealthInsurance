using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.ComponentModel;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Поля
        IRegionManager regionManager;
        IContainerExtension container;
        IRegion mainRegion;
        #endregion

        #region Свойства 
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel()
        {
        }
        public ShellViewModel(IRegionManager regionManager, IContainerExtension container)
        {
            ShowViewCommand = new DelegateCommand<Type>(ShowViewExecute);

            this.container = container;
            this.regionManager = regionManager;



            //mainRegion.Activate(view);
            //mainRegion.RequestNavigate(new Uri("PatientsFileView", UriKind.Relative));

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
