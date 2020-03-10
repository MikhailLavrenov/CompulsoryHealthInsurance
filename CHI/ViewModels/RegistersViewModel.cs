using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services.BillsRegister;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CHI.ViewModels
{
    public class RegistersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Register> registers;
        IFileDialogService fileDialogService;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Register> Registers { get => registers; set => SetProperty(ref registers, value); }

        public DelegateCommandAsync LoadCommand { get; }

        public RegistersViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Реестры";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Registers.Load();
            Registers = dbContext.Registers.Local.ToObservableCollection();

            LoadCommand = new DelegateCommandAsync(LoadExecute);
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
            var register = registerService.GetRegister();

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

            var unknownDepartment = dbContext.Departments.Local.FirstOrDefault(x => string.Equals(x.Title, Department.UnknownTitle, StringComparison.Ordinal));

            if (unknownDepartment == null)
            {
                unknownDepartment = new Department(Department.UnknownTitle);
                dbContext.Departments.Add(unknownDepartment);
            }

            for (int i = 0; i < register.Cases.Count; i++)
            {
                var mCase = register.Cases[i];
                mainRegionService.SetBusyStatus($"Сопоставление штатных единиц: {i} из {register.Cases.Count}");

                mCase.Employee = FindEmployeeInDbOrAdd(mCase.Employee, dbContext, unknownDepartment);

                foreach (var service in mCase.Services)
                    service.Employee = FindEmployeeInDbOrAdd(service.Employee, dbContext, unknownDepartment);
            }

            mainRegionService.SetBusyStatus("Сохранение в базу данных");

            dbContext.Registers.Add(register);
            dbContext.SaveChanges();

            Application.Current.Dispatcher.Invoke(() => Registers = dbContext.Registers.Local.ToObservableCollection());

            mainRegionService.SetCompleteStatus("Успешно загружено");
        }

        private static Employee FindEmployeeInDbOrAdd(Employee employee, ServiceAccountingDBContext dbContext, Department unknownDepartment)
        {
            var mid = employee.Medic.FomsId;
            var sid = employee.Specialty.FomsId;

            var foundEmployee = dbContext.Employees.Local.FirstOrDefault(x => string.Equals(x.Medic.FomsId, employee.Medic.FomsId, StringComparison.Ordinal) && x.Specialty.FomsId == employee.Specialty.FomsId);

            if (foundEmployee != null)
                return foundEmployee;

            employee.Department = unknownDepartment;

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

            

            dbContext.Employees.Add(employee);

            return employee;
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
