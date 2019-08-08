using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Models
{
    /// <summary>
    /// Работа с excel файлом пациентов
    /// </summary>
    public class PatientsFile : IDisposable
    {
        #region Поля
        private static readonly object locker = new object();
        private ExcelPackage excel;
        private ExcelWorksheet sheet;
        private ColumnProperty[] columnProperties;
        private int maxRow;
        private int maxCol;
        private int headerIndex = 1;
        private int insuranceColumn;
        private int initialsColumn;
        private int surnameColumn;
        private int nameColumn;
        private int patronymicColumn;
        #endregion

        #region Методы
        //возвращает альтернативное название столбца, если синонима нет возвращает это же название
        private string GetColumnAlternativeName(string columnName)
        {
            string altName = columnProperties.Where(x => x.Name == columnName).FirstOrDefault()?.AltName;

            if (string.IsNullOrEmpty(altName))
                altName = columnName;

            return altName;
        }
        //возвращает атрибуты столбца по имени, если такого нет - возвращает новый экземпляр с таким именем
        private ColumnProperty GetColumnProperty(string name)
        {
            foreach (var attribute in columnProperties)
                if ((attribute.Name == name) || (attribute.AltName == name))
                    return attribute;

            return new ColumnProperty { Name = name, AltName = name, Hide = false, Delete = false };
        }
        //проверяет структуру файла, при необходимости добавляет столбцы Фамилия, Имя, Отчество
        private void CheckStructure()
        {
            if (insuranceColumn == -1)
                throw new Exception("Не найден столбец с номером полиса");

            if (surnameColumn == -1)
            {
                surnameColumn = initialsColumn + 1;
                sheet.InsertColumn(surnameColumn, 1);
                sheet.Cells[headerIndex, surnameColumn].Value = "Фамилия";
            }

            if (nameColumn == -1)
            {
                nameColumn = surnameColumn + 1;
                sheet.InsertColumn(nameColumn, 1);
                sheet.Cells[headerIndex, nameColumn].Value = "Имя";
            }

            if (patronymicColumn == -1)
            {
                patronymicColumn = nameColumn + 1;
                sheet.InsertColumn(patronymicColumn, 1);
                sheet.Cells[headerIndex, patronymicColumn].Value = "Отчество";
            }
        }
        //ищет номер столбца по заголовку или его алтернативному названию, если столбец не найден возвращает -1
        private int GetColumnIndex(string columnName)
        {
            for (int col = 1; col <= maxCol; col++)
            {
                var cellValue = sheet.Cells[headerIndex, col].Value;

                if (cellValue == null)
                    continue;

                string cellText = cellValue.ToString();
                string altHeaderName = GetColumnAlternativeName(columnName);
                if (cellText == columnName || cellText == altHeaderName)
                    return col;
            }

            return -1;
        }
        //ищет номер столбца по заголовку или его алтернативному названию, если столбец не найден возвращает -1
        private int GetColumnIndex(ColumnProperty column)
        {
            for (int col = 1; col <= maxCol; col++)
            {
                var cellValue = sheet.Cells[headerIndex, col].Value;

                if (cellValue == null)
                    continue;

                var cellText = cellValue.ToString();
                if ((cellText == column.Name) || (cellText == column.AltName))
                    return col;
            }
            return -1;
        }
        //открывает файл
        public void Open(string filePath, ColumnProperty[] columnProperties = null)
        {
            excel = new ExcelPackage(new FileInfo(filePath));
            sheet = excel.Workbook.Worksheets[1];
            this.columnProperties = columnProperties ?? new ColumnProperty[0];
            maxRow = sheet.Dimension.Rows;
            maxCol = sheet.Dimension.Columns;
            insuranceColumn = GetColumnIndex("ENP");
            initialsColumn = GetColumnIndex("FIO");
            surnameColumn = GetColumnIndex("Фамилия");
            nameColumn = GetColumnIndex("Имя");
            patronymicColumn = GetColumnIndex("Отчество");

            CheckStructure();
        }
        //сохраняет изменения в файл
        public void Save()
        {
            excel.Save();
        }
        //добавляет фильтр
        public void SetAutoFilter()
        {
            sheet.Cells[sheet.Dimension.Address].AutoFilter = true;
        }
        //подстраивает ширину столбцов под содержимое
        public void FitColumnWidth()
        {
            sheet.Cells.AutoFitColumns();
        }
        //изменяет порядок столбоц
        //public async Task SetColumnsOrder()
        //    {
        //    await Task.Run(() =>
        //    {
        //        int insColPos = 1;
        //        int foundColPos;

        //        foreach (var columnSynonim in columnsAttributes)
        //            {
        //            foundColPos = FindColumnIndex(columnSynonim, 1);

        //            if (foundColPos == insColPos)
        //                insColPos++;
        //            else if (foundColPos != -1)
        //                {
        //                sheet.InsertColumn(insColPos, 1);
        //                foundColPos++;
        //                sheet.Cells[1, foundColPos, maxRow, foundColPos].Copy(sheet.Cells[1, insColPos, maxRow, insColPos]);
        //                sheet.DeleteColumn(foundColPos);
        //                insColPos++;
        //                }
        //            }
        //    });
        //    }

        //переименовывает цифры с полом в нормальные названия
        public void RenameSex()
        {
            int columnSex = GetColumnIndex("SEX");

            if (columnSex != -1)
                for (int i = 1; i <= maxRow; i++)
                {
                    if (sheet.Cells[i, columnSex].Value == null)
                        continue;

                    var str = sheet.Cells[i, columnSex].Value.ToString();
                    if (str == "1")
                        sheet.Cells[i, columnSex].Value = "Мужской";
                    else if (str == "2")
                        sheet.Cells[i, columnSex].Value = "Женский";
                }
        }
        //переименовывает названия столбцов в нормальные названия, скрывает и удаляет столбцы
        public void ProcessColumns()
        {
            for (int i = 1; i <= maxCol; i++)
            {
                if (sheet.Cells[1, i].Value == null)
                    continue;

                var name = sheet.Cells[1, i].Value.ToString();
                var synonim = GetColumnProperty(name);
                if (name != synonim.AltName)
                    sheet.Cells[1, i].Value = synonim.AltName;

                sheet.Column(i).Hidden = synonim.Hide;
                if (synonim.Delete)
                {
                    sheet.DeleteColumn(i);
                    maxCol--;
                    i--;
                }
            }
        }
        //Возвращае полиса паицентов без полных ФИО
        public string[] GetUnverifiedInsuaranceNumbersAsync(int limitCount)
        {
            if (initialsColumn == -1)
                throw new Exception("Не найден столбец с инициалами ФИО");

            var patients = new ConcurrentBag<string>();

            Parallel.For(headerIndex + 1, maxRow + 1, (row, state) =>
            {
                var insuranceValue = sheet.Cells[row, insuranceColumn].Value;
                var initialsValue = sheet.Cells[row, initialsColumn].Value;
                var surnameValue = sheet.Cells[row, surnameColumn].Value;

                if (insuranceValue != null && initialsValue != null && surnameValue == null)
                {
                    if (patients.Count < limitCount)
                        patients.Add(insuranceValue.ToString());
                    else
                        state.Break();
                }
            });

            //из-за асинхронного выполнения, размер стэка может получиться больше чем надо, выкидываем лишнее
            while (patients.Count > limitCount)
                patients.TryTake(out string _);

            return patients.ToArray();
        }
        //вставляет полные ФИО в файл
        public void SetFullNames(IEnumerable<Patient> cachedPatients)
        {
            Parallel.For(headerIndex + 1, maxRow + 1, (row, state) =>
            {
                object insuranceValue;
                object initialsValue;
                object surnameValue;

                lock (locker)
                {
                    insuranceValue = sheet.Cells[row, insuranceColumn].Value;
                    initialsValue = sheet.Cells[row, initialsColumn].Value;
                    surnameValue = sheet.Cells[row, surnameColumn].Value;
                }

                if (insuranceValue != null && initialsValue != null && surnameValue == null)
                {
                    var patient = cachedPatients.Where(x => x.InsuranceNumber == insuranceValue.ToString() && x.Initials == initialsValue.ToString()).FirstOrDefault();

                    if (patient != null)
                        lock (locker)
                        {
                            sheet.Cells[row, surnameColumn].Value = patient.Surname;
                            sheet.Cells[row, nameColumn].Value = patient.Name;
                            sheet.Cells[row, patronymicColumn].Value = patient.Patronymic;
                        }
                }
            });
        }
        public void Dispose()
        {
                sheet?.Dispose();
                excel?.Dispose();
        }
        #endregion
    }


}
