using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.Common;
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
        private List<Tuple<PatientExaminations, string>> result;
        private bool showErrors;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public bool ShowErrors { get => showErrors; set => SetProperty(ref showErrors, value); }
        public List<Tuple<PatientExaminations, string>> Result { get => result; set => SetProperty(ref result, value); }
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
            //Result = new List<Tuple<PatientExaminations, string>> {
            //    new Tuple<PatientExaminations, string>(pe, "success"),
            //    new Tuple<PatientExaminations, string>(pe, "success"),
            //    new Tuple<PatientExaminations, string>(pe, "success"),
            //    new Tuple<PatientExaminations, string>(pe, "error")
            //};
        }
        #endregion

        #region Методы
        private void ExportExaminationsExecute()
        {
            Result?.Clear();
            ShowErrors = false;

            if (!Settings.ConnectionIsValid)
            {
                MainRegionService.SetBusyStatus("Проверка настроек.");

                Settings.TestConnection();
                if (!Settings.ConnectionIsValid)
                {
                    MainRegionService.SetCompleteStatus("Не удалось подключиться к web-сервису.");
                    return;
                }
            }
            
            MainRegionService.SetBusyStatus("Выбор файлов.");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = Settings.PatientsFilePath;
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

            examinationService.AddCounterChangeEvent += UpdateProgress;

            Result = examinationService.AddPatientsExaminations(patientsExaminations)
                .OrderBy(x => x.Item1.Kind)
                .ThenBy(x => x.Item1.Year)
                .ThenByDescending(x => x.Item2)
                .ToList();           

            if (Result?.Count > 0)
                ShowErrors=true;

            examinationService.AddCounterChangeEvent -= UpdateProgress;

            MainRegionService.SetCompleteStatus("Успешно завершено.");            
        }
        private void UpdateProgress(object sender, CounterEventArgs args)
        {
            MainRegionService.SetBusyStatus($"Загрузка осмотров. Загружено пациентов: {args.Counter} из {args.Total}.");
        }
        #endregion



    }
}
