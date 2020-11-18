using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Services.Report;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CHI.ViewModels
{
    class ReportViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        Settings settings;
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

        public bool KeepAlive { get => false; }
        public int Year { get => year; set => SetProperty(ref year, value); }
        public int Month { get => month; set => SetProperty(ref month, value); }
        public bool IsGrowing { get => isGrowing; set => SetProperty(ref isGrowing, value); }
        public Dictionary<int, string> Months { get; } = Enumerable.Range(1, 12).ToDictionary(x => x, x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x));

        public List<HeaderItem> RowHeaders { get => rowHeaders; set => SetProperty(ref rowHeaders, value); }
        public List<HeaderItem> ColumnHeaders { get => columnHeaders; set => SetProperty(ref columnHeaders, value); }
        public GridItem[][] GridItems { get => gridItems; set => SetProperty(ref gridItems, value); }
        public DelegateCommand IncreaseYear { get; }
        public DelegateCommand DecreaseYear { get; }
        public DelegateCommandAsync BuildReportCommand { get; }
        public DelegateCommandAsync SaveExcelCommand { get; }
        public DelegateCommandAsync BuildAndSaveExcelCommand { get; }


        public ReportViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            dbContext = new AppDBContext();

            settings = Settings.Instance;

            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;

            mainRegionService.Header = "Отчет по объемам";

            IncreaseYear = new DelegateCommand(() => ++Year);
            DecreaseYear = new DelegateCommand(() => --Year);
            BuildReportCommand = new DelegateCommandAsync(BuildReportExecute);
            SaveExcelCommand = new DelegateCommandAsync(SaveExcelExecute);
            BuildAndSaveExcelCommand = new DelegateCommandAsync(BuildAndSaveExcelExecute, () => !string.IsNullOrEmpty(settings.ServiceAccountingReportPath));
        }


        private void BuildReportExecute()
        {
            mainRegionService.ShowProgressBar("Построение отчета");

            BuildReportInternal(reportService, IsGrowing);

            mainRegionService.HideProgressBar($"Отчет за {Months[Month]} {Year} построен");
        }

        private void BuildReportInternal(ReportService report, bool isGrowing)
        {
            var monthBegin = isGrowing ? 1 : Month;

            var registers = dbContext.Registers
                .Where(x => x.Year == Year && monthBegin <= x.Month && x.Month <= Month)
                .Include(x => x.Cases).ThenInclude(x => x.Services).ThenInclude(x => x.ClassifierItem)
                .ToList();

            var plans = dbContext.Plans.Where(x => x.Year == Year && monthBegin <= x.Month && x.Month <= Month).ToList();

            report.Build(registers, plans, Month, Year, isGrowing);

            foreach (var item in gridItemDataComparator)
                item.Key.Value = report.Results[item.Value];            
        }

        private void SaveExcelExecute()
        {
            mainRegionService.ShowProgressBar("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Отчет по выполнению объемов";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.HideProgressBar("Отменено");
                return;
            }

            var filePath = fileDialogService.FileName;

            if (Helpers.IsFileLocked(filePath))
            {
                mainRegionService.HideProgressBar("Отменено. Файл занят другим пользователем, поэтому не может быть изменен");
                return;
            }

            mainRegionService.ShowProgressBar("Сохранение файла");

            //Report.SaveExcel(filePath);

            mainRegionService.HideProgressBar($"Файл сохранен: {filePath}");
        }

        private void BuildAndSaveExcelExecute()
        {
            mainRegionService.ShowProgressBar("Построение отчета");

            if (!File.Exists(settings.ServiceAccountingReportPath))
            {
                mainRegionService.HideProgressBar("Отменено. Путь к отчету не задан либо файл отсутствует");
                return;
            }

            if (Helpers.IsFileLocked(settings.ServiceAccountingReportPath))
            {
                mainRegionService.HideProgressBar("Отменено. Файл занят другим пользователем, поэтому не может быть изменен");
                return;
            }

            var rootDepartment = dbContext.Departments.Local.First(x => x.IsRoot);
            var rootComponent = dbContext.Components.Local.First(x => x.IsRoot);
            var report = new ReportService(rootDepartment, rootComponent);

            BuildReportInternal(report, false);
            //report.SaveExcel(settings.ServiceAccountingReportPath);

            BuildReportInternal(report, true);
            //report.SaveExcel(settings.ServiceAccountingReportPath);

            mainRegionService.HideProgressBar($"Отчет за месяц и нарастающий успешно построены и сохранены в excel файл");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            dbContext.Departments.Load();

            dbContext.Employees
                .Include(x => x.Medic)
                .Include(x => x.Specialty)
                .Load();

            dbContext.Parameters.Load();

            dbContext.Components
                .Include(x => x.Indicators).ThenInclude(x => x.Ratios)
                .Include(x => x.CaseFilters)
                .Load();

            dbContext.Departments.Local.ToList().ForEach(x => x.Childs = x.Childs.OrderBy(x => x.Order).ToList());
            dbContext.Departments.Local.ToList().ForEach(x => x.Employees = x.Employees.OrderBy(x => x.Order).ToList());
            dbContext.Departments.Local.ToList().ForEach(x => x.Parameters = x.Parameters.OrderBy(x => x.Order).ToList());
            dbContext.Employees.Local.ToList().ForEach(x => x.Parameters = x.Parameters.OrderBy(x => x.Order).ToList());
            dbContext.Components.Local.ToList().ForEach(x => x.Childs = x.Childs?.OrderBy(x => x.Order).ToList());
            dbContext.Components.Local.ToList().ForEach(x => x.Indicators = x.Indicators.OrderBy(x => x.Order).ToList());

            var rootDepartment = dbContext.Departments.Local.First(x => x.IsRoot);
            var rootComponent = dbContext.Components.Local.First(x => x.IsRoot);

            reportService = new ReportService(rootDepartment, rootComponent);

            RowHeaders = CreateHeaderItemRecursive(rootDepartment, null).ToListRecursive().Skip(1).ToList();
            ColumnHeaders = CreateHeaderItemRecursive(rootComponent, null).ToListRecursive().Skip(1).ToList();

            var rowSubHeaders = RowHeaders.SelectMany(x => x.SubItems).ToList();
            var columnSubHeaders = ColumnHeaders.SelectMany(x => x.SubItems).ToList();

            GridItems = new GridItem[rowSubHeaders.Count][];

            for (int row = 0; row < rowSubHeaders.Count; row++)
            {
                GridItems[row] = new GridItem[columnSubHeaders.Count];

                for (int col = 0; col < columnSubHeaders.Count; col++)
                    GridItems[row][col] = new GridItem(rowSubHeaders[row], columnSubHeaders[col], false);
            }

            var parameters = rootDepartment.ToListRecursive().Skip(1).SelectMany(x => x.Parameters.Concat(x.Employees.SelectMany(y => y.Parameters))).ToList();
            var indicators = rootComponent.ToListRecursive().Skip(1).SelectMany(x => x.Indicators).ToList();

            gridItemDataComparator = new Dictionary<GridItem, (Parameter, Indicator)>();

            for (int row = 0; row < parameters.Count; row++)
                for (int col = 0; col < indicators.Count; col++)
                    gridItemDataComparator.Add(GridItems[row][col], (parameters[row], indicators[col]));
        }

        private HeaderItem CreateHeaderItemRecursive(Department department, HeaderItem parent)
        {
            var subItemNames = department.Parameters.Select(x => x.Kind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(department.Name, null, department.HexColor, false, true, parent, subItemNames);

            foreach (var child in department.Childs)
                CreateHeaderItemRecursive(child, headerItem);

            foreach (var employee in department.Employees)
            {
                subItemNames = employee.Parameters.Select(x => x.Kind.GetShortDescription()).ToList();

                new HeaderItem(employee.Medic.FullName, employee.Specialty.Name, string.Empty, false, false, headerItem, subItemNames);
            }

            return headerItem;
        }

        private HeaderItem CreateHeaderItemRecursive(Component component, HeaderItem parent)
        {
            var subItemNames = component.Indicators.Select(x => x.FacadeKind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(component.Name, null, component.HexColor, false, component.Childs.Any(), parent, subItemNames);

            if (component.Childs != null)
                foreach (var child in component.Childs)
                    CreateHeaderItemRecursive(child, headerItem);

            return headerItem;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
