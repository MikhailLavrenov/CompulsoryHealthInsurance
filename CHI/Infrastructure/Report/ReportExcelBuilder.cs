using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CHI.Infrastructure
{
    public class ReportExcelBuilder
    {
        ExcelPackage excel;
        ExcelWorksheet sheet;
        string month;
        int year;
        bool isGrowing;
        bool? isPlaning;
        string approvedBy;

        public ReportExcelBuilder(string path)
        {
            excel = new ExcelPackage(new FileInfo(path));
        }

        public ReportExcelBuilder UsePlaningStyle(string approvedBy)
        {
            CheckExcelNotClosed();

            isPlaning = true;
            this.approvedBy = approvedBy;

            return this;
        }

        public ReportExcelBuilder UseReportStyle()
        {
            CheckExcelNotClosed();

            isPlaning = false;

            return this;
        }

        public ReportExcelBuilder SetNewSheet(int monthNumber, int year)
            => SetNewSheet(monthNumber, year, false);

        public ReportExcelBuilder SetNewSheet(int monthNumber, int year, bool isGrowing)
        {
            CheckExcelNotClosed();

            if (isPlaning == null)
                throw new InvalidOperationException("Сначала задайте стиль.");

            month = monthNumber == 0 ? string.Empty : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNumber);
            this.year = year;
            this.isGrowing = isGrowing;

            var sheetName = string.IsNullOrEmpty(month) ? "Макет" : month.Substring(0, 3);

            if (isGrowing)
                sheetName = $"Σ {sheetName}";

            var deleteSheet = excel.Workbook.Worksheets.FirstOrDefault(x => x.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

            //в книге должен оставаться хотя бы 1 лист, иначе возникнет исключение
            if (deleteSheet != null)
                deleteSheet.Name += " sheet for delete";

            sheet = excel.Workbook.Worksheets.Add(sheetName);

            if (deleteSheet != null)
                excel.Workbook.Worksheets.Delete(deleteSheet);

            return this;
        }

        public ReportExcelBuilder FillSheet(List<HeaderItem> rowHeaders, List<HeaderItem> columnHeaders, GridItem[][] gridItems)
        {
            CheckExcelNotClosed();

            if (sheet == null)
                throw new InvalidOperationException("Сначала создайте лист");

            var title = isPlaning == true ? "Планирование объемов" : "Отчет по выполнению объемов";

            if (!string.IsNullOrEmpty(month))
            {
                title += $" за {month.ToLower()} {year}";

                if (isGrowing)
                    title += " нарастающий";
            }

            var subHeader = $"Построен {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";

            var exRowIndex = 1;

            if (isPlaning == true)
            {
                sheet.Cells[exRowIndex, 1].Value = approvedBy;
                sheet.Cells[exRowIndex, 1].Style.WrapText = true;

                exRowIndex += 2;
            }

            sheet.Cells[exRowIndex++, 1].Value = title;
            sheet.Cells[exRowIndex++, 1].Value = subHeader;

            var rowsOffset = isPlaning == true ? 5 : 3;

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

                if (isPlaning == true)
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
            foreach (var gridRowItems in gridItems.Where(x => x.Any() && !x.First().RowSubHeader.HeaderItem.AlwaysHidden))
            {
                exCol = 3;

                foreach (var gridItem in gridRowItems.Where(x => !x.ColumnSubHeader.HeaderItem.AlwaysHidden))
                {
                    sheet.Cells[exRow, exCol].Value = gridItem.Value;
                    var drawingColor = Helpers.GetDrawingColor(gridItem.Color);
                    sheet.Cells[exRow, exCol].Style.Fill.SetBackground(drawingColor);

                    exCol++;
                }

                exRow++;
            }

            var firstRow = rowsOffset + 1;
            var lastRow = sheet.Dimension.Rows;
            var firstColumn = 1;
            var lastColumn = sheet.Dimension.Columns;

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
            sheet.Column(1).Width = isPlaning == true ? 35 : 20;
            if (isPlaning == true)
                sheet.Row(1).Height = 80;
            Enumerable.Range(1, rowsOffset).ToList().ForEach(x => sheet.Cells[x, firstColumn, x, lastColumn].Merge = true);
            sheet.Cells[firstRow, firstColumn, firstRow + 1, firstColumn + 1].Merge = true;
            sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            if (isPlaning == true)
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(firstColumn).Style.Font.Bold = true;
            sheet.Column(firstColumn + 1).Style.Font.Bold = true;
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(2).Style.Font.Bold = false;
            sheet.Row(firstRow).Style.Font.Bold = true;
            sheet.Row(firstRow + 1).Style.Font.Bold = true;

            //добавление линии сетки таблицы
            var range = sheet.Cells[firstRow, firstColumn, lastRow, lastColumn];

            range.Style.Border.Left.Style = ExcelBorderStyle.Hair;
            range.Style.Border.Top.Style = ExcelBorderStyle.Hair;
            range.Style.Border.Right.Style = ExcelBorderStyle.Hair;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;

            sheet.Cells[firstRow, firstColumn, firstRow, lastColumn].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            sheet.Cells[lastRow+1, firstColumn, lastRow+1, lastColumn].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            sheet.Cells[firstRow, firstColumn, lastRow, firstColumn].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells[firstRow, lastColumn+1, lastRow, lastColumn+1].Style.Border.Left.Style = ExcelBorderStyle.Thin;

            exRow = firstRow + 2;

            foreach (var header in rowHeaders.Where(x => !x.AlwaysHidden))
            {
                if (header.CanCollapse == true)
                    sheet.Cells[exRow, firstColumn, exRow, lastColumn].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                exRow += header.SubItems.Count;
            }


            //добавление группировок
            sheet.OutLineSummaryRight = false;
            sheet.OutLineSummaryBelow = false;

            //добавление группировок по строкам
            exRow = firstRow + 2;

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

            return this;
        }

        public void SaveAndClose()
        {
            excel?.Save();

            excel?.Dispose();
            excel = null;
        }

        void CheckExcelNotClosed()
        {
            if (excel == null)
                throw new InvalidOperationException("Эксель файл был закрыт. Создайте новый экземпляр.");
        }
    }
}
