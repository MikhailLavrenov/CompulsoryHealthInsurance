using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services.BillsRegister;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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

            var registerService = new BillsRegisterService(fileDialogService.FileNames);
            var register=registerService.GetRegister();

            var dbContext = new ServiceAccountingDBContext();
            dbContext.Employees.Load();
            dbContext.Medics.Load();
            dbContext.Specialties.Load();
            dbContext.Departments.Load();

            var comparer = StringComparison.OrdinalIgnoreCase;

            foreach (var mCase in register.Cases)
            {
                var foundMedic=dbContext.Medics.FirstOrDefault(x => string.Equals(x.FomsId, mCase.Employee.Medic.FomsId, comparer));

                if (foundMedic != null)
                    mCase.Employee.Medic = foundMedic;
                else
                    dbContext.Medics.Add(mCase.Employee.Medic);

                var foundSpecialty = dbContext.Specialties.FirstOrDefault(x => x.FomsId== mCase.Employee.Specialty.FomsId);

                if (foundSpecialty != null)
                    mCase.Employee.Specialty = foundSpecialty;
                else
                    dbContext.Specialties.Add(mCase.Employee.Specialty);

                var foundEmployee = dbContext.Employees.FirstOrDefault(x => x.Medic==mCase.Employee.Medic && x.Specialty==mCase.Employee.Specialty);

                if (foundEmployee != null)
                    mCase.Employee = foundEmployee;
                else
                    dbContext.Employees.Add(mCase.Employee);
            }



            dbContext.Registers.Add(register);
            dbContext.SaveChanges();

            mainRegionService.SetCompleteStatus("Успешно загружено");
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
