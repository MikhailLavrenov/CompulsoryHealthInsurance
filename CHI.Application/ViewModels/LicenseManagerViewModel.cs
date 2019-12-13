using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Licensing;
using CHI.Services.BillsRegister;
using CHI.Services.Common;
using CHI.Services.MedicalExaminations;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.Application.ViewModels
{
    class LicenseManagerViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private readonly IFileDialogService fileDialogService;
        private readonly ILicenseManager licenseManager;
        private License currentLicense;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public DelegateCommand OpenLicenseCommand { get; }
        public DelegateCommand NewLicenseCommand { get; }
        public DelegateCommand SaveLicenseCommand { get; }
        public License CurrentLicense { get =>currentLicense; set=>SetProperty(ref currentLicense, value); }

        #endregion

        #region Конструкторы
        public LicenseManagerViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, ILicenseManager licenseManager)
        {
            this.licenseManager = licenseManager;
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Менеджер лицензий";

            OpenLicenseCommand = new DelegateCommand(OpenLicenseExecute);
            NewLicenseCommand = new DelegateCommand(NewLicenseExecute);
            SaveLicenseCommand = new DelegateCommand(SaveLicenseExecute);
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

            MainRegionService.SetCompleteStatus("Лицензия открыта.");
        }
        private void NewLicenseExecute()
        {
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

            licenseManager.SaveLicense(CurrentLicense, fileDialogService.FileName);

            MainRegionService.SetCompleteStatus("Лицензия сохранена.");               
        }
        #endregion



    }
}
