using CHI.Infrastructure;
using CHI.Models;
using CHI.Settings;
using Prism.Commands;
using Prism.Regions;
using System.Linq;

namespace CHI.ViewModels
{
    public class AttachedPatientsFileSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        ColumnProperty currentColumnProperty;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AttachedPatientsFileSettings Settings { get; set; }
        public ColumnProperty CurrentColumnProperty { get => currentColumnProperty; set => SetProperty(ref currentColumnProperty, value); }


        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }


        public AttachedPatientsFileSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            Settings = settings.AttachedPatientsFile;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Форматирование файла прикрепленных пациентов";

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute)                
                .ObservesProperty(() => CurrentColumnProperty)
                .ObservesProperty(() => Settings.ApplyFormat);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute)
                .ObservesProperty(() => CurrentColumnProperty)
                .ObservesProperty(() => Settings.ApplyFormat);
        }

        void SetDefaultExecute()
        {
            Settings.SetDefault();
            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }

        void MoveUpExecute()
        {
            Settings.MoveUpColumnProperty(CurrentColumnProperty);

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        void MoveDownExecute()
        {
            Settings.MoveDownColumnProperty(CurrentColumnProperty);

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        bool MoveUpCanExecute()
        {
            if (CurrentColumnProperty == null)
                return false;

            if ((Settings.ColumnProperties?.Any() ?? false) == false || Settings.ColumnProperties.First() == CurrentColumnProperty)
                return false;

            return Settings.ApplyFormat;
        }

        bool MoveDownCanExecute()
        {
            if (CurrentColumnProperty == null)
                return false;

            if ((Settings.ColumnProperties?.Any() ?? false) == false || Settings.ColumnProperties.Last() == CurrentColumnProperty)
                return false;

            return Settings.ApplyFormat;
        }
    }
}