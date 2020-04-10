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

            dbContext = new ServiceAccountingDBContext();
            dbContext.Registers.Load();
            Registers = dbContext.Registers.Local.ToObservableCollection();

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

            var dbContext = new ServiceAccountingDBContext();

            var registerForSamePeriod = dbContext.Registers.FirstOrDefault(x => x.Month == register.Month && x.Year == register.Year);

            if (registerForSamePeriod != null)
            {
                dbContext.Remove(registerForSamePeriod);
                dbContext.SaveChanges();
            }

            mainRegionService.SetBusyStatus("Сопоставление штатных единиц");

            dbContext.Employees.Load();
            dbContext.Medics.Load();
            dbContext.Specialties.Load();
            dbContext.Departments.Load();

            var defaultDepartment = dbContext.Departments.Local.First(x => x.IsRoot);

            for (int i = 0; i < register.Cases.Count; i++)
            {
                var mCase = register.Cases[i];
                mainRegionService.SetBusyStatus($"Сопоставление штатных единиц: {i} из {register.Cases.Count}");

                //в некоторых реестрах не указан врач закрывший случай
                if (string.IsNullOrEmpty(mCase.Employee.Medic.FomsId))
                {
                    var maxDate = mCase.Services.Select(x => x.Date).Max();
                    var medicFomsCodes=mCase.Services.Where(x =>x.Date== maxDate && x.Employee.Specialty.FomsId == mCase.Employee.Specialty.FomsId).Select(x => x.Employee.Medic.FomsId).Distinct();

                    if (medicFomsCodes.Count() == 1)
                        mCase.Employee.Medic.FomsId = medicFomsCodes.First();
                    else
                        throw new InvalidOperationException("Не удается однозначно определить мед. работника закрывшего случай");
                }

                mCase.Employee = FindEmployeeInDbOrAdd(mCase.Employee, dbContext, defaultDepartment);

                foreach (var service in mCase.Services)
                {
                    if (mCase.Employee.Specialty.FomsId == service.Employee.Specialty.FomsId && mCase.Employee.Medic.FomsId.Equals(service.Employee.Medic.FomsId))
                        service.Employee = mCase.Employee;
                    else
                        service.Employee = FindEmployeeInDbOrAdd(service.Employee, dbContext, defaultDepartment);
                }
            }

            mainRegionService.SetBusyStatus("Сохранение в базу данных");

            dbContext.Registers.Add(register);
            dbContext.SaveChanges();

            Application.Current.Dispatcher.Invoke(() => Registers = dbContext.Registers.Local.ToObservableCollection());

            mainRegionService.SetCompleteStatus("Успешно загружено");
        }

        private static Employee FindEmployeeInDbOrAdd(Employee employee, ServiceAccountingDBContext dbContext, Department defaultDepartment)
        {
            var foundEmployee = dbContext.Employees.Local.FirstOrDefault(x => string.Equals(x.Medic.FomsId, employee.Medic.FomsId, StringComparison.Ordinal) && x.Specialty.FomsId == employee.Specialty.FomsId);

            if (foundEmployee != null)
                return foundEmployee;

            employee.Department = defaultDepartment;

            var foundMedic = dbContext.Medics.Local.FirstOrDefault(x => string.Equals(x.FomsId, employee.Medic.FomsId, StringComparison.Ordinal));

            if (foundMedic != null)
                employee.Medic = foundMedic;
            else
                dbContext.Medics.Add(employee.Medic);

            var foundSpecialty = dbContext.Specialties.Local.FirstOrDefault(x => x.FomsId == employee.Specialty.FomsId);

            if (foundSpecialty != null)
                employee.Specialty = foundSpecialty;
            else
                dbContext.Specialties.Add(employee.Specialty);

            employee.Parameters = new List<Parameter>
            {
                new Parameter (0,ParameterKind.EmployeePlan),
                new Parameter (1,ParameterKind.EmployeeFact),
                new Parameter (2,ParameterKind.EmployeeRejectedFact),
                new Parameter (3,ParameterKind.EmployeePercent),
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

            var casePairs=register.Cases.Join(paidRegister.Cases, mcase => mcase.IdCase, paidCase => paidCase.IdCase, (mcase, paidCase) => new { mcase, paidCase }).ToList();

            foreach (var casePair in casePairs)
            {
                casePair.mcase.PaidStatus = casePair.paidCase.PaidStatus;
                casePair.mcase.AmountPaid = casePair.paidCase.AmountPaid;
                casePair.mcase.AmountUnpaid = casePair.paidCase.AmountUnpaid;
            }

            dbContext.SaveChanges();

            mainRegionService.SetCompleteStatus("Загрузка статуса оплаты завершена");
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
