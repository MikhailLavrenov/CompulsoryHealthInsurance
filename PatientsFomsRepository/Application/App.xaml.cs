using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.ViewModels;
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

            var viewModel = Current.MainWindow.DataContext as ShellViewModel;
            viewModel.ShowViewCommand.Execute(typeof(PatientsFileView));
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IActiveViewModel,ActiveViewModel>();
            containerRegistry.Register<IFileDialogService, FileDialogService>();

            containerRegistry.RegisterForNavigation<PatientsFileView>();
            containerRegistry.RegisterForNavigation<ImportPatientsView>();
            containerRegistry.RegisterForNavigation<SRZSettingsView>();
            containerRegistry.RegisterForNavigation<PatientsFileSettingsView>();
            containerRegistry.RegisterForNavigation<AboutApplicationView>();
        }
    }
}
