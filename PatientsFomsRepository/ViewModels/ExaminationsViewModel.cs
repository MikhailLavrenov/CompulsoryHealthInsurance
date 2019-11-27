using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.MedicalExaminations;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CHI.Application.ViewModels
{
    class ExaminationsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private List<PatientExaminations> errors;
        private bool showErrors;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public bool ShowErrors { get => showErrors; set => SetProperty(ref showErrors, value); }
        public List<PatientExaminations> Errors { get => errors; set => SetProperty(ref errors, value); }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync ExportExaminationsCommand { get; }
        #endregion

        #region Конструкторы
        public ExaminationsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            ShowErrors = false;
            Settings = Settings.Instance;
            MainRegionService.Header = "Загрузка осмотров на портал диспансеризации";

            ExportExaminationsCommand = new DelegateCommandAsync(ExportExaminationsExecute);


           // var examination1Stage = new Examination
           // {
           //     BeginDate = new DateTime(2019, 10, 10),
           //     EndDate = new DateTime(2019, 10, 15),
           //     HealthGroup = ExaminationHealthGroup.ThirdA,
           //     Referral = ExaminationReferral.LocalClinic
           // };
           // var examination2Stage = new Examination
           // {
           //     BeginDate = new DateTime(2019, 10, 20),
           //     EndDate = new DateTime(2019, 10, 25),
           //     HealthGroup = ExaminationHealthGroup.ThirdB,
           //     Referral = ExaminationReferral.AnotherClinic
           // };

           // ShowErrors = true;
           //var pe = new PatientExaminations("2751530822000157", 2019, ExaminationKind.Dispanserizacia1)
           // {
           //     Stage1 = examination1Stage,
           //     Stage2 = examination2Stage
           // };
           // Errors = new List<PatientExaminations> { pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe, pe };
        }

        public ExaminationsViewModel(bool showErrors)
        {
            this.showErrors = showErrors;
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
            var Errors = examinationService.AddPatientsExaminations(patientsExaminations);
            ShowErrors = true;

            MainRegionService.SetCompleteStatus("Успешно завершено.");
        }
        #endregion



    }
}
