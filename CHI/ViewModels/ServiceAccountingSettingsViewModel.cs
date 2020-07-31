using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Linq;

namespace CHI.ViewModels
{
    public class ServiceAccountingSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        private Settings settings;
        private IDialogService dialogService;
        private readonly IFileDialogService fileDialogService;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }

        public DelegateCommandAsync SelectFileCommand { get; set; }


        public ServiceAccountingSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Настройки учета услуг";

            SelectFileCommand = new DelegateCommandAsync(SelectFileExecute);
        }


        public void SelectFileExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            Settings.ServiceAccountingReportPath = fileDialogService.FileName;
        }

    }
}
