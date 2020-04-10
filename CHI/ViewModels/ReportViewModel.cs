using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services.Report;
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


        public bool KeepAlive { get => false; }
        public int Year { get => year; set => SetProperty(ref year, value); }
        public int Month { get => month; set => SetProperty(ref month, value); }
        public bool IsGrowing { get => isGrowing; set => SetProperty(ref isGrowing, value); }
        public Dictionary<int, string> Months { get; } = Enumerable.Range(1, 12).ToDictionary(x => x, x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x));
        public ReportService Report { get; set; }

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
                .Include(x => x.Indicators).ThenInclude(x => x.Ratios)
                .Include(x => x.CaseFilters)
                .Load();

            var rootDepartment = dbContext.Departments.Local.First(x => x.IsRoot);
            var rootComponent = dbContext.Components.Local.First(x => x.IsRoot);

            Report = new ReportService(rootDepartment, rootComponent);

            BuildReportCommand = new DelegateCommandAsync(BuildReportExecute);
        }

        private void BuildReportExecute()
        {
            var month1 = IsGrowing ? 1 : Month;
            var month2 = Month;

            var registers = dbContext.Registers
                .Where(x => x.Year == Year && month1 <= x.Month && x.Month >= month2)
                .Include(x => x.Cases).ThenInclude(x => x.Services)
                .ToList();

            var plans = dbContext.Plans.Where(x => x.Year == Year && month1 <= x.Month && x.Month >= month2 ).ToList();

            var classifiers = dbContext.ServiceClassifiers.ToList()
                .AsEnumerable()                     
                .Where(x => PeriodsIntersects(x.ValidFrom, x.ValidTo, month1, month2, Year))
                .ToList()
                .AsQueryable()
                .Include(x => x.ServiceClassifierItems)                  
                .ToList();

            Report.Build(registers, plans, classifiers, month1, month2, Year);
        }

        public static bool PeriodsIntersects(DateTime? period1date1, DateTime? period1date2, int period2Month1, int period2Month2, int period2Year)
        {
            var p1d1 = period1date1.HasValue ? period1date1.Value.Year * 100 + period1date1.Value.Month : 0;
            var p1d2 = period1date2.HasValue ? period1date2.Value.Year * 100 + period1date2.Value.Month : int.MaxValue;
            var p2d1 = period2Year * 100 + period2Month1;
            var p2d2 = period2Year * 100 + period2Month2;

            return !(p2d2 < p1d1 || p1d2 < p2d1);
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
