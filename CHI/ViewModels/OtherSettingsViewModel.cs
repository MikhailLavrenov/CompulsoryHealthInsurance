using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace CHI.ViewModels
{
    public class OtherSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        private Settings settings;
        private IDialogService dialogService;
        private readonly IFileDialogService fileDialogService;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync MigrateDBCommand { get; }


        public OtherSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Прочие настройки";

            TestCommand = new DelegateCommandAsync(TestExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MigrateDBCommand = new DelegateCommandAsync(MigrateDBExecute);
        }


        private void TestExecute()
        {
            MainRegionService.ShowProgressBar("Проверка настроек.");
            Settings.TestConnectionProxy();

            if (Settings.ProxyConnectionIsValid)
                MainRegionService.HideProgressBar("Настройки корректны.");
            else
                MainRegionService.HideProgressBar("Прокси сервер не доступен.");
        }

        private void SetDefaultExecute()
        {
            Settings.SetDefaultOther();
            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }

        private void MigrateDBExecute()
        {
            MainRegionService.ShowProgressBar("Обновление структуры базы данных.");
            var dbContext = new ServiceAccountingDBContext();
            dbContext.Database.Migrate();
            MainRegionService.HideProgressBar("Обновление структуры базы данных успешно завершено.");
        }
    }
}
