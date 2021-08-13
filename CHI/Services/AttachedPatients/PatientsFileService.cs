using CHI.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.Services
{
    /// <summary>
    /// Cервис для работа с файлом прикрепленных пациентов
    /// </summary>
    public class PatientsFileService : IDisposable
    {
        ExcelPackage excel;
        ExcelWorksheet sheet;
        List<IColumnProperties> columnProperties;
        int maxRow;
        int maxCol;
        int headerRowIndex = 1;
        int insuranceColumn;
        int initialsColumn;
        int surnameColumn;
        int nameColumn;
        int patronymicColumn;
        Patient[] patientsInFile;

        /// <summary>
        ///
        /// </summary>
        /// <param name="xlsxFilePath">Полный путь к xlsx файлу.</param>
        /// <param name="columnProperties">Коллекиця настроиваемых свойств столбоц файла.</param>
        public PatientsFileService(string xlsxFilePath, IEnumerable<IColumnProperties> columnProperties)
        {

            excel = new ExcelPackage(new FileInfo(xlsxFilePath));
            sheet = excel.Workbook.Worksheets.First();
            this.columnProperties = columnProperties.ToList();
            maxRow = sheet.Dimension.Rows;
            maxCol = sheet.Dimension.Columns;
            patientsInFile = new Patient[maxRow - headerRowIndex];

            SetColumnsIndexes();
            AddMissingColumnsIfNeeded();
            ReadPatients();
        }

        void SetColumnsIndexes()
        {
            insuranceColumn = FindColumnIndexByHeaderName("ENP");
            initialsColumn = FindColumnIndexByHeaderName("FIO");
            surnameColumn = FindColumnIndexByHeaderName("Фамилия");
            nameColumn = FindColumnIndexByHeaderName("Имя");
            patronymicColumn = FindColumnIndexByHeaderName("Отчество");

            if (insuranceColumn == -1)
                throw new InvalidOperationException("Не найден столбец с номером полиса");

            if (initialsColumn == -1)
                throw new InvalidOperationException("Не найден столбец с инициалами ФИО");
        }

        void AddMissingColumnsIfNeeded()
        {
            if (surnameColumn == -1)
                surnameColumn = AddMissingColumn(initialsColumn, "Фамилия");

            if (nameColumn == -1)
                nameColumn = AddMissingColumn(surnameColumn, "Имя");

            if (patronymicColumn == -1)
                patronymicColumn = AddMissingColumn(nameColumn, "Отчество");
        }

        int AddMissingColumn(int previousColumnIndex, string columnName)
        {
            var columnIndex = previousColumnIndex + 1;
            sheet.InsertColumn(columnIndex, 1);
            sheet.Cells[headerRowIndex, columnIndex].Value = columnName;
            maxCol++;

            return columnIndex;
        }

        IColumnProperties GetColumnProperty(string text)
            => columnProperties.Where(x => x.NameOrAltNameIsEqual(text)).FirstOrDefault();

        int FindColumnIndexByHeaderName(string[] nameVariants)
        {
            var cell = sheet.Cells[headerRowIndex, 1, headerRowIndex, maxCol]
                .Where(x => x.Value != null && nameVariants.Contains(x.Value.ToString()))
                .FirstOrDefault();

            return cell?.Start.Column ?? -1;

            //for (int col = 1; col <= maxCol; col++)
            //{
            //    var cellValue = sheet.Cells[headerRowIndex, col].Value;

            //    if (cellValue == null)
            //        continue;

            //    var cellText = cellValue.ToString();
            //    if ((cellText == name) || (cellText == altName))
            //        return col;
            //}
            //return -1;
        }

        int FindColumnIndexByHeaderName(IColumnProperties columnProperties)
            => FindColumnIndexByHeaderName(new[] { columnProperties.Name, columnProperties.AltName });

        int FindColumnIndexByHeaderName(string columnName)
        {
            var columnProperty = GetColumnProperty(columnName);

            return columnProperty == null ? FindColumnIndexByHeaderName(new[] { columnName }) : FindColumnIndexByHeaderName(columnProperty);
        }

        void ReadPatients()
        {
            for (int i = 0; i < patientsInFile.Length; i++)
            {
                var sheetRow = i + headerRowIndex + 1;

                var patient = new Patient
                {
                    InsuranceNumber = sheet.Cells[sheetRow, insuranceColumn].Value.ToString(),
                    Initials = sheet.Cells[sheetRow, initialsColumn].Value.ToString()
                };

                if (sheet.Cells[sheetRow, surnameColumn].Value != null && sheet.Cells[sheetRow, nameColumn].Value != null)
                {
                    patient.Surname = sheet.Cells[sheetRow, surnameColumn].Value.ToString();
                    patient.Name = sheet.Cells[sheetRow, nameColumn].Value.ToString();
                    patient.Patronymic = sheet.Cells[sheetRow, patronymicColumn].Value.ToString();
                    patient.FullNameExist = true;
                }

                patientsInFile[i] = patient;
            }
        }

        /// <summary>
        /// Cохраняет изменения в файл.
        /// </summary>
        public void Save()
        {
            //Задаем всему листу прозрачную заливку, т.к. EpPlus в некоторых случаях устанавливает серую заливку.
            sheet.Cells.Style.Fill.PatternType = ExcelFillStyle.None;

            excel.Save();
        }

        void WritePatient(int rowIndex, Patient patient)
        {
            if (sheet.Cells[rowIndex, surnameColumn].Value == null && patient.FullNameExist)
            {
                sheet.Cells[rowIndex, surnameColumn].Value = patient.Surname;
                sheet.Cells[rowIndex, nameColumn].Value = patient.Name;
                sheet.Cells[rowIndex, patronymicColumn].Value = patient.Patronymic;
            }
        }

        /// <summary>
        /// Получает список серии и/или номера полиса пациентов без полных ФИО
        /// </summary>
        /// <returns>Список серии и/или номера полиса пациентов без полных ФИО</returns>
        public List<string> GetInsuranceNumberOfPatientsWithoutFullName()
            => patientsInFile.Where(x => !x.FullNameExist).Select(x => x.InsuranceNumber).ToList();

        /// <summary>
        /// Вставляет полные ФИО в файл.
        /// </summary>
        /// <param name="patients">Коллекиця сведений о пациентах.</param>
        public void InsertPatientsWithFullName(IEnumerable<Patient> patients)
        {
            var patientsToAdd = patients.ToDictionary(x => x.insuranceNumber, x => x);

            for (int i = 0; i < patientsInFile.Length; i++)
            {
                if (patientsInFile[i].FullNameExist)
                    continue;

                patientsToAdd.TryGetValue(patientsInFile[i].InsuranceNumber, out var patientToAdd);

                if (patientToAdd != null && patientsInFile[i].Initials == patientToAdd.Initials)
                {
                    WritePatient(i + headerRowIndex, patientToAdd);
                    patientsInFile[i] = patientToAdd;
                }
            }
        }

        /// <summary>
        /// Применяет форматирования к файлу в соотвествии с настройками свойств столбцов
        /// </summary>
        public void Format()
        {
            ApplyColumnProperty();
            ApplyColumnsOrder();
            RenameSexColumn();
            sheet.Cells.AutoFitColumns();
            sheet.Cells[sheet.Dimension.Address].AutoFilter = true;
            SetColumnsIndexes();
        }

        void ApplyColumnProperty()
        {
            for (int i = 1; i <= maxCol; i++)
            {
                var cellValue = sheet.Cells[headerRowIndex, i].Value;

                if (cellValue == null)
                    continue;

                var columnProperty = GetColumnProperty(cellValue.ToString());

                if (columnProperty == null)
                    continue;

                if (!string.IsNullOrEmpty(columnProperty.AltName))
                    sheet.Cells[headerRowIndex, i].Value = columnProperty.AltName;

                if (columnProperty.Hide)
                    sheet.Column(i).Hidden = true;

                if (columnProperty.Delete)
                {
                    sheet.DeleteColumn(i);
                    maxCol--;
                    i--;
                }
            }
        }

        void ApplyColumnsOrder()
        {
            int mustBeIndex = 1;
            var tempsheet = excel.Workbook.Worksheets.Add("tempSheet");

            foreach (var columnProperty in columnProperties)
            {
                var actualIndex = FindColumnIndexByHeaderName(columnProperty);

                if (actualIndex == -1)
                    continue;

                else if (mustBeIndex != actualIndex)
                {
                    var propertyRange = sheet.Cells[headerRowIndex, actualIndex, maxRow, actualIndex];
                    var notInPlaceRange = sheet.Cells[headerRowIndex, mustBeIndex, maxRow, mustBeIndex];
                    var tempRange = tempsheet.Cells[headerRowIndex, mustBeIndex, maxRow, mustBeIndex];

                    notInPlaceRange.Copy(tempRange);
                    propertyRange.Copy(notInPlaceRange);
                    tempRange.Copy(propertyRange);
                    tempRange.Clear();
                }

                mustBeIndex++;                
            }

            excel.Workbook.Worksheets.Delete("tempSheet");
        }

        void RenameSexColumn()
        {
            int sexColumn = FindColumnIndexByHeaderName("SEX");

            if (sexColumn == -1)
                return;

            var cells = sheet.Cells[headerRowIndex + 1, sexColumn, maxRow, sexColumn];

            foreach (var cell in cells.Where(x => x.Value != null))
            {
                cell.Value = cell.Value.ToString() switch
                {
                    "1" => "Мужской",
                    "2" => "Женский",
                    _ => cell.Value
                };
            }
        }
        
        public void Dispose()
        {
            excel?.Dispose();
        }
    }
}
