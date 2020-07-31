using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Views;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Linq;

namespace CHI.ViewModels
{
    public class NavigationMenuViewModel : DomainObject, IRegionMemberLifetime
    {
        IMainRegionService mainRegionService;
        User currentUser;

        public bool KeepAlive { get => false; }

        public DelegateCommand<object> SwitchViewCommand { get; }


        public NavigationMenuViewModel(IMainRegionService mainRegionService, User currentUser)
        {
            this.mainRegionService = mainRegionService;
            this.currentUser = currentUser;

            mainRegionService.Header = "Меню";

            SwitchViewCommand = new DelegateCommand<object>(SwitchViewExecute);
        }


        private void SwitchViewExecute(object view)
        {
            string name;

            if (view is Type)
                name = ((Type)view).Name;
            else if (view is string)
                name = (string)view;
            else
                name = string.Empty;

            var isPermitted = name switch
            {
                nameof(PlanningView) when currentUser.PlanningPermisions.Any() => true,
                nameof(ReportView) when currentUser.ReportPermision => true,
                nameof(RegistersView) when currentUser.RegistersPermision => true,
                nameof(MedicsView) when currentUser.ReferencesPerimision => true,
                nameof(SpecialtiesView) when currentUser.ReferencesPerimision => true,
                nameof(EmployeesView) when currentUser.ReferencesPerimision => true,
                nameof(DepartmentsView) when currentUser.ReferencesPerimision => true,
                nameof(ComponentsView) when currentUser.ReferencesPerimision => true,
                nameof(ServiceClassifiersView) when currentUser.ReferencesPerimision => true,
                nameof(AttachedPatientsView) when currentUser.AttachedPatientsPermision => true,
                nameof(ExaminationsView) when currentUser.MedicalExaminationsPermision => true,
                nameof(UsersView) when currentUser.UsersPerimision => true,
                nameof(CommonSettingsView) when currentUser.SettingsPermision => true,
                nameof(ServiceAccountingSettingsView) when currentUser.SettingsPermision => true,
                nameof(AttachedPatientsFileSettingsView) when currentUser.SettingsPermision => true,
                nameof(AttachedPatientsStorageSettingsView) when currentUser.SettingsPermision => true,
                nameof(SrzSettingsView) when currentUser.SettingsPermision => true,
                nameof(ExaminationsSettingsView) when currentUser.SettingsPermision => true,
                nameof(AboutView) => true,
                _ => false
            };

            if (!isPermitted)
                mainRegionService.Message = "Нет прав доступа.";
            else
                mainRegionService.RequestNavigate(name);
        }
    }
}
