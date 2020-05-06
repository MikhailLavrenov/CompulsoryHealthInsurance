using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services.Report;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;

namespace CHI.ViewModels
{
    class PlanningViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        bool isGrowing;
        IMainRegionService mainRegionService;
        IFileDialogService fileDialogService;
        ReportService report;

        public bool KeepAlive { get => false; }
        public int Year { get => year; set => SetProperty(ref year, value); }
        public int Month { get => month; set => SetProperty(ref month, value); }
        public bool IsGrowing { get => isGrowing; set => SetProperty(ref isGrowing, value); }
        public Dictionary<int, string> Months { get; } = Enumerable.Range(1, 12).ToDictionary(x => x, x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x));
        public ReportService Report { get => report; set => SetProperty(ref report, value); }

        public DelegateCommandAsync BuildReportCommand { get; }
        public DelegateCommandAsync SaveExcelCommand { get; }
        public DelegateCommandAsync UpdateCalculatedCellsCommand { get; }
        public DelegateCommandAsync SavePlanCommand { get; }

        public PlanningViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;

            mainRegionService.Header = "Планирование объемов";

            BuildReportCommand = new DelegateCommandAsync(BuildReportExecute);
            SaveExcelCommand = new DelegateCommandAsync(SaveExcelExecute);
            SavePlanCommand = new DelegateCommandAsync(SavePlanExecute);
            UpdateCalculatedCellsCommand = new DelegateCommandAsync(() => report.UpdateCalculatedCells());
        }


        private void BuildReportExecute()
        {
            var registers = dbContext.Registers
                .Where(x => x.Year == Year && x.Month == Month)
                .Include(x => x.Cases).ThenInclude(x => x.Services).ThenInclude(x => x.ClassifierItem)
                .ToList();

            var plans = dbContext.Plans.Where(x => x.Year == Year && x.Month == Month).ToList();

            Report.Build(registers, plans, Month, Month, Year);
        }

        private void SaveExcelExecute()
        {
            mainRegionService.ShowProgressBar("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Отчет по выполнению объемов";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var filePath = fileDialogService.FileName;

            mainRegionService.ShowProgressBar("Сохранение файла");

            Report.SaveExcel(filePath);

            mainRegionService.HideProgressBar($"Файл сохранен: {filePath}");
        }

        private void SavePlanExecute()
        {
            report.UpdateCalculatedCells();

            var plans = dbContext.Plans.Local.Where(x => x.Month == Month && x.Year == Year).ToLookup(x => (x.Parameter.Id, x.Indicator.Id));

            foreach (var valueItem in report.ValuesList.Where(x => x.IsWritable))
            {
                var planItem = plans.FirstOrDefault(x => x.Key == (valueItem.RowHeader.Parameter.Id, valueItem.ColumnHeader.Indicator.Id))?.FirstOrDefault();

                if (planItem == null && valueItem.Value != null)
                {
                    planItem = new Plan()
                    {
                        Month = Month,
                        Year = Year,
                        Indicator = valueItem.ColumnHeader.Indicator,
                        Parameter = valueItem.RowHeader.Parameter,
                        Value = valueItem.Value.Value
                    };

                    dbContext.Add(planItem);
                }
                else if (planItem != null)
                {
                    if (valueItem.Value == null)
                        dbContext.Remove(planItem);
                    else
                        planItem.Value = valueItem.Value.Value;
                }
            }

            dbContext.SaveChanges();
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
            dbContext = new ServiceAccountingDBContext();

            dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            dbContext.Departments.Load();

            var currentSid = UserPrincipal.Current.Sid.ToString();
            var user = dbContext.Users.Where(x => x.Sid.Equals(currentSid)).Include(x => x.PlanningPermisions).FirstOrDefault();

            if (user == null || user.PlanningPermisions.Count == 0)
            {
                mainRegionService.RequestNavigateHome();
                return;
            }

            var rootDepartment = new Department();
            rootDepartment.IsRoot = true;
            rootDepartment.Childs = user.PlanningPermisions.Select(x => x.Department).ToList();
            rootDepartment.Childs.ForEach(x => x.Parent = rootDepartment);

            dbContext.Parameters.Where(x => x.Kind == ParameterKind.EmployeePlan || x.Kind == ParameterKind.DepartmentHandPlan).Load();


            dbContext.Employees
                .Include(x => x.Medic)
                .Include(x => x.Specialty)
                .Load();

            dbContext.Components
                .Include(x => x.Indicators).ThenInclude(x => x.Ratios)
                .Include(x => x.CaseFilters)
                .Load();


            var rootComponent = dbContext.Components.Local.First(x => x.IsRoot);

            Report = new ReportService(rootDepartment, rootComponent, true);

            dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //dbContext.SaveChanges();
        }
    }
}
;