using CHI.Infrastructure;
using CHI.Models;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace CHI.ViewModels
{
    public class OtherSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private IDialogService dialogService;
        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        #endregion

        #region Конструкторы
        public OtherSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Прочие настройки";

            TestCommand = new DelegateCommandAsync(TestExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
        }
        #endregion

        #region Методы
        private void TestExecute()
        {
            MainRegionService.ShowProgressBarWithMessage("Проверка настроек.");
            Settings.TestConnectionProxy();

            if (Settings.ProxyConnectionIsValid)
                MainRegionService.HideProgressBarWithhMessage("Настройки корректны.");
            else
                MainRegionService.HideProgressBarWithhMessage("Прокси сервер не доступен.");
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultOther();
            MainRegionService.HideProgressBarWithhMessage("Настройки установлены по умолчанию.");
        }
        #endregion
    }
}
