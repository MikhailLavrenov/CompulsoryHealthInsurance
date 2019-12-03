using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.MedicalExaminations;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Application.ViewModels
{
    class ExaminationsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private Dictionary<PatientExaminations, string> result;
        private bool showErrors;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public bool ShowErrors { get => showErrors; set => SetProperty(ref showErrors, value); }
        public Dictionary<PatientExaminations, string> Result { get => result; set => SetProperty(ref result, value); }
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


            //var examination1Stage = new Examination
            //{
            //    BeginDate = new DateTime(2019, 10, 10),
            //    EndDate = new DateTime(2019, 10, 15),
            //    HealthGroup = ExaminationHealthGroup.ThirdA,
            //    Referral = ExaminationReferral.LocalClinic
            //};
            //var examination2Stage = new Examination
            //{
            //    BeginDate = new DateTime(2019, 10, 20),
            //    EndDate = new DateTime(2019, 10, 25),
            //    HealthGroup = ExaminationHealthGroup.ThirdB,
            //    Referral = ExaminationReferral.AnotherClinic
            //};

            //ShowErrors = true;
            //var pe = new PatientExaminations("2751530822000157", 2019, ExaminationKind.Dispanserizacia1)
            //{
            //    Stage1 = examination1Stage,
            //    Stage2 = examination2Stage
            //};
            //Result = new Dictionary<PatientExaminations, string> { {pe, "success" } };
        }
        #endregion

        #region Методы
        private void ExportExaminationsExecute()
        {
            Result?.Clear();
            ShowErrors = false;

            MainRegionService.SetBusyStatus("Выбор файлов.");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = settings.PatientsFilePath;
            fileDialogService.MiltiSelect = true;
            fileDialogService.Filter = "Zip files (*.zip)|*.zip|Xml files (*.xml)|*.xml";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.SetCompleteStatus("Отменено.");
                return;
            }

            var files = fileDialogService.FileNames;

            MainRegionService.SetBusyStatus("Чтение файлов.");

            var registers = new BillsRegisterService(files);
            var patientsFileNames = Settings.PatientFileNames.Split(',');           
            var examinationFileNames = Settings.ExaminationFileNames.Split(',');

            for (int i = 0; i < patientsFileNames.Length; i++)
                patientsFileNames[i] = patientsFileNames[i].Trim();
            for (int i = 0; i < examinationFileNames.Length; i++)
                examinationFileNames[i] = examinationFileNames[i].Trim();

            var patientsExaminations = registers.GetPatientsExaminations(examinationFileNames, patientsFileNames);

            MainRegionService.SetBusyStatus($"Загрузка осмотров. Всего пациентов: {patientsExaminations.Count}.");

            var examinationService = new ExaminationServiceParallel(Settings.MedicalExaminationsAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort, Settings.ThreadsLimit, Settings.Credentials);

            Result=examinationService.AddPatientsExaminations(patientsExaminations);

            if (Result?.Count > 0)
                ShowErrors=true;

            MainRegionService.SetCompleteStatus("Успешно завершено.");
        }
        #endregion



    }
}
