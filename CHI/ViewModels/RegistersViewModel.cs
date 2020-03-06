using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services.BillsRegister;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            mainRegionService.SetBusyStatus("Сопоставление штатных единиц");

            var dbContext = new ServiceAccountingDBContext();
            dbContext.Employees.Load();
            dbContext.Medics.Load();
            dbContext.Specialties.Load();
            dbContext.Departments.Load();

            var employees = dbContext.Employees.ToList();
            var medics = dbContext.Medics.ToList();
            var specialties = dbContext.Specialties.ToList();
            var departments = dbContext.Departments.ToList();

            var unknownDepartment = dbContext.Departments.AsEnumerable().FirstOrDefault(x => string.Equals(x.Title, Department.UnknownTitle, StringComparison.OrdinalIgnoreCase));

            if (unknownDepartment == null)
            {
                unknownDepartment = new Department(Department.UnknownTitle);
                dbContext.Departments.Add(unknownDepartment);
            }

            int j=0;
            Parallel.ForEach(register.Cases, (Case mCase) =>
            //Parallel.For(0, register.Cases.Count
            //for (int i = 0; i < register.Cases.Count; i++)
            {
                Interlocked.Increment(ref j);
                mainRegionService.SetBusyStatus($"Сопоставление штатных единиц: {j} из {register.Cases.Count}");

                mCase.Employee = FindEmployeeInDbOrAdd(mCase.Employee, dbContext, unknownDepartment);

                foreach (var service in mCase.Services)
                    service.Employee = FindEmployeeInDbOrAdd(service.Employee, dbContext, unknownDepartment);
            }
            );
            //foreach (var mCase in register.Cases)
            //{
            //    mCase.Employee = FindEmployeeInDbOrAdd(mCase.Employee, dbContext, unknownDepartment);

            //    foreach (var service in mCase.Services)
            //        service.Employee = FindEmployeeInDbOrAdd(service.Employee, dbContext, unknownDepartment);
            //}

            mainRegionService.SetBusyStatus("Сохранение в базу данных");

            dbContext.Registers.Add(register);
            dbContext.SaveChanges();

            mainRegionService.SetCompleteStatus("Успешно загружено");
        }

        private static Employee FindEmployeeInDbOrAdd(Employee employee, ServiceAccountingDBContext dbContext, Department unknownDepartment)
        {
            var foundMedic = dbContext.Medics.AsEnumerable().FirstOrDefault(x => string.Equals(x.FomsId, employee.Medic.FomsId, StringComparison.OrdinalIgnoreCase));

            if (foundMedic != null)
                employee.Medic = foundMedic;
            else
                dbContext.Medics.Add(employee.Medic);

            var foundSpecialty = dbContext.Specialties.AsEnumerable().FirstOrDefault(x => x.FomsId == employee.Specialty.FomsId);

            if (foundSpecialty != null)
                employee.Specialty = foundSpecialty;
            else
                dbContext.Specialties.Add(employee.Specialty);

            var foundEmployee = dbContext.Employees.AsEnumerable().FirstOrDefault(x => x.Medic == employee.Medic && x.Specialty == employee.Specialty);

            if (foundEmployee != null)
                employee = foundEmployee;
            else
            {
                employee.Department = unknownDepartment;

                dbContext.Employees.Add(employee);
            }

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
