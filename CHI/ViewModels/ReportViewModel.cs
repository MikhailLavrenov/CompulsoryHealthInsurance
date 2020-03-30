using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CHI.ViewModels
{
    class ReportViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        bool isGrowing;
        Report report;

        public bool KeepAlive { get => false; }
        public int Year { get => year; set => SetProperty(ref year, value); }
        public int Month { get => month; set => SetProperty(ref month, value); }
        public bool IsGrowing { get => isGrowing; set => SetProperty(ref isGrowing, value); }
        public Dictionary<int, string> Months { get; } = Enumerable.Range(1, 12).ToDictionary(x => x, x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x));

        public DelegateCommandAsync BuildReportCommand { get; }

        public ReportViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Отчет";

            dbContext = new ServiceAccountingDBContext();

            dbContext.Employees
                .Include(x => x.Medic)
                .Include(x => x.Specialty)
                .Include(x => x.Parameters)
                .Load();

            dbContext.Departments.Include(x => x.Parameters).Load();

            dbContext.Components
                .Include(x => x.Indicators)
                .Include(x=>x.CaseFilters)
                .Load();

            var rootDepartment = dbContext.Departments.Local.First(x => x.IsRoot);
            var rootComponent = dbContext.Components.Local.First(x => x.IsRoot);

            report = new Report(rootDepartment, rootComponent);

            BuildReportCommand = new DelegateCommandAsync(BuildReportExecute);
        }

        private void BuildReportExecute()
        {
            var cases = new List<Case>();

            if (IsGrowing)
                dbContext.Registers
                    .Where(x => x.Year == Year && x.Month <= Month)
                    .Include(x => x.Cases).ThenInclude(x => x.Services).ToList().ForEach(x => cases.AddRange(x.Cases));
            else
                dbContext.Registers
                    .Where(x => x.Year == Year && x.Month == Month)
                    .Include(x => x.Cases).ThenInclude(x => x.Services).ToList().ForEach(x => cases.AddRange(x.Cases));

            report.Build(cases);
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
