using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Settings;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;

namespace CHI.ViewModels
{
    public class PlanPermisionsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        private readonly AppSettings settings;
        IMainRegionService mainRegionService;
        User currentUser;


        public bool KeepAlive { get => false; }
        public List<SelectedObject<Department>> Departments { get; set; }


        public PlanPermisionsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Отделения пользователя";

            dbContext = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);

            Departments = dbContext.Departments.Where(x => !x.IsRoot).AsEnumerable().Select(x => new SelectedObject<Department>(false, x)).ToList();
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(User)))
            {
                currentUser = navigationContext.Parameters.GetValue<User>(nameof(User));

                currentUser = dbContext.Users.Where(x => x.Id == currentUser.Id).Include(x => x.PlanningPermisions).First();

                Departments.Where(x => currentUser.PlanningPermisions.Any(y => y.DepartmentId == x.Object.Id)).ToList().ForEach(x => x.IsSelected = true);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var selectedDepartments = Departments.Where(x => x.IsSelected).Select(x => x.Object).ToList();

            var removeDepartments = currentUser.PlanningPermisions.Where(x => !selectedDepartments.Any(y => y.Id == x.DepartmentId)).ToList();
            var addDepartments = selectedDepartments
                .Where(x => !currentUser.PlanningPermisions.Any(y => y.DepartmentId == x.Id))
                .Select(x => new PlanningPermision(currentUser, x))
                .ToList();

            dbContext.RemoveRange(removeDepartments);
            dbContext.AddRange(addDepartments);

            dbContext.SaveChanges();
        }

    }
}
