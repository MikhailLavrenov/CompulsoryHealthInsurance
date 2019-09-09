using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.ViewModels;
using PatientsFomsRepository.Views;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Controls;

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

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<SRZSettingsView>();
            containerRegistry.RegisterForNavigation<AboutApplicationView>();
        }
    }
}
