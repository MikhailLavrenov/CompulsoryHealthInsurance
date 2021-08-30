using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Settings;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System.Linq;

namespace CHI.ViewModels
{
    public class CommonSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AppSettings Settings { get; set; }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync MigrateDBCommand { get; }


        public CommonSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            Settings = settings;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Общие настройки";

            TestCommand = new DelegateCommandAsync(TestExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MigrateDBCommand = new DelegateCommandAsync(MigrateDBExecute);
        }


        async void TestExecute()
        {
            MainRegionService.ShowProgressBar("Проверка настроек.");
            await Settings.TestConnectionProxyAsync();

            if (Settings.Common.ProxyConnectionIsValid)
                MainRegionService.HideProgressBar("Настройки корректны.");
            else
                MainRegionService.HideProgressBar("Прокси сервер не доступен.");
        }

        private void SetDefaultExecute()
        {
            Settings.Common.SetDefault();
            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }

        private void MigrateDBExecute()
        {
            MainRegionService.ShowProgressBar("Обновление структуры базы данных.");
            var dbContext = new AppDBContext(Settings.Common.SQLServer, Settings.Common.SQLServerDB);
            dbContext.Database.Migrate();

            var rootDepartment = dbContext.Departments.FirstOrDefault(x => x.IsRoot);

            if (rootDepartment == null)
            {
                rootDepartment = new Department { IsRoot = true };
                dbContext.Add(rootDepartment);
                dbContext.SaveChanges();
            }

            var rootComponent = dbContext.Components.FirstOrDefault(x => x.IsRoot);

            if (rootComponent == null)
            {
                rootComponent = new Component { IsRoot = true };
                dbContext.Add(rootComponent);
                dbContext.SaveChanges();
            }

            MainRegionService.HideProgressBar("Обновление структуры базы данных успешно завершено.");
        }
    }
}
