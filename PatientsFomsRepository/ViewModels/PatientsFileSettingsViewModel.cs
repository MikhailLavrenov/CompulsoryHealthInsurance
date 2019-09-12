using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using Prism.Regions;

namespace PatientsFomsRepository.ViewModels
{
    public class PatientsFileSettingsViewModel : BindableBase, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        #endregion

        #region Свойства
        public IActiveViewModel ActiveViewModel { get; set; }
        public bool KeepAlive { get => false; }
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
        }
        public PatientsFileSettingsViewModel(IActiveViewModel activeViewModel)
        {
            ActiveViewModel = activeViewModel;

            ActiveViewModel.Header = "Настройки файла пациентов";            
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
            ActiveViewModel.Status = "Настройки сохранены.";
        }
        private void LoadExecute(object parameter)
        {
            Settings = Settings.Load();
            ActiveViewModel.Status = "Изменения настроек отменены.";
        }
        private void SetDefaultExecute(object parameter)
        {
            Settings.SetDefaultPatiensFile();
            ActiveViewModel.Status = "Настройки установлены по умолчанию.";
        }
        #endregion
    }
}