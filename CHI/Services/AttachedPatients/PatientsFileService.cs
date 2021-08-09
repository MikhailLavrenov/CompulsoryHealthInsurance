using CHI.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.Services.AttachedPatients
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
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <param name="columnProperties">Коллекиця настроиваемых свойств столбоц файла.</param>
        public PatientsFileService(string filePath, IEnumerable<IColumnProperties> columnProperties)
        {

            excel = new ExcelPackage(new FileInfo(filePath));
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
            insuranceColumn = GetColumnIndex("ENP");
            initialsColumn = GetColumnIndex("FIO");
            surnameColumn = GetColumnIndex("Фамилия");
            nameColumn = GetColumnIndex("Имя");
            patronymicColumn = GetColumnIndex("Отчество");

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
            sheet.InsertColumn(surnameColumn, 1);
            sheet.Cells[headerRowIndex, surnameColumn].Value = columnName;
            maxCol++;

            return columnIndex;
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
        /// <param name="limitCount">Предельное кол-во возвращаемого списка</param>
        /// <returns>Список серии и/или номера полиса пациентов без полных ФИО</returns>
        public List<string> GetUnknownInsuaranceNumbers(long limitCount)
        {
            return patientsInFile.Where(x => !x.FullNameExist).Take((int)limitCount).Select(x => x.InsuranceNumber).ToList();
        }

        /// <summary>
        /// Вставляет полные ФИО в файл.
        /// </summary>
        /// <param name="patients">Коллекиця сведений о пациентах.</param>
        public void AddPatientsWithFullName(IEnumerable<Patient> patients)
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
            SetColumnsOrder();
            RenameSex();
            sheet.Cells.AutoFitColumns();
            sheet.Cells[sheet.Dimension.Address].AutoFilter = true;

            SetColumnsIndexes();
        }

        /// <summary>
        /// Ищет номер столбца в файле по названию заголовка.
        /// </summary>
        /// <param name="columnName">Название столбца</param>
        /// <returns>Индекс искомого столбца, если столбец не найден возвращает -1.</returns>
        int GetColumnIndex(string columnName)
        {
            var columnProperty = columnProperties
                .Where(x => string.Equals(x.Name, columnName, StringComparison.OrdinalIgnoreCase) || string.Equals(x.AltName, columnName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (columnProperty == null)
                return GetColumnIndex(columnName, columnName);

            return GetColumnIndex(columnProperty.Name, columnProperty.AltName);
        }

        /// <summary>
        /// Ищет номер столбца в файле по названию заголовка или его алтернативному названию.
        /// </summary>
        /// <param name="name">Название столбца.</param>
        /// <param name="altName">Альтернативное название столбца.</param>
        /// <returns>Индекс искомого столбца, если столбец не найден возвращает -1.</returns>
        int GetColumnIndex(string name, string altName)
        {
            for (int col = 1; col <= maxCol; col++)
            {
                var cellValue = sheet.Cells[headerRowIndex, col].Value;

                if (cellValue == null)
                    continue;

                var cellText = cellValue.ToString();
                if ((cellText == name) || (cellText == altName))
                    return col;
            }
            return -1;
        }

        /// <summary>
        /// возвращает атрибуты столбца по имени, если такого нет - null
        /// </summary>
        /// <param name="name">Имя столбца</param>
        /// <returns>Атрибуты столбца</returns>
        IColumnProperties GetColumnProperty(string name)
            => columnProperties.Where(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) || string.Equals(x.AltName, name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

        /// <summary>
        /// Изменяет порядок столбоцов в соответствии с порядком следования свойств настроек столбцов.
        /// </summary>
        void SetColumnsOrder()
        {
            int correctIndex = 1;

            foreach (var columnProperty in columnProperties)
            {
                var currentIndex = GetColumnIndex(columnProperty.Name, columnProperty.AltName);

                //если столбец на своем месте 
                if (currentIndex == correctIndex)
                    correctIndex++;
                //если столбец не на своем месте и столбец найден в таблице
                else if (currentIndex != -1)
                {
                    sheet.InsertColumn(correctIndex, 1);
                    currentIndex++;
                    var currentRange = sheet.Cells[headerRowIndex, currentIndex, maxRow, currentIndex];
                    var correctRange = sheet.Cells[headerRowIndex, correctIndex, maxRow, correctIndex];
                    currentRange.Copy(correctRange);
                    sheet.DeleteColumn(currentIndex);
                    correctIndex++;
                }
            }
        }

        /// <summary>
        /// Заменяет цифры с полом в понятные названия
        /// </summary>
        void RenameSex()
        {
            int sexColumn = GetColumnIndex("SEX");

            if (sexColumn == -1)
                return;

            var cells = sheet.Cells[headerRowIndex + 1, sexColumn, maxRow, sexColumn]
                .Where(x => x.Value != null);

            foreach (var cell in cells)
                cell.Value = cell.Value.ToString()
                    .Replace("1", "Мужской")
                    .Replace("2", "Женский");
        }

        /// <summary>
        /// Применяет свойства столбцов к таблице в соотвествии с настройками свойств столбцов:
        /// заменяет названия столбцов на настроенные, скрывает и удаляет столбцы.
        /// </summary>
        void ApplyColumnProperty()
        {
            for (int i = 1; i <= maxCol; i++)
            {
                var cellValue = sheet.Cells[headerRowIndex, i].Value;

                if (cellValue == null)
                    continue;

                var name = cellValue.ToString();
                var columnProperty = GetColumnProperty(name);

                if (columnProperty?.AltName != string.Empty)
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

        public void Dispose()
        {
            excel?.Dispose();
        }
    }
}
