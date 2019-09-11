using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace PatientsFomsRepository
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<ShellView>();
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate(RegionNames.MainRegion, nameof(PatientsFileView));
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PatientsFileView>();
            containerRegistry.RegisterForNavigation<ImportPatientsView>();
            containerRegistry.RegisterForNavigation<SRZSettingsView>();
            containerRegistry.RegisterForNavigation<PatientsFileSettingsView>();
            containerRegistry.RegisterForNavigation<AboutApplicationView>();
        }
    }
}
