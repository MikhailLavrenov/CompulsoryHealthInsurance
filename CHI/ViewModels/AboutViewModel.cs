using CHI.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CHI.Application.ViewModels
{
    class AboutViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        #region Поля
        private readonly string manualPath;
        private readonly IFileDialogService fileDialogService;
        private readonly ILicenseManager licenseManager;
        private string license;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => true; }
        public string Name { get; }
        public string Version { get; }
        public string Copyright { get; }
        public string Author { get; }
        public string Email { get; }
        public string Phone { get; }
        public string License { get => license; set => SetProperty(ref license, value); }
        public DelegateCommand OpenManualCommand { get; }
        public DelegateCommandAsync ImportLicenseCommand { get; }
        #endregion

        #region Конструкторы
        public AboutViewModel(IMainRegionService mainRegionService, ILicenseManager licenseManager, IFileDialogService fileDialogService)
        {
            MainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;
            this.licenseManager = licenseManager;
            var assembly = Assembly.GetExecutingAssembly();

            manualPath = "Инструкция.docx";
            Name = ((AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false).First()).Title;
            Version = assembly.GetName().Version.ToString();
            Copyright = @"©  2020";
            Author = "Лавренов Михаил Владимирович";
            Email = "mvlavrenov@mail.ru";
            Phone = "8-924-213-79-11";
            License = licenseManager.GetActiveLicenseInfo();

            OpenManualCommand = new DelegateCommand(() => Process.Start(manualPath), () => File.Exists(manualPath));
            ImportLicenseCommand = new DelegateCommandAsync(ImportLicenseExecute);
        }
        #endregion

        #region Методы
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            MainRegionService.Header = "О программе";
        }
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
        private void ImportLicenseExecute()
        {
            MainRegionService.SetBusyStatus("Импорт лицензии.");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "License file (*.lic)|*.lic";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.SetCompleteStatus("Отменено.");
                return;
            }

            var existenLicenseFiles = Directory.GetFiles(licenseManager.DefaultDirectory, $"*{licenseManager.LicenseExtension}").ToList();

            foreach (var file in existenLicenseFiles)
                File.Move(file,Path.ChangeExtension(file,".old"));

            var destination = Path.Combine(licenseManager.DefaultDirectory, Path.GetFileName(fileDialogService.FileName));

            File.Copy(fileDialogService.FileName, destination);

            licenseManager.Initialize();
            License = licenseManager.GetActiveLicenseInfo();

            MainRegionService.SetCompleteStatus("Лицензия установлена.");
        }
        #endregion

    }
}
