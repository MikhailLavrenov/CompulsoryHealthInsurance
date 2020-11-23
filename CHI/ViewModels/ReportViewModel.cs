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
        Color alternationColor1 = Colors.White;
        Color alternationColor2 = Colors.WhiteSmoke;
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

            SetAlternationColor();
        }

        //задает чередование цвета в видимых строках с Employee
        private void SetAlternationColor()
        {
            var colorSwitch = true;
            HeaderItem previousItemParent = null;

            foreach (var item in RowHeaders.Where(x => x.IsColorAlternation && !x.AlwaysHidden))
            {
                if (item.Parent != previousItemParent)
                    colorSwitch = true;

                item.Color = colorSwitch ? alternationColor1 : alternationColor2;

                colorSwitch = !colorSwitch;
                previousItemParent = item.Parent;
            }
        }

        private void SaveExcelInternal(string path)
        {
            using var excel = new ExcelPackage(new FileInfo(path));

            var sheetName = Year == 0 ? "Макет" : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month).Substring(0, 3);

            if (IsGrowing)
                sheetName = $"Σ {sheetName}";

            var deleteSheet = excel.Workbook.Worksheets.FirstOrDefault(x => x.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            //в книге должен оставаться хотя бы 1 лист, иначе возникнет исключение
            if (deleteSheet != null)
                deleteSheet.Name += " list for delete";

            var sheet = excel.Workbook.Worksheets.Add(sheetName);

            if (deleteSheet != null)
                excel.Workbook.Worksheets.Delete(deleteSheet);

            //var header = IsPlanningMode ? "Планирование объемов" : "Отчет по выполнению объемов";
            var title = "Отчет по выполнению объемов";

            if (Month != 0)
            {
                title += $" за {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month).ToLower()} {Year}";

                if (IsGrowing)
                    title += " нарастающий";
            }

            var subHeader = $"Построен {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";

            var exRowIndex = 1;

            //if (IsPlanningMode)
            //{
            //    sheet.Cells[exRowIndex, 1].Value = ApprovedBy;
            //    sheet.Cells[exRowIndex, 1].Style.WrapText = true;

            //    exRowIndex += 2;
            //}

            sheet.Cells[exRowIndex++, 1].Value = title;
            sheet.Cells[exRowIndex++, 1].Value = subHeader;

            //var rowsOffset = IsPlanningMode ? 5 : 3;
            var rowsOffset = 3;

            //индексы записи в excel
            var exRow = rowsOffset + 1;
            var exCol = 3;

            //вставляет в excel заголовки столбцов
            foreach (var header in ColumnHeaders.Where(x => !x.AlwaysHidden))
            {
                sheet.Cells[exRow, exCol, exRow, exCol + header.SubItems.Count - 1].Merge = true;
                sheet.Cells[exRow, exCol].Value = header.Name;
                var drawingColor = Helpers.GetDrawingColor(header.Color);
                sheet.Cells[exRow, exCol, exRow + 1, exCol + header.SubItems.Count - 1].Style.Fill.SetBackground(drawingColor);

                foreach (var item in header.SubItems)
                    sheet.Cells[exRow + 1, exCol++].Value = item.Name;
            }

            exRow = rowsOffset + 3;
            exCol = 1;

            //вставляет в excel заголовки строк
            foreach (var header in RowHeaders.Where(x => !x.AlwaysHidden))
            {
                sheet.Cells[exRow, exCol, exRow + header.SubItems.Count - 1, exCol].Merge = true;
                sheet.Cells[exRow, exCol].Style.WrapText = true;

                //if (IsPlanningMode)
                //    sheet.Cells[exRow, exCol].Value = $"{header.Name}   ({header.SubName})";
                //else 
                sheet.Cells[exRow, exCol].Value = $"{header.Name}{Environment.NewLine}{header.SubName}";

                var drawingColor = Helpers.GetDrawingColor(header.Color);
                sheet.Cells[exRow, exCol, exRow + header.SubItems.Count - 1, exCol + 1].Style.Fill.SetBackground(drawingColor);

                foreach (var item in header.SubItems)
                    sheet.Cells[exRow++, exCol + 1].Value = item.Name;

            }

            exRow = rowsOffset + 3;

            foreach (var gridRowItems in GridItems)
            {
                exCol = 3;

                foreach (var gridItem in gridRowItems.Where(x => !x.RowSubHeader.HeaderItem.AlwaysHidden && !x.ColumnSubHeader.HeaderItem.AlwaysHidden))
                {
                    sheet.Cells[exRow, exCol].Value = gridItem.Value;
                    var drawingColor = Helpers.GetDrawingColor(gridItem.Color);
                    sheet.Cells[exRow, exCol].Style.Fill.SetBackground(drawingColor);

                    exCol++;
                }

                exRow++;
            }

            //форматирование
            sheet.PrinterSettings.Orientation = eOrientation.Landscape;
            sheet.PrinterSettings.PaperSize = ePaperSize.A4;
            sheet.PrinterSettings.LeftMargin = 0.2m;
            sheet.PrinterSettings.RightMargin = 0.2m;
            sheet.PrinterSettings.TopMargin = 0.2m;
            sheet.PrinterSettings.BottomMargin = 0.2m;
            sheet.PrinterSettings.HeaderMargin = 0;
            sheet.PrinterSettings.FooterMargin = 0;
            sheet.PrinterSettings.Scale = 60;
            sheet.DefaultColWidth = 9;
            //sheet.Column(1).Width = IsPlanningMode ? 35 : 20;
            sheet.Column(1).Width = 20;
            //if (IsPlanningMode)
            //    sheet.Row(1).Height = 80;
            Enumerable.Range(1, rowsOffset).ToList().ForEach(x => sheet.Cells[x, 1, x, sheet.Dimension.Columns].Merge = true);
            sheet.Cells[1 + rowsOffset, 1, 2 + rowsOffset, 2].Merge = true;
            sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //if (IsPlanningMode)
            //    sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(1).Style.Font.Bold = true;
            sheet.Column(2).Style.Font.Bold = true;
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(2).Style.Font.Bold = false;
            sheet.Row(1 + rowsOffset).Style.Font.Bold = true;
            sheet.Row(2 + rowsOffset).Style.Font.Bold = true;

            var range = sheet.Cells[1 + rowsOffset, 1, sheet.Dimension.Rows, sheet.Dimension.Columns];

            range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
            range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
            range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;


            sheet.OutLineSummaryRight = false;
            sheet.OutLineSummaryBelow = false;

            //добавление группировок по строкам
            exRow = rowsOffset + 3;

            foreach (var rowItems in GridItems)
            {
                var header = rowItems[0].RowSubHeader.HeaderItem;

                if (header.AlwaysHidden)
                    continue;

                if (header.Level > 1)
                    sheet.Row(exRow).OutlineLevel = header.Level;

                exRow++;
            }

            //добавление группировок по столбцам
            exCol = 3;

            foreach (var colItem in GridItems[0])
            {
                var header = colItem.ColumnSubHeader.HeaderItem;

                if (header.AlwaysHidden)
                    continue;

                if (header.Level > 1)
                    sheet.Column(exCol).OutlineLevel = header.Level;

                exCol++;
            }

            sheet.View.FreezePanes(3 + rowsOffset, 3);

            excel.Save();
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

            SaveExcelInternal(filePath);

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

            ReportIsVisible = false;

            var isGrowingInitialValue = IsGrowing;

            IsGrowing = false;
            BuildReportInternal();
            SaveExcelInternal(settings.ServiceAccountingReportPath);

            IsGrowing = true;
            BuildReportInternal();
            SaveExcelInternal(settings.ServiceAccountingReportPath);

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

            SetAlternationColor();
        }

        private HeaderItem CreateHeaderItemRecursive(Department department, HeaderItem parent)
        {
            var subItemNames = department.Parameters.Select(x => x.Kind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(department.Name, null, department.HexColor, false, false, true, parent, subItemNames);

            foreach (var child in department.Childs)
                CreateHeaderItemRecursive(child, headerItem);

            foreach (var employee in department.Employees)
            {
                subItemNames = employee.Parameters.Select(x => x.Kind.GetShortDescription()).ToList();

                new HeaderItem(employee.Medic.FullName, employee.Specialty.Name, string.Empty, true, false, false, headerItem, subItemNames);
            }

            return headerItem;
        }

        private HeaderItem CreateHeaderItemRecursive(Component component, HeaderItem parent)
        {
            var subItemNames = component.Indicators.Select(x => x.FacadeKind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(component.Name, null, component.HexColor, false, false, component.Childs.Any(), parent, subItemNames);

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