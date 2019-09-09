using CommonServiceLocator;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Views;
using Prism.Regions;
using System.Linq;
using System.Windows;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Поля
        IRegionManager regionManager;
        #endregion

        #region Свойства 
        public RelayCommand ShowSRZSettingsViewCommand { get; }
        public RelayCommand ShowAboutApplicationViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel()
        {
        }

        public ShellViewModel(IRegionManager regionManager)
        {
            ShowSRZSettingsViewCommand = new RelayCommand(ShowSRZSettingsViewExecute);
            ShowAboutApplicationViewCommand = new RelayCommand(ShowAboutApplicationViewExecute);

            this.regionManager = regionManager;

            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(SRZSettingsView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AboutApplicationView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ImportPatientsView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(PatientsFileSettingsView));
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(PatientsFileView));
        }
        #endregion

        #region Методы
        private void ShowSRZSettingsViewExecute(object parameter)
        {

        }
        private void ShowAboutApplicationViewExecute(object parameter)
        {
            var region = regionManager.Regions[RegionNames.MainRegion];
            var view=regionManager.Regions[RegionNames.MainRegion].GetView(nameof(AboutApplicationView));
            regionManager.Regions[RegionNames.MainRegion].Activate(view);
        }
        #endregion
    }
}
