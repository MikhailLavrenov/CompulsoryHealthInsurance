using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Services.BillsRegister;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CHI.ViewModels
{
    public class RegistersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        Register currentRegister;
        ObservableCollection<Register> registers;
        IFileDialogService fileDialogService;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }
        public Register CurrentRegister { get => currentRegister; set => SetProperty(ref currentRegister, value); }
        public ObservableCollection<Register> Registers { get => registers; set => SetProperty(ref registers, value); }

        public DelegateCommandAsync LoadRegisterCommand { get; }
        public DelegateCommandAsync LoadPaymentStateCommand { get; }


        public RegistersViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Реестры";

            Refresh();

            LoadRegisterCommand = new DelegateCommandAsync(LoadRegisterExecute);
            LoadPaymentStateCommand = new DelegateCommandAsync(LoadPaymentStateExecute);
        }


        private void LoadRegisterExecute()
        {
            mainRegionService.ShowProgressBar("Выбор файлов");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Archive files (*.zip)|*.zip|Xml files (*.xml)|*.xml";
            fileDialogService.MiltiSelect = true;

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.HideProgressBar("Отменено");
                return;
            }

            mainRegionService.ShowProgressBar("Загрузка xml-реестров");

            var registerService = new BillsRegisterService(fileDialogService.FileNames);
            var register = registerService.GetRegister(false);

            using var localDbContext = new AppDBContext();

            var registerForSamePeriod = localDbContext.Registers.FirstOrDefault(x => x.Month == register.Month && x.Year == register.Year);

            if (registerForSamePeriod != null)
                localDbContext.Registers.Remove(registerForSamePeriod);

            mainRegionService.ShowProgressBar("Сопоставление штатных единиц");

            localDbContext.Employees.Include(x => x.Medic).Include(x => x.Specialty).Load();
            localDbContext.Departments.Load();

            var periods = register.Cases.GroupBy(x => new { x.DateEnd.Year, x.DateEnd.Month }).Select(x => x.Key).ToList();

            var classifierIds = localDbContext.ServiceClassifiers
                .AsNoTracking()
                .AsEnumerable()
                .Where(x => periods.Any(y => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, y.Month, y.Year)))
                .Select(x => x.Id)
                .ToList();

            localDbContext.ServiceClassifiers.Where(x => classifierIds.Contains(x.Id)).Include(x => x.ServiceClassifierItems).Load();

            var defaultDepartment = localDbContext.Departments.Local.First(x => x.IsRoot);

            //в некоторых реестрах не указан врач закрывший случай
            List<int> caseClosingCodes = null;

            foreach (var mCase in register.Cases.Where(x => string.IsNullOrEmpty(x.Employee.Medic.FomsId)))
            {
                var maxDate = mCase.Services.Select(x => x.Date).Max();
                var laterServices = mCase.Services.Where(x => x.Date == maxDate && x.Employee.Specialty.FomsId == mCase.Employee.Specialty.FomsId).ToList();
                var medicFomsIds = laterServices.Select(x => x.Employee.Medic.FomsId).Distinct().ToList();

                if (medicFomsIds.Count() == 1)
                    mCase.Employee.Medic.FomsId = medicFomsIds.First();
                else
                {
                    if (caseClosingCodes == null)
                    {
                        var classifier = localDbContext.ServiceClassifiers.Local
                            .Where(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, register.Month, register.Year))
                            .FirstOrDefault();

                        caseClosingCodes = classifier == null ?
                            new List<int>()
                            : classifier.ServiceClassifierItems
                            .Where(x => x.IsCaseClosing)
                            .Select(x => x.Code)
                            .ToList();
                    }

                    medicFomsIds = laterServices.Where(x => caseClosingCodes.Contains(x.Code)).Select(x => x.Employee.Medic.FomsId).Distinct().ToList();

                    if (medicFomsIds.Count() == 1)
                        mCase.Employee.Medic.FomsId = medicFomsIds.First();
                    else
                        throw new InvalidOperationException($"Не удается однозначно определить мед. работника закрывшего случай {mCase.IdCase}");
                }
            }

            //сопоставление штатных единиц в случаях и услугах
            var casesGroups = register.Cases.GroupBy(x => new { MedicFomsId = x.Employee.Medic.FomsId, SpecialtyFomsId = x.Employee.Specialty.FomsId, x.AgeKind }).ToList();

            var progressCounter = 0;

            foreach (var casesGroup in casesGroups)
            {
                var employee = FindEmployeeInDbOrCreateNew(casesGroup.Key.MedicFomsId, casesGroup.Key.SpecialtyFomsId, casesGroup.Key.AgeKind, localDbContext, defaultDepartment);

                foreach (var mCase in casesGroup)
                {
                    mainRegionService.ShowProgressBar($"Сопоставление штатных единиц: {++progressCounter} из {register.Cases.Count}");

                    mCase.Employee = employee;

                    foreach (var service in mCase.Services)
                    {
                        if (mCase.Employee.Specialty.FomsId == service.Employee.Specialty.FomsId && mCase.Employee.Medic.FomsId.Equals(service.Employee.Medic.FomsId))
                            service.Employee = mCase.Employee;
                        else
                            service.Employee = FindEmployeeInDbOrCreateNew(service.Employee.Medic.FomsId, service.Employee.Specialty.FomsId, casesGroup.Key.AgeKind, localDbContext, defaultDepartment);
                    }
                }
            }

            //сопоставление услуг с классификатором

            progressCounter = 0;

            var services = register.Cases.SelectMany(x => x.Services).ToList();

            foreach (var periodGroup in register.Cases.GroupBy(x => new { x.DateEnd.Year, x.DateEnd.Month }))
            {
                mainRegionService.ShowProgressBar($"Сопоставление услуг случаев с классификатором: {++progressCounter} из {register.Cases.Count}");

                var classifier = localDbContext.ServiceClassifiers.Local.FirstOrDefault(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, periodGroup.Key.Month, periodGroup.Key.Year));

                if (classifier == null)
                    throw new InvalidOperationException($"Не найден классификатор соответствующий {periodGroup.Key.Month} месяцу {periodGroup.Key.Year} году");

                var classifierItems = classifier.ServiceClassifierItems.ToLookup(x => x.Code);

                periodGroup.SelectMany(x => x.Services).ToList().ForEach(x => x.ClassifierItem = classifierItems[x.Code].FirstOrDefault());
            }

            mainRegionService.ShowProgressBar("Сохранение в базу данных");

            localDbContext.Registers.Add(register);
            localDbContext.SaveChanges();

            Application.Current.Dispatcher.Invoke(() => Refresh());

            mainRegionService.HideProgressBar("Успешно загружено");
        }

        private static Employee FindEmployeeInDbOrCreateNew(string medicFomsId, int specialtyFomsId, AgeKind ageKind, AppDBContext dbContext, Department defaultDepartment)
        {
            var employee = dbContext.Employees.Local.FirstOrDefault(x => x.Specialty.FomsId == specialtyFomsId && string.Equals(x.Medic.FomsId, medicFomsId, StringComparison.Ordinal) && (x.AgeKind == AgeKind.Any || x.AgeKind == ageKind));

            if (employee != null)
                return employee;

            var medic = dbContext.Medics.Local.FirstOrDefault(x => string.Equals(x.FomsId, medicFomsId, StringComparison.Ordinal));
            var specialty = dbContext.Specialties.Local.FirstOrDefault(x => x.FomsId == specialtyFomsId);

            employee = new Employee
            {
                Medic = medic == null ? Medic.CreateUnknown(medicFomsId) : medic,
                Specialty = specialty == null ? Specialty.CreateUnknown(specialtyFomsId) : specialty,
                Department = defaultDepartment,
                Parameters = new List<Parameter>
                {
                    new Parameter (0,ParameterKind.EmployeePlan),
                    new Parameter (1,ParameterKind.EmployeeFact),
                    new Parameter (2,ParameterKind.EmployeeRejectedFact),
                    new Parameter (3,ParameterKind.EmployeePercent),
                }
            };

            dbContext.Employees.Add(employee);

            return employee;
        }

        private void LoadPaymentStateExecute()
        {
            mainRegionService.ShowProgressBar("Выбор файлов");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Archive files (*.zip)|*.zip|Xml files (*.xml)|*.xml";
            fileDialogService.MiltiSelect = true;

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.HideProgressBar("Отменено");
                return;
            }

            mainRegionService.ShowProgressBar("Загрузка xml-реестров");

            var registerService = new BillsRegisterService(fileDialogService.FileNames);
            var paidRegister = registerService.GetRegister(true);

            var dbContext = new AppDBContext();

            var register = dbContext.Registers.Where(x => x.Month == paidRegister.Month && x.Year == paidRegister.Year).Include(x => x.Cases).FirstOrDefault();

            if (register == null)
                mainRegionService.HideProgressBar($"Загрузка статусов оплаты отменена. Сначала загрузите реестр за период {paidRegister.Month} месяц {paidRegister.Year} год.");

            mainRegionService.ShowProgressBar("Запись статусов оплаты");

            var casePairs = register.Cases.Join(paidRegister.Cases, mcase => mcase.IdCase, paidCase => paidCase.IdCase, (mcase, paidCase) => new { mcase, paidCase }).ToList();

            foreach (var casePair in casePairs)
            {
                casePair.mcase.PaidStatus = casePair.paidCase.PaidStatus;
                casePair.mcase.AmountPaid = casePair.paidCase.AmountPaid;
                casePair.mcase.AmountUnpaid = casePair.paidCase.AmountUnpaid;
            }

            register.PaymentStateCasesCount = register.Cases.Count(x => x.PaidStatus != PaidKind.None);

            dbContext.SaveChanges();

            Refresh();

            mainRegionService.HideProgressBar($"Загрузка статусов оплаты завершена. В файле(ах) {paidRegister.Cases.Count} случая, загружено {casePairs.Count()}.");
        }

        private void Refresh()
        {
            dbContext = new AppDBContext();
            dbContext.Registers.Load();
            Registers = new ObservableCollection<Register>(dbContext.Registers.Local.OrderByDescending(x => x.Month).OrderByDescending(x => x.Year));
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            dbContext.SaveChanges();
        }
    }
}
