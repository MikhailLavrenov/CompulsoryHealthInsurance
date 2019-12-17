using CHI.Application.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using System;
using System.IO;

namespace CHI.Application.ViewModels
{
    class LicenseManagerViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private readonly IFileDialogService fileDialogService;
        private readonly ILicenseManager licenseManager;
        private License currentLicense;
        private bool showLicense;
        private bool showSave;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public DelegateCommand OpenLicenseCommand { get; }
        public DelegateCommand NewLicenseCommand { get; }
        public DelegateCommand SaveLicenseCommand { get; }
        public License CurrentLicense { get => currentLicense; set => SetProperty(ref currentLicense, value); }
        public bool ShowLicense { get => showLicense; set => SetProperty(ref showLicense, value); }
        public bool ShowSave { get => showSave; set { showSave = value; SaveLicenseCommand.RaiseCanExecuteChanged(); } }
        #endregion

        #region Конструкторы
        public LicenseManagerViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, ILicenseManager licenseManager)
        {
            this.licenseManager = licenseManager;
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Менеджер лицензий";
            ShowLicense = false;

            OpenLicenseCommand = new DelegateCommand(OpenLicenseExecute);
            NewLicenseCommand = new DelegateCommand(NewLicenseExecute);
            SaveLicenseCommand = new DelegateCommand(SaveLicenseExecute, () => ShowSave);
        }
        #endregion

        #region Методы
        private void OpenLicenseExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "License file (*.lic)|*.lic";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.SetCompleteStatus("Отменено.");
                return;
            }

            CurrentLicense = licenseManager.LoadLicense(fileDialogService.FileName);

            ShowLicense = true;
            ShowSave = false;


            MainRegionService.SetCompleteStatus($"Лицензия открыта: {Path.GetFileName(fileDialogService.FileName)}");
        }
        private void NewLicenseExecute()
        {
            ShowLicense = true;
            ShowSave = true;

            CurrentLicense = new License();
            MainRegionService.SetCompleteStatus("Новая лицензия.");
        }
        private void SaveLicenseExecute()
        {
            var dateTimeStr = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF");

            fileDialogService.FileName = $@"{Environment.SpecialFolder.Desktop}\License {dateTimeStr}.lic";
            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.Filter = "License file (*.lic)|*.lic";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.SetCompleteStatus("Отменено.");
                return;
            }

            ShowSave = false;
            ShowLicense = false;

            licenseManager.SaveLicense(CurrentLicense, fileDialogService.FileName);
            MainRegionService.SetCompleteStatus("Лицензия сохранена.");
        }
        #endregion
    }
}
