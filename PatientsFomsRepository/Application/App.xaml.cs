using CHI.Application.Infrastructure;
using CHI.Application.ViewModels;
using CHI.Application.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System.Windows;

namespace CHI.Application
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
            containerRegistry.RegisterSingleton<IMainRegionService, MainRegionService>();
            containerRegistry.Register<IFileDialogService, FileDialogService>();
            containerRegistry.RegisterDialog<NotificationDialogView, NotificationDialogViewModel>();

            containerRegistry.RegisterForNavigation<PatientsFileView>();
            containerRegistry.RegisterForNavigation<ExaminationsView>();
            containerRegistry.RegisterForNavigation<ServicesSettingsView>();
            containerRegistry.RegisterForNavigation<PatientsFileSettingsView>();
            containerRegistry.RegisterForNavigation<ExaminationsSettingView>();
            containerRegistry.RegisterForNavigation<AboutApplicationView>();
            containerRegistry.RegisterForNavigation<ProgressBarView>();
        }
    }
}
