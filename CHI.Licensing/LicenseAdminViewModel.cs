using CHI.Infrastructure;
using Prism.Commands;
using System;
using System.IO;

namespace CHI.Licensing
{
    public class LicenseAdminViewModel : DomainObject
    {
        #region Поля
        private readonly IFileDialogService fileDialogService;
        private readonly LicenseAdmin licenseAdmin;
        private License currentLicense;
        private bool showLicense;
        private bool showSave;
        private string status;
        private string licenseFileName;
        #endregion

        #region Свойства
        public DelegateCommand NewSignKeysCommand { get; }
        public DelegateCommand OpenLicenseCommand { get; }
        public DelegateCommand NewLicenseCommand { get; }
        public DelegateCommand SaveLicenseCommand { get; }
        public License CurrentLicense { get => currentLicense; set => SetProperty(ref currentLicense, value); }
        public bool ShowLicense { get => showLicense; set => SetProperty(ref showLicense, value); }
        public bool ShowSave { get => showSave; set { showSave = value; SaveLicenseCommand.RaiseCanExecuteChanged(); } }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public string LicenseFileName { get => licenseFileName; set => SetProperty(ref licenseFileName, value); }

        #endregion

        #region Конструкторы
        public LicenseAdminViewModel(IFileDialogService fileDialogService)
        {
            licenseAdmin = new LicenseAdmin();
            this.fileDialogService = fileDialogService;

            Status = string.Empty;
            ShowLicense = false;

            NewSignKeysCommand = new DelegateCommand(NewSignKeysExecute, NewSignKeysCanExecute);
            OpenLicenseCommand = new DelegateCommand(OpenLicenseExecute);
            NewLicenseCommand = new DelegateCommand(NewLicenseExecute);
            SaveLicenseCommand = new DelegateCommand(SaveLicenseExecute, () => ShowSave);
        }
        #endregion

        #region Методы
        private void NewSignKeysExecute()
        {
            licenseAdmin.NewSignKeyPair();
            licenseAdmin.Initialize();
            NewSignKeysCommand.RaiseCanExecuteChanged();
            Status = "Создана новая пара ключей для подписания лицензий.";
        }
        private bool NewSignKeysCanExecute()
        {
            if (File.Exists(licenseAdmin.SecretKeyPath) || File.Exists(licenseAdmin.PublicKeyPath))
                return false;

            return true;
        }
        private void OpenLicenseExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "License file (*.lic)|*.lic";

            if (fileDialogService.ShowDialog() != true)
            {
                Status = "Отменено.";
                return;
            }

            CurrentLicense = licenseAdmin.LoadLicense(fileDialogService.FileName);

            ShowLicense = true;
            ShowSave = false;

            LicenseFileName = Path.GetFileName(fileDialogService.FileName);
            Status = $"Лицензия открыта: {LicenseFileName}";
        }
        private void NewLicenseExecute()
        {
            ShowLicense = true;
            ShowSave = true;

            CurrentLicense = new License();

            LicenseFileName = "Новая";
            Status = "Новая лицензия.";
        }
        private void SaveLicenseExecute()
        {
            var dateTimeStr = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF");

            fileDialogService.FileName = $@"{Environment.SpecialFolder.Desktop}\License {dateTimeStr}.lic";
            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.Filter = "License file (*.lic)|*.lic";

            if (fileDialogService.ShowDialog() != true)
            {
                Status = "Отменено.";
                return;
            }

            ShowSave = false;
            ShowLicense = false;

            licenseAdmin.SignAndSaveLicense(CurrentLicense, fileDialogService.FileName);
            Status = "Лицензия сохранена.";
        }
        #endregion
    }
}
