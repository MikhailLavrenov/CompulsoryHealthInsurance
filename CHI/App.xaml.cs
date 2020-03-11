using CHI.Infrastructure;
using CHI.ViewModels;
using CHI.Views;
using NLog;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace CHI
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private ILogger logger { get; set; }

        protected override Window CreateShell()
        {
            var window = Container.Resolve<ShellView>();

            //устанавливает язык для DatePicker MaterialDesign
            window.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            //необходимо для работы с различными кодировками
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Encoding.GetEncoding("windows-1251");

            return window;
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            logger = Container.Resolve<ILogger>();

            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            DispatcherUnhandledException += LogDispatcherUnhandledException;
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<ILogger>(LogManager.GetCurrentClassLogger());
            containerRegistry.RegisterSingleton<IMainRegionService, MainRegionService>();
            containerRegistry.RegisterSingleton<ILicenseManager, LicenseManager>();
            containerRegistry.Register<IFileDialogService, FileDialogService>();
            containerRegistry.RegisterDialog<NotificationDialogView, NotificationDialogViewModel>();

            containerRegistry.RegisterForNavigation<NavigationMenuView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsSettingsView>();
            containerRegistry.RegisterForNavigation<SrzSettingsView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsStorageSettingsView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsFileSettingsView>();
            containerRegistry.RegisterForNavigation<ExaminationsView>();
            containerRegistry.RegisterForNavigation<ExaminationsSettingsView>();
            containerRegistry.RegisterForNavigation<SrzSettingsView>();
            containerRegistry.RegisterForNavigation<OtherSettingsView>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<ProgressBarView>();
            containerRegistry.RegisterForNavigation<DepartmentsView>();
            containerRegistry.RegisterForNavigation<MedicsView>();
            containerRegistry.RegisterForNavigation<SpecialtiesView>();
            containerRegistry.RegisterForNavigation<EmployeesView>();
            containerRegistry.RegisterForNavigation<RegistersView>();
            containerRegistry.RegisterForNavigation<ServiceClassifierView>();
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
