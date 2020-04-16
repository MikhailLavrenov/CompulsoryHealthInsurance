using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
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
        ServiceAccountingDBContext dbContext;
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

            LoadRegisterCommand = new DelegateCommandAsync(LoadExecute);
            LoadPaymentStateCommand = new DelegateCommandAsync(LoadPaymentStateExecute, () => CurrentRegister != null).ObservesProperty(() => CurrentRegister);
        }


        private void LoadExecute()
        {
            mainRegionService.SetBusyStatus("Выбор файлов");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Archive files (*.zip)|*.zip|Xml files (*.xml)|*.xml";
            fileDialogService.MiltiSelect = true;

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.SetCompleteStatus("Отменено");
                return;
            }

            mainRegionService.SetBusyStatus("Загрузка xml-реестров");

            var registerService = new BillsRegisterService(fileDialogService.FileNames);
            registerService.FileNamesNotStartsWith = new string[] { "L" };
            var register = registerService.GetRegister(false);

            using var localDbContext = new ServiceAccountingDBContext();

            var registerForSamePeriod = localDbContext.Registers.FirstOrDefault(x => x.Month == register.Month && x.Year == register.Year);

            if (registerForSamePeriod != null)
                localDbContext.Registers.Remove(registerForSamePeriod);

            mainRegionService.SetBusyStatus("Сопоставление штатных единиц");

            localDbContext.Employees.Load();
            localDbContext.Medics.Load();
            localDbContext.Specialties.Load();
            localDbContext.Departments.Load();

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
                        var classifierId = dbContext.ServiceClassifiers
                            .ToList()
                            .Where(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, register.Month, register.Year))
                            .FirstOrDefault()?.Id;

                        caseClosingCodes = classifierId == null ?
                                new List<int>()
                                :
                                dbContext.ServiceClassifiers
                                .Where(x => x.Id == classifierId)
                                .Include(x => x.ServiceClassifierItems)
                                .First()
                                .ServiceClassifierItems
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


            var casesGroups = register.Cases.GroupBy(x => new { MedicFomsId = x.Employee.Medic.FomsId, SpecialtyFomsId = x.Employee.Specialty.FomsId }).ToList();

            var progressCounter = 0;

            foreach (var casesGroup in casesGroups)
            {
                var employee = FindEmployeeInDbOrAdd(casesGroup.Key.MedicFomsId, casesGroup.Key.SpecialtyFomsId, localDbContext, defaultDepartment);

                foreach (var mCase in casesGroup)
                {
                    mainRegionService.SetBusyStatus($"Сопоставление штатных единиц: {++progressCounter} из {register.Cases.Count}");

                    mCase.Employee = employee;

                    foreach (var service in mCase.Services)
                    {
                        if (mCase.Employee.Specialty.FomsId == service.Employee.Specialty.FomsId && mCase.Employee.Medic.FomsId.Equals(service.Employee.Medic.FomsId))
                            service.Employee = mCase.Employee;
                        else
                            service.Employee = FindEmployeeInDbOrAdd(service.Employee.Medic.FomsId, service.Employee.Specialty.FomsId, localDbContext, defaultDepartment);
                    }
                }
            }

            mainRegionService.SetBusyStatus("Сохранение в базу данных");

            localDbContext.Registers.Add(register);
            localDbContext.SaveChanges();

            Application.Current.Dispatcher.Invoke(() => Refresh());

            mainRegionService.SetCompleteStatus("Успешно загружено");
        }

        private static Employee FindEmployeeInDbOrAdd(string medicFomsId, int specialtyFomsId, ServiceAccountingDBContext dbContext, Department defaultDepartment)
        {
            var employee = dbContext.Employees.Local.FirstOrDefault(x => x.Specialty.FomsId == specialtyFomsId && string.Equals(x.Medic.FomsId, medicFomsId, StringComparison.Ordinal));

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
            mainRegionService.SetBusyStatus("Выбор файлов");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Archive files (*.zip)|*.zip|Xml files (*.xml)|*.xml";
            fileDialogService.MiltiSelect = true;

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.SetCompleteStatus("Отменено");
                return;
            }

            mainRegionService.SetBusyStatus("Загрузка xml-реестров");

            var registerService = new BillsRegisterService(fileDialogService.FileNames);
            registerService.FileNamesNotStartsWith = new string[] { "L" };
            var paidRegister = registerService.GetRegister(true);

            var dbContext = new ServiceAccountingDBContext();

            var register = dbContext.Registers.FirstOrDefault(x => x.Month == paidRegister.Month && x.Year == paidRegister.Year);

            if (register == null)
                mainRegionService.SetCompleteStatus("Период загружаемого реестра не соотвествует выбранному");

            mainRegionService.SetBusyStatus("Установка статуса оплаты");

            var casePairs = register.Cases.Join(paidRegister.Cases, mcase => mcase.IdCase, paidCase => paidCase.IdCase, (mcase, paidCase) => new { mcase, paidCase }).ToList();

            foreach (var casePair in casePairs)
            {
                casePair.mcase.PaidStatus = casePair.paidCase.PaidStatus;
                casePair.mcase.AmountPaid = casePair.paidCase.AmountPaid;
                casePair.mcase.AmountUnpaid = casePair.paidCase.AmountUnpaid;
            }

            register.PaymentStateCasesCount = register.Cases.Count(x => x.PaidStatus != PaidKind.None);

            dbContext.SaveChanges();

            mainRegionService.SetCompleteStatus("Загрузка статуса оплаты завершена");
        }

        private void Refresh()
        {
            dbContext = new ServiceAccountingDBContext();
            dbContext.Registers.Load();
            Registers = dbContext.Registers.Local.ToObservableCollection();
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
