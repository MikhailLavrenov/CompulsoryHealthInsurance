using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CHI.ViewModels
{
    public class UserDepartmentsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        IMainRegionService mainRegionService;
        User currentUser;


        public bool KeepAlive { get => false; }
        public List<dynamic> Departments { get; set; }


        public UserDepartmentsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Отделения пользователя";

            dbContext = new ServiceAccountingDBContext();

            dbContext.Departments.Load();
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(User)))
            {
                currentUser = navigationContext.Parameters.GetValue<User>(nameof(User));

                currentUser=dbContext.Users.Where(x=>x.Id==currentUser.Id).Include(x => x.UserDepartments).First();

                Departments = dbContext.Departments.Local.Select(x => new { Department = x, Selected = currentUser.UserDepartments.Any(y => y.DepartmentId == x.Id) }).ToList<dynamic>();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var selectedDepartments = Departments.Where(x => x.Selected == true).Select(x => x.Department).Cast<Department>().ToList();

            var removeDepartments=currentUser.UserDepartments.Where(x => !selectedDepartments.Any(y => y.Id == x.DepartmentId)).ToList();
            var addDepartments = selectedDepartments
                .Where(x => !currentUser.UserDepartments.Any(y => y.DepartmentId == x.Id))
                .Select(x => new UserDepartment( currentUser, x))
                .ToList();

            dbContext.RemoveRange(removeDepartments);
            dbContext.AddRange(addDepartments);

            dbContext.SaveChanges();
        }

    }
}
