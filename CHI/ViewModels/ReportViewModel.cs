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
        bool reportIsVisible;
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
        public bool ReportIsVisible { get => reportIsVisible; set => SetProperty(ref reportIsVisible, value); }
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

            BuildReportInternal();

            ReportIsVisible = true;

            mainRegionService.HideProgressBar($"Отчет за {Months[Month]} {Year} построен");
        }

        private void BuildReportInternal()
        {
            var monthBegin = IsGrowing ? 1 : Month;

            var registers = dbContext.Registers
                .Where(x => x.Year == Year && monthBegin <= x.Month && x.Month <= Month)
                .Include(x => x.Cases).ThenInclude(x => x.Services).ThenInclude(x => x.ClassifierItem)
                .ToList();

            var plans = dbContext.Plans.Where(x => x.Year == Year && monthBegin <= x.Month && x.Month <= Month).ToList();

            reportService.Build(registers, plans, Month, Year, IsGrowing);

            foreach (var item in gridItemDataComparator)
                item.Key.Value = reportService.Results[item.Value];

            //скрывает заголовки строк Employee где нет значений
            var employeeHeaderGroups = GridItems
                .SelectMany(x => x)
                .Where(x => !x.RowSubHeader.HeaderItem.Childs.Any())
                .GroupBy(x => x.RowSubHeader.HeaderItem);

            foreach (var group in employeeHeaderGroups)
                group.Key.AlwaysHidden = !group.Where(x => x.Value.HasValue).Any();

            ReportHelper.SetAlternationColor(rowHeaders);
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

            if (File.Exists(filePath) && Helpers.IsFileLocked(filePath))
            {
                mainRegionService.HideProgressBar("Отменено. Файл занят другим пользователем, поэтому не может быть изменен");
                return;
            }

            mainRegionService.ShowProgressBar("Сохранение файла");

            new ReportExcelBuilder(filePath)   
                .UseReportStyle()   
                .SetNewSheet(reportService.Month,reportService.Year,reportService.IsGrowing)   
                .FillSheet(RowHeaders, ColumnHeaders, GridItems)  
                .SaveAndClose();

            mainRegionService.HideProgressBar($"Файл сохранен: {filePath}");
        }

        private void BuildAndSaveExcelExecute()
        {
            mainRegionService.ShowProgressBar("Построение отчета");

            if (!Directory.Exists(Path.GetDirectoryName(settings.ServiceAccountingReportPath)))
            {
                mainRegionService.HideProgressBar("Отменено. Заданная директория не существует");
                return;
            }

            if (File.Exists(settings.ServiceAccountingReportPath) && Helpers.IsFileLocked(settings.ServiceAccountingReportPath))
            {
                mainRegionService.HideProgressBar("Отменено. Файл занят другим пользователем, поэтому не может быть изменен");
                return;
            }

            ReportIsVisible = false;

            var isGrowingInitialValue = IsGrowing;

            IsGrowing = false;
            BuildReportInternal();

            var excelBuilder = new ReportExcelBuilder(settings.ServiceAccountingReportPath)                
                .UseReportStyle()
                .SetNewSheet(reportService.Month, reportService.Year, reportService.IsGrowing)
                .FillSheet(RowHeaders, ColumnHeaders, GridItems);

            IsGrowing = true;
            BuildReportInternal();

            excelBuilder.SetNewSheet(reportService.Month, reportService.Year, reportService.IsGrowing)
                .FillSheet(RowHeaders, ColumnHeaders, GridItems)
                .SaveAndClose();

            IsGrowing = isGrowingInitialValue;

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
                    gridItemDataComparator.Add(GridItems[row][col], (parameters[row], indicators[col]));

            ReportHelper.SetAlternationColor(rowHeaders);
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