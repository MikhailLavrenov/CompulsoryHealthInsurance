using CHI.Models.ServiceAccounting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    public static class ReportHelper
    {
        static Color alternationColor1 = Colors.White;
        static Color alternationColor2 = Colors.WhiteSmoke;

        public static HeaderItem CreateHeaderItemRecursive(Department department, HeaderItem parent)
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

        public static HeaderItem CreateHeaderItemRecursive(Component component, HeaderItem parent)
        {
            var subItemNames = component.Indicators.Select(x => x.FacadeKind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(component.Name, null, component.HexColor, false, false, component.Childs.Any(), parent, subItemNames);

            if (component.Childs != null)
                foreach (var child in component.Childs)
                    CreateHeaderItemRecursive(child, headerItem);

            return headerItem;
        }

        public static void SetAlternationColor(List<HeaderItem> headerItems)
        {
            var colorSwitch = true;
            HeaderItem previousItemParent = null;

            foreach (var item in headerItems.Where(x => x.IsColorAlternation && !x.AlwaysHidden))
            {
                if (item.Parent != previousItemParent)
                    colorSwitch = true;

                item.Color = colorSwitch ? alternationColor1 : alternationColor2;

                colorSwitch = !colorSwitch;
                previousItemParent = item.Parent;
            }
        }

        public static void SaveExcel(string path, List<HeaderItem> rowHeaders, List<HeaderItem> columnHeaders, GridItem[][] gridItems, 
            int month, int year, bool isGrowing, bool isPlanning, string approvedBy)
        {
            using var excel = new ExcelPackage(new FileInfo(path));

            var sheetName = month == 0 ? "Макет" : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).Substring(0, 3);

            if (isGrowing)
                sheetName = $"Σ {sheetName}";

            var deleteSheet = excel.Workbook.Worksheets.FirstOrDefault(x => x.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            //в книге должен оставаться хотя бы 1 лист, иначе возникнет исключение
            if (deleteSheet != null)
                deleteSheet.Name += " sheet for delete";

            var sheet = excel.Workbook.Worksheets.Add(sheetName);

            if (deleteSheet != null)
                excel.Workbook.Worksheets.Delete(deleteSheet);

            var title = isPlanning ? "Планирование объемов" : "Отчет по выполнению объемов";

            if (month != 0)
            {
                title += $" за {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToLower()} {year}";

                if (isGrowing)
                    title += " нарастающий";
            }

            var subHeader = $"Построен {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";

            var exRowIndex = 1;

            if (isPlanning)
            {
                sheet.Cells[exRowIndex, 1].Value = approvedBy;
                sheet.Cells[exRowIndex, 1].Style.WrapText = true;

                exRowIndex += 2;
            }

            sheet.Cells[exRowIndex++, 1].Value = title;
            sheet.Cells[exRowIndex++, 1].Value = subHeader;

            var rowsOffset = isPlanning ? 5 : 3;

            //индексы записи в excel
            var exRow = rowsOffset + 1;
            var exCol = 3;

            //вставляет в excel заголовки столбцов
            foreach (var header in columnHeaders.Where(x => !x.AlwaysHidden))
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
            foreach (var header in rowHeaders.Where(x => !x.AlwaysHidden))
            {
                sheet.Cells[exRow, exCol, exRow + header.SubItems.Count - 1, exCol].Merge = true;
                sheet.Cells[exRow, exCol].Style.WrapText = true;

                if (isPlanning)
                    sheet.Cells[exRow, exCol].Value = $"{header.Name}   ({header.SubName})";
                else
                    sheet.Cells[exRow, exCol].Value = $"{header.Name}{Environment.NewLine}{header.SubName}";

                var drawingColor = Helpers.GetDrawingColor(header.Color);
                sheet.Cells[exRow, exCol, exRow + header.SubItems.Count - 1, exCol + 1].Style.Fill.SetBackground(drawingColor);

                foreach (var item in header.SubItems)
                    sheet.Cells[exRow++, exCol + 1].Value = item.Name;

            }

            exRow = rowsOffset + 3;

            //вставляет в excel значения
            foreach (var gridRowItems in gridItems.Where(x=>x.Any() && !x.First().RowSubHeader.HeaderItem.AlwaysHidden ))
            {
                exCol = 3;

                foreach (var gridItem in gridRowItems.Where(x => ! x.ColumnSubHeader.HeaderItem.AlwaysHidden))
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
            sheet.Column(1).Width = isPlanning ? 35 : 20;
            if (isPlanning)
                sheet.Row(1).Height = 80;
            Enumerable.Range(1, rowsOffset).ToList().ForEach(x => sheet.Cells[x, 1, x, sheet.Dimension.Columns].Merge = true);
            sheet.Cells[1 + rowsOffset, 1, 2 + rowsOffset, 2].Merge = true;
            sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            if (isPlanning)
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

            //добавление группировок по строкам
            exRow = rowsOffset + 3;

            foreach (var rowItems in gridItems)
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

            foreach (var colItem in gridItems[0])
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
    }
}
