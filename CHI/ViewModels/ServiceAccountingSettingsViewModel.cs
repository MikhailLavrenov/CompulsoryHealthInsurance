using CHI.Infrastructure;
using CHI.Models.AppSettings;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace CHI.ViewModels
{
    public class ServiceAccountingSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        readonly IFileDialogService fileDialogService;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AppSettings Settings { get; set; }

        public DelegateCommandAsync SelectFileCommand { get; set; }


        public ServiceAccountingSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            Settings = settings;
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Настройки учета услуг";

            SelectFileCommand = new DelegateCommandAsync(SelectFileExecute);
        }


        public void SelectFileExecute()
        {
            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            Settings.ServiceAccounting.ReportPath = fileDialogService.FileName;
        }
    }
}
