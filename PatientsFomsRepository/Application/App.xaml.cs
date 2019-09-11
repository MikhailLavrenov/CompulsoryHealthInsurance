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
            Window shellView= Container.Resolve<ShellView>();
            shellView.Loaded += Loaded;
            return shellView;
        }
        private void Loaded(object sender, RoutedEventArgs e)
        {
            var view=sender as Window;
            var viewModel = view.DataContext as ShellViewModel;
            viewModel.ShowViewCommand.Execute(typeof(PatientsFileView));
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
