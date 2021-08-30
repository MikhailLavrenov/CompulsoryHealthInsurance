using CHI.Infrastructure;
using CHI.Models;
using CHI.Settings;
using Prism.Commands;
using Prism.Regions;

namespace CHI.ViewModels
{
    public class AttachedPatientsFileSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AttachedPatientsFileSettings Settings { get; set; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand<ColumnProperty> MoveUpCommand { get; }
        public DelegateCommand<ColumnProperty> MoveDownCommand { get; }


        public AttachedPatientsFileSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            Settings = settings.AttachedPatientsFile;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Форматирование файла прикрепленных пациентов";

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MoveUpCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveUpColumnProperty(x));
            MoveDownCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveDownColumnProperty(x));
        }


        private void SetDefaultExecute()
        {
            Settings.SetDefault();
            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }
    }
}