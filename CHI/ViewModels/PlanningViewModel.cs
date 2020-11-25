using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Services.Report;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace CHI.ViewModels
{
    class PlanningViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        bool isGrowing;
        List<HeaderItem> rowHeaders;
        List<HeaderItem> columnHeaders;
        GridItem[][] gridItems;
        Dictionary<GridItem, (Parameter, Indicator)> gridItemDataComparator;
        IMainRegionService mainRegionService;
        IFileDialogService fileDialogService;
        ReportService reportService;
        Settings settings ;
        User currentUser;

        public bool KeepAlive { get => false; }
        public int Year
        {
            get => year;
            set
            {
                if (year == value)
                    return;

                BringChangesToDbContext();

                SetProperty(ref year, value);

                BuildReport();
            }
        }
        public int Month
        {
            get => month;
            set
            {
                if (month == value)
                    return;

                BringChangesToDbContext();

                SetProperty(ref month, value);

                BuildReport();
            }
        }
        public bool IsGrowing { get => isGrowing; set => SetProperty(ref isGrowing, value); }
        public Dictionary<int, string> Months { get; } = Enumerable.Range(1, 12).ToDictionary(x => x, x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x));
        public List<HeaderItem> RowHeaders { get => rowHeaders; set => SetProperty(ref rowHeaders, value); }
        public List<HeaderItem> ColumnHeaders { get => columnHeaders; set => SetProperty(ref columnHeaders, value); }
        public GridItem[][] GridItems { get => gridItems; set => SetProperty(ref gridItems, value); }

        public DelegateCommand IncreaseYear { get; }
        public DelegateCommand DecreaseYear { get; }
        public DelegateCommandAsync SaveExcelCommand { get; }
        public DelegateCommandAsync UpdateCalculatedCellsCommand { get; }


        public PlanningViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, User currentUser)
        {
            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;
            settings = Settings.Instance;
            this.currentUser = currentUser;

            mainRegionService.Header = "Планирование объемов";

            dbContext = new AppDBContext();

            IncreaseYear = new DelegateCommand(() => ++Year);
            DecreaseYear = new DelegateCommand(() => --Year);
            SaveExcelCommand = new DelegateCommandAsync(SaveExcelExecute);
            UpdateCalculatedCellsCommand = new DelegateCommandAsync(UpdateCalculatedCellsExecute);
        }

        private void UpdateCalculatedCellsExecute()
        {
            mainRegionService.ShowProgressBar("Обновление");

            BringChangesToDbContext();

            SetGridValues();

            mainRegionService.HideProgressBar("Обновлено");
        }

        private void BuildReport()
        {
            mainRegionService.ShowProgressBar("Построение плана");

            SetGridValues();

            SetHiddenRows();

            ReportHelper.SetAlternationColor(RowHeaders);

            mainRegionService.HideProgressBar("План построен");
        }

        private void SetGridValues()
        {
            if (!dbContext.Plans.Local.Where(x => x.Year == Year && x.Month == Month).Any())
                dbContext.Plans.Where(x => x.Year == Year && x.Month == Month).Load();

            var plans = dbContext.Plans.Local.Where(x => x.Year == Year && x.Month == Month).ToList();

            reportService.Build(null, plans, Month, Year);

            foreach (var item in gridItemDataComparator)
                item.Key.Value = reportService.Results[item.Value];
        }

        private void SetHiddenRows()
        {
            var employeeHeaderGroups = GridItems
                .SelectMany(x => x)    
                .Where(x => !x.RowSubHeader.HeaderItem.Childs.Any())   
                .GroupBy(x => x.RowSubHeader.HeaderItem);

            foreach (var group in employeeHeaderGroups)
            {
                group.Key.AlwaysHidden = !group.Where(x => x.Value.HasValue).Any();

                var rowHeader = group.Key;
                var rowGridItems = group.ToList();
                var employee = rowGridItems.Any() ? gridItemDataComparator[rowGridItems.First()].Item1.Employee : null;

                if (employee == null)
                    continue;

                rowHeader.AlwaysHidden = !rowGridItems.Where(x => x.Value.HasValue && x.Value!=0).Any() && employee.IsArchive;
            }
        }

        private void SaveExcelExecute()
        {
            mainRegionService.ShowProgressBar("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Планирование объемов";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.HideProgressBar("Отменено");
                return;
            }

            var filePath = fileDialogService.FileName;

            if (File.Exists(filePath) && Helpers.IsFileLocked(filePath))
            {
                mainRegionService.HideProgressBar("Отменено. Файл занят другим пользователем, поэтому не может быть изменен");
                return;
            }

            mainRegionService.ShowProgressBar("Сохранение файла");

            new ReportExcelBuilder(filePath)
                .UsePlaningStyle(settings.ApprovedBy)
                .SetNewSheet(Month,Year)
                .FillSheet(RowHeaders,ColumnHeaders,GridItems)
                .SaveAndClose();

            mainRegionService.HideProgressBar($"Файл сохранен: {filePath}");
        }

        private void BringChangesToDbContext()
        {
            var plans = dbContext.Plans.Local.Where(x => x.Month == Month && x.Year == Year).ToLookup(x => (x.Parameter, x.Indicator));

            foreach (var comparatorItem in gridItemDataComparator.Where(x => x.Key.IsEditable))
            {
                var gridItem = comparatorItem.Key;

                var storedPlanItem = plans.FirstOrDefault(x => x.Key == comparatorItem.Value)?.FirstOrDefault();

                if (gridItem.Value.HasValue && gridItem.Value != 0)
                {
                    if (storedPlanItem == null)
                        dbContext.Add(new Plan()
                        {
                            Month = Month,
                            Year = Year,
                            Parameter = comparatorItem.Value.Item1,
                            Indicator = comparatorItem.Value.Item2,
                            Value = gridItem.Value.Value
                        });

                    else
                        storedPlanItem.Value = gridItem.Value.Value;

                }
                else if (storedPlanItem != null)
                    dbContext.Remove(storedPlanItem);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            dbContext.Parameters.Where(x => x.Kind == ParameterKind.EmployeePlan || x.Kind == ParameterKind.DepartmentHandPlan || x.Kind == ParameterKind.DepartmentCalculatedPlan).Load();

            dbContext.Employees
                .Include(x => x.Medic)
                .Include(x => x.Specialty)
                .Load();

            dbContext.Components
                .Include(x => x.Indicators).ThenInclude(x => x.Ratios)
                .Include(x => x.CaseFilters)
                .Load();

            var user = dbContext.Users
                .Where(x => x.Id == currentUser.Id)
                .Include(x => x.PlanningPermisions).ThenInclude(x => x.Department)
                .FirstOrDefault();

            dbContext.Departments.Local.ToList().ForEach(x => x.Childs = x.Childs.OrderBy(x => x.Order).ToList());
            dbContext.Departments.Local.ToList().ForEach(x => x.Employees = x.Employees.OrderBy(x => x.Order).ToList());
            dbContext.Departments.Local.ToList().ForEach(x => x.Parameters = x.Parameters.OrderBy(x => x.Order).ToList());
            dbContext.Employees.Local.ToList().ForEach(x => x.Parameters = x.Parameters.OrderBy(x => x.Order).ToList());
            dbContext.Components.Local.ToList().ForEach(x => x.Childs = x.Childs?.OrderBy(x => x.Order).ToList());
            dbContext.Components.Local.ToList().ForEach(x => x.Indicators = x.Indicators.OrderBy(x => x.Order).ToList());

            var permittedDepartments = user.PlanningPermisions.Select(x => x.Department).ToList();

            var rootDepartment = new Department();
            rootDepartment.IsRoot = true;
            foreach (var permittedDepartment in permittedDepartments.Where(x => !permittedDepartments.Contains(x.Parent)))
            {
                rootDepartment.Childs.Add(permittedDepartment);
                permittedDepartment.Parent = rootDepartment;
            }

            var rootComponent = dbContext.Components.Local.First(x => x.IsRoot);

            reportService = new ReportService(rootDepartment, rootComponent);

            RowHeaders = ReportHelper.CreateHeaderItemRecursive(rootDepartment, null).ToListRecursive().Skip(1).ToList();
            ColumnHeaders = ReportHelper.CreateHeaderItemRecursive(rootComponent, null).ToListRecursive().Skip(1).ToList();

            var rowSubHeaders = RowHeaders.SelectMany(x => x.SubItems).ToList();
            var columnSubHeaders = ColumnHeaders.SelectMany(x => x.SubItems).ToList();

            GridItems = new GridItem[rowSubHeaders.Count][];

            for (int row = 0; row < rowSubHeaders.Count; row++)
            {
                GridItems[row] = new GridItem[columnSubHeaders.Count];

                for (int col = 0; col < columnSubHeaders.Count; col++)
                    GridItems[row][col] = new GridItem(rowSubHeaders[row], columnSubHeaders[col]);
            }

            RowHeaders.ForEach(x => x.SwitchCollapseCommand.Execute());
            ColumnHeaders.ForEach(x => x.SwitchCollapseCommand.Execute());

            var parameters = rootDepartment.ToListRecursive().Skip(1).SelectMany(x => x.Parameters.Concat(x.Employees.SelectMany(y => y.Parameters))).ToList();
            var indicators = rootComponent.ToListRecursive().Skip(1).SelectMany(x => x.Indicators).ToList();

            gridItemDataComparator = new Dictionary<GridItem, (Parameter, Indicator)>();

            for (int row = 0; row < parameters.Count; row++)
                for (int col = 0; col < indicators.Count; col++)
                {
                    var gridItem = GridItems[row][col];
                    var parameter = parameters[row];
                    var indicator = indicators[col];

                    gridItem.IsEditable = indicator.Component.IsCanPlanning
                        && (parameter.Kind == ParameterKind.EmployeePlan || parameter.Kind == ParameterKind.DepartmentHandPlan)
                        && !(parameter.Department?.Childs.Any() ?? false);

                    gridItemDataComparator.Add(gridItem, (parameter, indicator));
                }

            BuildReport();            
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            BringChangesToDbContext();

            dbContext.ChangeTracker
                .Entries()
                .Where(x => x.State != EntityState.Unchanged && x.Entity.GetType() != typeof(Plan))
                .ToList()
                .ForEach(x => dbContext.Entry(x.Entity).State = EntityState.Detached);

            dbContext.SaveChanges();
        }
    }
}