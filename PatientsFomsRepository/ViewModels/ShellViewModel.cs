using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : DomainObject
    {
        #region Поля
        private IRegionManager regionManager;
        #endregion

        #region Свойства 
        public IActiveViewModel ActiveViewModel { get; set; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel()
        {
        }
        public ShellViewModel(IRegionManager regionManager, IActiveViewModel activeViewMode)
        {            
            ActiveViewModel = activeViewMode;
            this.regionManager = regionManager;

            ShowViewCommand = new DelegateCommand<Type>(ShowViewExecute);
        }
        #endregion

        #region Методы
        private void ShowViewExecute(Type parameter)
        {
            ActiveViewModel.Status = "";
            regionManager.RequestNavigate(RegionNames.MainRegion, parameter.Name);            
        }
        #endregion
    }
}
