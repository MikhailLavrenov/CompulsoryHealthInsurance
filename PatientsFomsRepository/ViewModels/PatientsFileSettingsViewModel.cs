using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;

namespace PatientsFomsRepository.ViewModels
{
    public class PatientsFileSettingsViewModel : BindableBase, IViewModel
    {
        #region Поля
        private Settings settings;
        private string progress;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public string Progress { get => progress; set => SetProperty(ref progress, value); }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }
        #endregion

        #region Конструкторы
        public PatientsFileSettingsViewModel()
        {
            ShortCaption = "Настройки файла пациентов";
            FullCaption = "Настройки  файла пациентов";
            Progress = "";
            Settings = Settings.Instance;
            SaveCommand = new RelayCommand(SaveExecute);
            LoadCommand = new RelayCommand(LoadExecute);
            SetDefaultCommand = new RelayCommand(SetDefaultExecute);
            MoveUpCommand = new RelayCommand(x => Settings.MoveUpColumnProperty(x as ColumnProperty));
            MoveDownCommand = new RelayCommand(x => Settings.MoveDownColumnProperty(x as ColumnProperty));            
        }
        #endregion

        #region Методы
        private void SaveExecute(object parameter)
        {
            Settings.Save();
            Progress = "Настройки сохранены.";
        }
        private void LoadExecute(object parameter)
        {
            Settings = Settings.Load();
            Progress = "Изменения настроек отменены.";
        }
        private void SetDefaultExecute(object parameter)
        {
            Settings.SetDefaultPatiensFile();
            Progress = "Настройки установлены по умолчанию.";
        }
        #endregion
    }
}