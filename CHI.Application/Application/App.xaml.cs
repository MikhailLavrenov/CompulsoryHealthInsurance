using CHI.Application.Infrastructure;
using CHI.Application.ViewModels;
using CHI.Application.Views;
using NLog;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Threading;

namespace CHI.Application
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private ILogger logger { get; set; }

        protected override Window CreateShell()
        {
            return Container.Resolve<ShellView>();
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            logger = Container.Resolve<ILogger>();

            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            DispatcherUnhandledException += LogDispatcherUnhandledException;

            var viewModel = Current.MainWindow.DataContext as ShellViewModel;
            viewModel.ShowViewCommand.Execute(typeof(PatientsFileView));
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<ILogger>(LogManager.GetCurrentClassLogger());
            containerRegistry.RegisterSingleton<IMainRegionService, MainRegionService>();
            containerRegistry.RegisterSingleton<ILicenseManager, LicenseManager>();
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
        private void LogUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            logger.Error((Exception)args.ExceptionObject, "AppDomainException");
        }
        private void LogDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            logger.Error(args.Exception, "XamlDispatcherException");
        }
    }
}
