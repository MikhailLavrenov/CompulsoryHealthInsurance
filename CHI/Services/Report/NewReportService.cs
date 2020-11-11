using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CHI.Services.Report
{
    public class NewReportService
    {
        RowHeaderGroup rootRow;
        ColumnHeaderGroup rootColumn;
        List<RowHeaderItem> RowItems;
        List<ColumnHeaderItem> ColumnItems;

        List<Parameter> parameters;
        List<Indicator> indicators;
        List<Department> departments;
        List<Employee> employees;
        List<Component> components;
        Department rootDepartment;
        Component rootComponent;
        public Dictionary<(Parameter, Indicator), double?> Results { get; set; }

        public int MoneyRoundDigits { get; set; }
        public List<RowHeaderGroup> RowGroups { get; set; }
        public List<ColumnHeaderGroup> ColumnGroups { get; set; }
        public ValueItem[][] Values { get; private set; }
        public List<ValueItem> ValuesList { get; private set; }
        public bool IsPlanningMode { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }
        public bool IsGrowing { get; private set; }
        public string ApprovedBy { get; set; }


        public NewReportService(Department rootDepartment, Component rootComponent, bool isPlanningMode = false)
        {
            IsPlanningMode = isPlanningMode;

            this.rootDepartment = rootDepartment;
            this.rootComponent = rootComponent;

            departments = rootDepartment.ToListRecursive();
            employees = departments.SelectMany(x => x.Employees).ToList();
            components = rootComponent.ToListRecursive();
            parameters = departments.SelectMany(x => x.Parameters).ToList().Concat(employees.SelectMany(x => x.Parameters).ToList()).ToList();
            indicators = components.SelectMany(x => x.Indicators).ToList();

            Results = parameters.SelectMany(x => indicators, (x, y) => (x, y)).ToDictionary(x => x, x => (double?)null);
        }


        public void Build(List<Register> registers, List<Plan> plans, int month, int year, bool isGrowing = false)
        {
            Month = month;
            Year = year;
            IsGrowing = isGrowing;

            foreach (var key in Results.Keys)
                Results[key] = 0;

            //заполняет план
            foreach (var parameter in parameters.Where(x => x.Kind == ParameterKind.EmployeePlan || x.Kind == ParameterKind.DepartmentHandPlan))
                foreach (var indicator in indicators.Where(x => x.Component.IsCanPlanning))
                    Results[(parameter, indicator)] = plans.Where(x => x.Parameter.Id == parameter.Id && x.Indicator.Id == indicator.Id).Sum(x => x.Value);


            if (!IsPlanningMode && registers != null)
                for (var currentMonth = isGrowing ? 1 : month; currentMonth <= month; currentMonth++)
                {
                    var cases = registers.FirstOrDefault(x => x.Month == currentMonth)?.Cases;

                    if (cases == null)
                        continue;

                    //заполняет факт
                    SetValuesFromCases(currentMonth, year, cases, true);

                    //заполняет ошибки (снятия)
                    SetValuesFromCases(currentMonth, year, cases, false);
                }

            SumRows();

            SumColumns();

            //вычисляет проценты в штатных единицах
            foreach (var employee in employees)
            {
                var percentParamenter = employee.Parameters.Where(x => x.Kind == ParameterKind.EmployeePercent).First();
                var dividendParameter = employee.Parameters.Where(x => x.Kind == ParameterKind.EmployeePlan).First();
                var dividerParameter = employee.Parameters.Where(x => x.Kind == ParameterKind.EmployeeFact).First();

                foreach (var indicator in indicators)
                {
                    var dividend = Results[(dividendParameter, indicator)];
                    var divider = Results[(dividerParameter, indicator)];
                    Results[(percentParamenter, indicator)] = divider == 0 ? 0 : dividend / divider;
                }
            }

            //вычисляет проценты в подразделениях
            foreach (var department in departments)
            {
                var percentParamenter = department.Parameters.Where(x => x.Kind == ParameterKind.DepartmentPercent).First();
                var dividendParameter = department.Parameters.Where(x => x.Kind == ParameterKind.DepartmentHandPlan).First();
                var dividerParameter = department.Parameters.Where(x => x.Kind == ParameterKind.DepartmentFact).First();

                foreach (var indicator in indicators)
                {
                    var dividend = Results[(dividendParameter, indicator)];
                    var divider = Results[(dividerParameter, indicator)];
                    Results[(percentParamenter, indicator)] = divider == 0 ? 0 : dividend / divider;
                }
            }

            DropZeroAndRoundValues();

            ////скрывает пустые строки
            //foreach (var rowGroup in RowGroups)
            //{
            //    bool canVisible = IsPlanningMode ? !(rowGroup.Employee?.IsArchive ?? false) : false;

            //    if (!IsPlanningMode || !canVisible)
            //        foreach (var headerItem in rowGroup.HeaderItems)
            //            if (Values[headerItem.Index].Any(x => x.Value.HasValue))
            //            {
            //                canVisible = true;
            //                break;
            //            }

            //    rowGroup.CanVisible = canVisible;
            //}

            ////обновляет чередующийся цвет, тк при сокрытии пустых строк чередование цвета может сбиться
            //foreach (var rowGroup in RowGroups.Where(x => x.Childs.FirstOrDefault()?.Employee != null))
            //{
            //    var alter = true;

            //    foreach (var item in rowGroup.Childs.Where(x => x.CanVisible))
            //    {
            //        item.Color = alter ? RowHeaderGroup.AlternationColor1 : RowHeaderGroup.AlternationColor2;
            //        alter = !alter;
            //    }
            //}
        }

        public void UpdateCalculatedCells()
        {
            SumRows();

            SumColumns();

            DropZeroAndRoundValues();
        }

        //суммирует строки
        private void SumRows()
        {
            foreach (var parameter in parameters.Where(x => x.Department != null).Reverse())
            {
                ParameterKind eqEmployeeKind = parameter.Kind switch
                {
                    ParameterKind.DepartmentCalculatedPlan => ParameterKind.EmployeePlan,
                    ParameterKind.DepartmentHandPlan when parameter.Department.Childs.Any() => ParameterKind.DepartmentHandPlan,
                    ParameterKind.DepartmentFact => ParameterKind.EmployeeFact,
                    ParameterKind.DepartmentRejectedFact => ParameterKind.EmployeeRejectedFact,
                    _ => ParameterKind.None,
                };

                if (eqEmployeeKind == ParameterKind.None)
                    continue;

                foreach (var indicator in indicators.Where(x => x.Component.CaseFilters.First().Kind != CaseFilterKind.Total))
                {
                    double sum = 0;

                    parameter.Department.Childs
                        .SelectMany(x => x.Parameters).Concat(parameter.Department.Employees.SelectMany(y => y.Parameters))
                        .Where(x => x.Kind == parameter.Kind || x.Kind == eqEmployeeKind)
                        .ToList()
                        .ForEach(x => sum += Results[(x, indicator)] ?? 0);

                    Results[(parameter, indicator)] = sum;
                }
            }
        }

        //суммирует столбцы
        private void SumColumns()
        {
            foreach (var parameter in parameters)
                foreach (var indicator in indicators.Where(x => x.Component.CaseFilters.First().Kind == CaseFilterKind.Total).Reverse())
                {
                    double sum = 0;

                    indicator.Component.Childs
                        .SelectMany(x => x.Indicators)
                        .Where(x => x.FacadeKind == indicator.ValueKind)
                        .ToList()
                        .ForEach(x => sum += Results[(parameter, x)] ?? 0);

                    Results[(parameter, indicator)] = sum;
                }
        }

        //округляет значения и убирает нули
        private void DropZeroAndRoundValues()
        {
            foreach (var key in Results.Keys)
            {
                var result = Results[key];

                if (result == null)
                    continue;

                if (result == 0)
                    result = null;
                else if (key.Item1.Kind == ParameterKind.DepartmentPercent)
                    result = Math.Round(result.Value, 1, MidpointRounding.AwayFromZero);

                else if (key.Item2.FacadeKind == IndicatorKind.Cost)
                    result = Math.Round(result.Value, MoneyRoundDigits, MidpointRounding.AwayFromZero);

                else
                    result = Math.Round(result.Value, 0, MidpointRounding.AwayFromZero);

                Results[key] = result;
            }
        }

        private void SetValuesFromCases(int month, int year, IEnumerable<Case> cases, bool isPaymentAccepted)
        {
            var filters = ColumnGroups.Select(x => new
            {
                TreatmentCodes = x.TreatmentFilters.Where(y => Helpers.BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList(),
                VisitCodes = x.VisitFilters.Where(y => Helpers.BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList(),
                ContainsServiceCodes = x.ContainsServiceFilters.Where(y => Helpers.BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList(),
                NotContainsServiceCodes = x.NotContainsServiceFilters.Where(y => Helpers.BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList()
            })
                .ToList();

            cases = cases.Where(x => x.PaidStatus != PaidKind.Refuse == isPaymentAccepted).ToList();

            foreach (var rowGroup in RowGroups.Where(x => x.Employee != null))
            {
                var employeeCases = cases.Where(x => x.Employee.Id == rowGroup.Employee.Id).ToList();

                foreach (var columnGroup in ColumnGroups.Where(x => x.Component.CaseFilters.First().Kind != CaseFilterKind.Total))
                {
                    IEnumerable<Case> selectedCases = employeeCases;

                    var filter = filters[columnGroup.Index];

                    if (filter.TreatmentCodes.Any())
                        selectedCases = selectedCases.Where(x => filter.TreatmentCodes.Contains(x.TreatmentPurpose));
                    if (filter.VisitCodes.Any())
                        selectedCases = selectedCases.Where(x => filter.VisitCodes.Contains(x.VisitPurpose));
                    if (filter.ContainsServiceCodes.Any())
                        selectedCases = selectedCases.Where(x => x.Services.Any(y => filter.ContainsServiceCodes.Contains(y.Code)));
                    if (filter.NotContainsServiceCodes.Any())
                        selectedCases = selectedCases.Where(x => x.Services.Any(y => !filter.NotContainsServiceCodes.Contains(y.Code)));

                    selectedCases = selectedCases.ToList();

                    foreach (var rowItem in rowGroup.HeaderItems.Where(x => (isPaymentAccepted && x.Parameter.Kind == ParameterKind.EmployeeFact) || (!isPaymentAccepted && x.Parameter.Kind == ParameterKind.EmployeeRejectedFact)))
                        foreach (var columnItem in columnGroup.HeaderItems)
                        {
                            var valueItem = Values[rowItem.Index][columnItem.Index];

                            double value = 0;

                            switch (valueItem.ColumnHeader.Indicator.ValueKind)
                            {
                                case IndicatorKind.Cases:
                                    value = selectedCases.Count();
                                    break;

                                case IndicatorKind.Services:
                                    value = selectedCases.Select(x => x.Services.Count == 0 ? 0 : x.Services.Count - 1).Sum();
                                    break;

                                case IndicatorKind.BedDays:
                                    value = selectedCases.Sum(x => x.BedDays);
                                    break;

                                case IndicatorKind.LaborCost:
                                    value = selectedCases
                                        .SelectMany(x => x.Services)
                                        .Where(x => x.ClassifierItem != null)
                                        .Sum(x => x.Count * x.ClassifierItem.LaborCost);
                                    break;

                                case IndicatorKind.Cost:
                                    if (isPaymentAccepted)
                                        value = selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.None)
                                            .SelectMany(x => x.Services)
                                            .Where(x => x.ClassifierItem != null)
                                            .Sum(x => x.Count * x.ClassifierItem.Price)
                                            + selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.Full || x.PaidStatus == PaidKind.Partly)
                                            .Sum(x => x.AmountPaid);
                                    else
                                        value = selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.Refuse || x.PaidStatus == PaidKind.Partly)
                                            .Sum(x => x.AmountUnpaid);
                                    break;
                            }

                            var ratio = columnItem.Indicator.Ratios.FirstOrDefault(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, month, year));

                            if (ratio != null && value != 0)
                                valueItem.Value += value * ratio.Multiplier / ratio.Divider;
                            else
                                valueItem.Value += value;

                        }
                }
            }

        }

        public void SaveExcel(string path)
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

            var header = IsPlanningMode ? "Планирование объемов" : "Отчет по выполнению объемов";

            if (Month != 0)
            {
                header += $" за {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month).ToLower()} {Year}";

                if (IsGrowing)
                    header += " нарастающий";
            }

            var subHeader = $"Построен {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";

            var exRowIndex = 1;

            if (IsPlanningMode)
            {
                sheet.Cells[exRowIndex, 1].Value = ApprovedBy;
                sheet.Cells[exRowIndex, 1].Style.WrapText = true;

                exRowIndex += 2;
            }

            sheet.Cells[exRowIndex++, 1].Value = header;
            sheet.Cells[exRowIndex++, 1].Value = subHeader;

            var rowsOffset = IsPlanningMode ? 5 : 3;

            //вставляет в excel заголовки столбцов
            var exCol = 3;

            for (int i = 0; i < ColumnItems.Count; i++)
            {
                if ((i > 0 && ColumnItems[i - 1].Group != ColumnItems[i].Group) || i == 0)
                {
                    sheet.Cells[1 + rowsOffset, exCol, 1 + rowsOffset, exCol + ColumnItems[i].Group.HeaderItems.Count - 1].Merge = true;

                    sheet.Cells[1 + rowsOffset, exCol].Value = ColumnItems[i].Group.Name;
                    var drawingColor = Helpers.GetDrawingColor(ColumnItems[i].Group.Color);
                    sheet.Cells[1 + rowsOffset, exCol].Style.Fill.SetBackground(drawingColor);
                }

                sheet.Cells[2 + rowsOffset, exCol].Value = ColumnItems[i].Name;

                var drawingColor2 = Helpers.GetDrawingColor(ColumnItems[i].Group.Color);
                sheet.Cells[2 + rowsOffset, exCol].Style.Fill.SetBackground(drawingColor2);

                exCol++;
            }

            //вставляет в excel заголовки строк
            var exRow = 3 + rowsOffset;

            for (int i = 0; i < RowItems.Count; i++)
            {
                if ((i > 0 && RowItems[i - 1].Group != RowItems[i].Group) || i == 0)
                {
                    sheet.Cells[exRow, 1, exRow + RowItems[i].Group.HeaderItems.Count - 1, 1].Merge = true;

                    sheet.Cells[exRow, 1].Style.WrapText = true;

                    sheet.Cells[exRow, 1].Value = IsPlanningMode switch
                    {
                        false => $"{RowItems[i].Group.Name}{Environment.NewLine}{RowItems[i].Group.SubName}",
                        true when !string.IsNullOrEmpty(RowItems[i].Group.SubName) => $"{RowItems[i].Group.Name}   ({RowItems[i].Group.SubName})",
                        _ => RowItems[i].Group.Name
                    };

                    var drawingColor = Helpers.GetDrawingColor(RowItems[i].Group.Color);
                    sheet.Cells[exRow, 1].Style.Fill.SetBackground(drawingColor);
                }

                sheet.Cells[exRow, 2].Value = RowItems[i].Name;

                System.Drawing.Color drawingColor2 = new System.Drawing.Color();
                drawingColor2 = Helpers.GetDrawingColor(RowItems[i].Group.Color);
                sheet.Cells[exRow, 2].Style.Fill.SetBackground(drawingColor2);

                exRow++;
            }

            //вставляет в excel значения отчета
            exRow = 3 + rowsOffset;
            foreach (var rowValues in Values)
            {
                foreach (var valueItem in rowValues)
                {
                    sheet.Cells[exRow, valueItem.ColumnIndex + 3].Value = valueItem.Value;

                    var drawingColor = Helpers.GetDrawingColor(valueItem.Color);
                    sheet.Cells[exRow, valueItem.ColumnIndex + 3].Style.Fill.SetBackground(drawingColor);
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
            sheet.Column(1).Width = IsPlanningMode ? 35 : 20;
            if (IsPlanningMode)
                sheet.Row(1).Height = 80;
            Enumerable.Range(1, rowsOffset).ToList().ForEach(x => sheet.Cells[x, 1, x, sheet.Dimension.Columns].Merge = true);
            sheet.Cells[1 + rowsOffset, 1, 2 + rowsOffset, 2].Merge = true;
            sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            if (IsPlanningMode)
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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

            foreach (var columnItem in ColumnItems.Where(x => x.Group.Level > 0))
                for (int i = 1; i <= columnItem.Group.Level; i++)
                    sheet.Column(columnItem.Index + 3).OutlineLevel = i;

            foreach (var rowItem in RowItems.Where(x => x.Group.Level > 0))
                for (int i = 1; i <= rowItem.Group.Level; i++)
                    sheet.Row(rowItem.Index + 3 + rowsOffset).OutlineLevel = i;

            sheet.View.FreezePanes(3 + rowsOffset, 3);

            //удаляет скрытые строки
            var deletedCount = 0;
            foreach (var rowItem in RowItems.Where(x => !x.Group.CanVisible))
            {
                //sheet.Row().Style.Hidden = true;
                sheet.DeleteRow(3 + rowsOffset + rowItem.Index - deletedCount);

                deletedCount++;
            }

            excel.Save();
        }
    }
}
