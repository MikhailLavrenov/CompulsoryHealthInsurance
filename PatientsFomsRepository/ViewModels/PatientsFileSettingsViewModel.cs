using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using Prism.Commands;
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
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand<ColumnProperty> MoveUpCommand { get; }
        public DelegateCommand<ColumnProperty> MoveDownCommand { get; }
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
            SaveCommand = new DelegateCommand(SaveExecute);
            LoadCommand = new DelegateCommand(LoadExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MoveUpCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveUpColumnProperty(x as ColumnProperty));
            MoveDownCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveDownColumnProperty(x as ColumnProperty));            
        }
        #endregion

        #region Методы
        private void SaveExecute()
        {
            Settings.Save();
            ActiveViewModel.Status = "Настройки сохранены.";
        }
        private void LoadExecute()
        {
            Settings = Settings.Load();
            ActiveViewModel.Status = "Изменения настроек отменены.";
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultPatiensFile();
            ActiveViewModel.Status = "Настройки установлены по умолчанию.";
        }
        #endregion
    }
}