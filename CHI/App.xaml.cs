using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Settings;
using CHI.Views;
using Microsoft.EntityFrameworkCore;
using NLog;
using OfficeOpenXml;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
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
        ILogger logger;
        IMainRegionService mainRegionService;
        string appCrashMessage = "Произошла ошибка, диагностические данные записаны в лог, приложение будет закрыто.\r\n Если ошибка повторится, обратитесь к системному администратору.";


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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            logger = Container.Resolve<ILogger>();
            mainRegionService = Container.Resolve<IMainRegionService>();

            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            DispatcherUnhandledException += LogDispatcherUnhandledException;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(containerRegistry.GetContainer());
            containerRegistry.RegisterInstance(AppSettings.Load());
            containerRegistry.RegisterInstance(GetCurrentUser());
            containerRegistry.RegisterInstance<ILogger>(LogManager.GetCurrentClassLogger());
            containerRegistry.RegisterSingleton<IMainRegionService, MainRegionService>();
            containerRegistry.RegisterSingleton<ILicenseManager, LicenseManager>();
            containerRegistry.Register<IFileDialogService, FileDialogService>();


            containerRegistry.RegisterForNavigation<NavigationMenuView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsView>();
            containerRegistry.RegisterForNavigation<SrzSettingsView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsStorageSettingsView>();
            containerRegistry.RegisterForNavigation<AttachedPatientsFileSettingsView>();
            containerRegistry.RegisterForNavigation<ExaminationsView>();
            containerRegistry.RegisterForNavigation<ExaminationsSettingsView>();
            containerRegistry.RegisterForNavigation<SrzSettingsView>();
            containerRegistry.RegisterForNavigation<CommonSettingsView>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<ProgressBarView>();
            containerRegistry.RegisterForNavigation<DepartmentsView>();
            containerRegistry.RegisterForNavigation<MedicsView>();
            containerRegistry.RegisterForNavigation<SpecialtiesView>();
            containerRegistry.RegisterForNavigation<EmployeesView>();
            containerRegistry.RegisterForNavigation<RegistersView>();
            containerRegistry.RegisterForNavigation<ServiceClassifiersView>();
            containerRegistry.RegisterForNavigation<ServiceClassifierItemsView>();
            containerRegistry.RegisterForNavigation<ComponentsView>();
            containerRegistry.RegisterForNavigation<IndicatorsView>();
            containerRegistry.RegisterForNavigation<RatiosView>();
            containerRegistry.RegisterForNavigation<CaseFiltersView>();
            containerRegistry.RegisterForNavigation<ReportView>();
            containerRegistry.RegisterForNavigation<NotificationDialogView>();
            containerRegistry.RegisterForNavigation<ColorDialogView>();
            containerRegistry.RegisterForNavigation<UsersView>();
            containerRegistry.RegisterForNavigation<WindowsAccountsView>();
            containerRegistry.RegisterForNavigation<PlanPermisionsView>();
            containerRegistry.RegisterForNavigation<PlanningView>();
            containerRegistry.RegisterForNavigation<ServiceAccountingSettingsView>();
        }

        User GetCurrentUser()
        {
            using var curentWindowsUser = UserPrincipal.Current;

            string sid = curentWindowsUser.Sid.ToString();

            AppDBContext dbContext = null;
            User currentUser = null;

            try
            {
                var settings = Container.Resolve<AppSettings>();
                dbContext = new AppDBContext(settings.Common.SqlServer, settings.Common.SqlDatabase, settings.Common.SqlLogin, settings.Common.SqlPassword);
                currentUser = dbContext.Users.Where(x => x.Sid == sid).Include(x => x.PlanningPermisions).FirstOrDefault();
            }
            catch (Exception)
            { }

            if (currentUser != null)
                return currentUser;

            currentUser = new User()
            {
                Name = curentWindowsUser.UserPrincipalName,
                Sid = sid
            };

            bool noUsers = true;

            try
            {
                noUsers = !dbContext.Users.Any();
            }
            catch (Exception)
            { }


            if (noUsers)
            {
                currentUser.ReportPermision = true;
                currentUser.AttachedPatientsPermision = true;
                currentUser.MedicalExaminationsPermision = true;
                currentUser.UsersPerimision = true;
                currentUser.ReferencesPerimision = true;
                currentUser.RegistersPermision = true;
                currentUser.SettingsPermision = true;
                currentUser.RegistersPermision = true;
            }

            return currentUser;
        }

        void LogUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            logger.Error((Exception)args.ExceptionObject, "AppDomainException");

            mainRegionService?.ShowNotificationDialog(appCrashMessage).Wait();
        }

        void LogDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            logger.Error(args.Exception, "XamlDispatcherException");

            mainRegionService?.ShowNotificationDialog(appCrashMessage).Wait();
        }
    }
}
