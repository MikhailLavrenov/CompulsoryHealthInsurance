using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.ViewModels;
using PatientsFomsRepository.Views;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PatientsFomsRepository
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var shellView = Container.Resolve<ShellView>();
            var regionManager = Container.Resolve<IRegionManager>();

            RegionManager.SetRegionManager(shellView, regionManager);
            RegionManager.UpdateRegions();
            
            var region = regionManager.Regions[RegionNames.MainRegion];
            var defaultView = Container.Resolve<PatientsFileView>();


            region.Add(defaultView);
            region.Add(Container.Resolve<ImportPatientsView>());
            region.Add(Container.Resolve<SRZSettingsView>());
            region.Add(Container.Resolve<PatientsFileSettingsView>());
            region.Add(Container.Resolve<AboutApplicationView>());


            region.Activate(defaultView);

            return shellView;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterForNavigation<PatientsFileView>();            
            //containerRegistry.RegisterForNavigation<ImportPatientsView>();
            //containerRegistry.RegisterForNavigation<SRZSettingsView>();
            //containerRegistry.RegisterForNavigation<PatientsFileSettingsView>();            
            //containerRegistry.RegisterForNavigation<AboutApplicationView>();

        }

    }
}
