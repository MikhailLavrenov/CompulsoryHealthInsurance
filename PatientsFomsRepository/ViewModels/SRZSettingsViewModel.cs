using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Linq;
using System.Text;

namespace PatientsFomsRepository.ViewModels
{
    public class SRZSettingsViewModel : BindableBase, IViewModel
    {
        #region Поля
        private Settings settings;
        private bool showTextPassword;
        private bool showProtectedPassword;
        #endregion

        #region Свойства
        public IStatusBar StatusBar { get; set; }
        public bool KeepAlive { get => false; }
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public bool ShowTextPassword { get => showTextPassword; set => SetProperty(ref showTextPassword, value); }
        public bool ShowProtectedPassword { get => showProtectedPassword; set => SetProperty(ref showProtectedPassword, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommandAsync TestCommand { get; }
        public RelayCommand SwitchShowPasswordCommand { get; }
        #endregion

        #region Конструкторы
        public SRZSettingsViewModel()
        {
        }
        public SRZSettingsViewModel(IStatusBar statusBar)
        {
            ShortCaption = "Настройки СРЗ";
            FullCaption = "Настройки подключения к СРЗ ХК ФОМС";
            StatusBar = statusBar;
            Settings = Settings.Instance;
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;
            SaveCommand = new RelayCommand(SaveCommandExecute);
            LoadCommand = new RelayCommand(LoadCommandExecute);
            SetDefaultCommand = new RelayCommand(SetDefaultExecute);
            TestCommand = new RelayCommandAsync(TestExecute);
            SwitchShowPasswordCommand = new RelayCommand(SwitchShowPasswordExecute);
        }
        #endregion

        #region Методы        
        private void SaveCommandExecute(object parameter)
        {
            Settings.Save();
            StatusBar.StatusText = "Настройки сохранены.";
        }
        private void LoadCommandExecute(object parameter)
        {
            Settings = Settings.Load();
            StatusBar.StatusText = "Изменения настроек отменены.";
        }
        private void SetDefaultExecute(object parameter)
        {
            Settings.SetDefaultSRZ();
            StatusBar.StatusText = "Настройки установлены по умолчанию.";
        }
        private void TestExecute(object parameter)
        {
            StatusBar.StatusText = "Ожидайте. Проверка настроек...";
            Settings.TestConnection();

            if (Settings.ConnectionIsValid)
                StatusBar.StatusText = "Завершено. Настройки корректны.";
            else if (Settings.ProxyIsNotValid)
                StatusBar.StatusText = "Завершено. Прокси сервер не доступен.";
            else if (Settings.SiteAddressIsNotValid)
                StatusBar.StatusText = "Завершено. Web-сайт СРЗ не доступен.";
            else if (Settings.CredentialsIsNotValid)
            {
                var logins = new StringBuilder();
                Settings.Credentials
                .Where(x => x.IsNotValid)
                .ToList()
                .ForEach(x => logins.Append(x).Append(", "));
                logins.Remove(logins.Length - 3, 2);

                StatusBar.StatusText = $"Завершено. Учетные записи не верны: {logins}. ";
            }
        }
        private void SwitchShowPasswordExecute(object parameter)
        {
            ShowTextPassword = !ShowTextPassword;
            ShowProtectedPassword = !ShowTextPassword;
        }
        #endregion
    }
}
;