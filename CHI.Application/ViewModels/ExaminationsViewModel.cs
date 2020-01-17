using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.Common;
using CHI.Services.MedicalExaminations;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.Application.ViewModels
{
    class ExaminationsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private List<Tuple<PatientExaminations, bool, string>> result;
        private bool showErrors;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public ILicenseManager LicenseManager { get; set; }
        public bool KeepAlive { get => false; }
        public bool ShowErrors { get => showErrors; set => SetProperty(ref showErrors, value); }
        public List<Tuple<PatientExaminations, bool, string>> Result { get => result; set => SetProperty(ref result, value); }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync ExportExaminationsCommand { get; }
        #endregion

        #region Конструкторы
        public ExaminationsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, ILicenseManager licenseManager)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;
            LicenseManager = licenseManager;

            Result = new List<Tuple<PatientExaminations, bool, string>>();

            ShowErrors = false;
            Settings = Settings.Instance;
            MainRegionService.Header = "Загрузка периодических осмотров на портал";

            ExportExaminationsCommand = new DelegateCommandAsync(ExportExaminationsExecute);


            var examination1Stage = new Examination
            {
                BeginDate = new DateTime(2019, 10, 10),
                EndDate = new DateTime(2019, 10, 15),
                HealthGroup = HealthGroup.ThirdA,
                Referral = Referral.LocalClinic
            };
            var examination2Stage = new Examination
            {
                BeginDate = new DateTime(2019, 10, 20),
                EndDate = new DateTime(2019, 10, 25),
                HealthGroup = HealthGroup.ThirdB,
                Referral = Referral.AnotherClinic
            };

            ShowErrors = true;
            var pe = new PatientExaminations("2751530822000157", 2019, ExaminationKind.Dispanserizacia1)
            {
                Stage1 = examination1Stage,
                Stage2 = examination2Stage
            };
            Result = new List<Tuple<PatientExaminations, bool, string>> {
                new Tuple<PatientExaminations, bool,string>(pe, true, ""),
                new Tuple<PatientExaminations,bool, string>(pe, true, ""),
                new Tuple<PatientExaminations,bool, string>(pe, false, "Какая то надпись"),
                new Tuple<PatientExaminations, bool,string>(pe, false, "Еще одна надпись"),
            };
        }
        #endregion

        #region Методы
        private void ExportExaminationsExecute()
        {
            Result?.Clear();
            ShowErrors = false;

            if (!Settings.ExaminationsConnectionIsValid)
            {
                MainRegionService.SetBusyStatus("Проверка настроек.");

                Settings.TestConnectionExaminations();
                if (!Settings.ExaminationsConnectionIsValid)
                {
                    MainRegionService.SetCompleteStatus("Не удалось подключиться к web-сервису.");
                    return;
                }
            }

            MainRegionService.SetBusyStatus("Выбор файлов.");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = Settings.ExaminationsFileDirectory;
            fileDialogService.MiltiSelect = true;
            fileDialogService.Filter = "Zip files (*.zip)|*.zip|Xml files (*.xml)|*.xml";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.SetCompleteStatus("Отменено.");
                return;
            }

            Settings.ExaminationsFileDirectory =Path.GetDirectoryName(fileDialogService.FileNames.FirstOrDefault());

            MainRegionService.SetBusyStatus("Чтение файлов.");

            var registers = new BillsRegisterService(fileDialogService.FileNames);
            var patientsFileNames = Settings.PatientFileNames.Split(',');
            var examinationFileNames = Settings.ExaminationFileNames.Split(',');

            for (int i = 0; i < patientsFileNames.Length; i++)
                patientsFileNames[i] = patientsFileNames[i].Trim();
            for (int i = 0; i < examinationFileNames.Length; i++)
                examinationFileNames[i] = examinationFileNames[i].Trim();

            var patientsExaminations = registers.GetPatientsExaminations(examinationFileNames, patientsFileNames);

            var maxDate1 = patientsExaminations.Max(x => x.Stage1?.EndDate);
            var maxDate2 = patientsExaminations.Max(x => x.Stage2?.EndDate);
            var maxDate = maxDate1 > maxDate2 ? maxDate1 : maxDate2;

            var license = LicenseManager.ActiveLicense;

            if (!(license.ExaminationsUnlimited || license.ExaminationsFomsCodeMO == Settings.FomsCodeMO || license.ExaminationsMaxDate > maxDate))
            {
                MainRegionService.SetCompleteStatus("Отменено, ограничение лицензии.");
                return;
            }

            MainRegionService.SetBusyStatus($"Загрузка осмотров. Всего пациентов: {patientsExaminations.Count}.");

            var examinationService = new ExaminationServiceParallel(Settings.ExaminationsAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort, Settings.ExaminationsThreadsLimit, Settings.ExaminationsCredentials);

            examinationService.AddCounterChangeEvent += UpdateProgress;

            Result = examinationService.AddPatientsExaminations(patientsExaminations)
                .OrderBy(x => x.Item2)
                .ThenBy(x => x.Item1.Kind)
                .ThenBy(x => x.Item1.Year)
                .ToList();

            if (Result?.Count > 0)
                ShowErrors = true;

            examinationService.AddCounterChangeEvent -= UpdateProgress;

            MainRegionService.SetCompleteStatus("Завершено.");
        }
        private void UpdateProgress(object sender, CounterEventArgs args)
        {
            MainRegionService.SetBusyStatus($"Загрузка осмотров. Обработано пациентов: {args.Counter} из {args.Total}.");
        }
        #endregion



    }
}
