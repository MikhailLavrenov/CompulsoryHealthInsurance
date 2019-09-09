using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Views;
using Prism.Regions;

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

            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => new SRZSettingsView());
            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => new AboutApplicationView());
            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ImportPatientsView));
            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(PatientsFileSettingsView));
            //regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(PatientsFileView));
        }
        #endregion

        #region Методы
        private void ShowSRZSettingsViewExecute(object parameter)
        {
            regionManager.RequestNavigate(RegionNames.MainRegion, nameof(SRZSettingsView));
        }
        private void ShowAboutApplicationViewExecute(object parameter)
        {
            regionManager.RequestNavigate(RegionNames.MainRegion, nameof(AboutApplicationView));
        }
        #endregion
    }
}
