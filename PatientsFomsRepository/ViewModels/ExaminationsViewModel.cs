using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.MedicalExaminations;
using Prism.Commands;
using Prism.Regions;
using System.ComponentModel;
using System.Linq;

namespace CHI.Application.ViewModels
{
    class ExaminationsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync ExportExaminationsCommand { get; }
        #endregion

        #region Конструкторы
        public ExaminationsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Загрузка осмотров на портал диспансеризации";

            ExportExaminationsCommand = new DelegateCommandAsync(ExportExaminationsExecute);
        }
        #endregion

        #region Методы
        private void ExportExaminationsExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = settings.PatientsFilePath;
            fileDialogService.MiltiSelect = true;
            fileDialogService.Filter = "Zip files (*.zip)|*.zip|Xml files (*.xml)|*.xml";

            if (fileDialogService.ShowDialog() != true)
                return;

            var files = fileDialogService.FileNames;

            MainRegionService.SetBusyStatus("Открытие файла.");

            var registers = new BillsRegisterService(files);
            var patientsFileNames = Settings.PatientFileNames.Split(',');
            var examinationFileNames= Settings.ExaminationFileNames.Split(',');
            var patientsExaminations=registers.GetPatientsExaminations(examinationFileNames, patientsFileNames);

            MainRegionService.SetBusyStatus("Экспорт осмотров на портал диспансеризации.");

            var examinationService = new ExaminationServiceParallel(Settings.MedicalExaminationsAddress, Settings.ProxyAddress, Settings.ProxyPort, Settings.ThreadsLimit, Settings.Credentials);
            var errors=examinationService.AddPatientsExaminations(patientsExaminations);

            MainRegionService.SetCompleteStatus("Успешно завершено.");
        }
        #endregion



    }
}
