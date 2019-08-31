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
        private string progress;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public string Progress { get => progress; set => SetProperty(ref progress, value); }
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
            ShortCaption = "Настройки СРЗ";
            FullCaption = "Настройки подключения к СРЗ ХК ФОМС";
            Progress = "";
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
            Progress = "Настройки сохранены.";
        }
        private void LoadCommandExecute(object parameter)
        {
            Settings = Settings.Load();
            Progress = "Изменения настроек отменены.";
        }
        private void SetDefaultExecute(object parameter)
        {
            Settings.SetDefaultSRZ();
            Progress = "Настройки установлены по умолчанию.";
        }
        private void TestExecute(object parameter)
        {
            Progress = "Ожидайте. Проверка настроек...";
            Settings.TestConnection();

            if (Settings.ConnectionIsValid)
                Progress = "Завершено. Настройки корректны.";
            else if (Settings.ProxyIsNotValid)
                Progress = "Завершено. Прокси сервер не доступен.";
            else if (Settings.SiteAddressIsNotValid)
                Progress = "Завершено. Web-сайт СРЗ не доступен.";
            else if (Settings.CredentialsIsNotValid)
            {
                var logins = new StringBuilder();
                Settings.Credentials
                .Where(x => x.IsNotValid)
                .ToList()
                .ForEach(x => logins.Append(x).Append(", "));
                logins.Remove(logins.Length - 3, 2);

                Progress = $"Завершено. Учетные записи не верны: {logins}. ";
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