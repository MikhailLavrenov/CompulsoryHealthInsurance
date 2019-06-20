using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace FomsPatientsDB.Models
    {
    /// <summary>
    /// Работа с excel файлом пациентов
    /// </summary>
    public class PatientsFile : IDisposable
        {
        private static readonly object lockSync = new object();
        private ExcelPackage excel;
        private ExcelWorksheet sheet;
        private ColumnAttribute[] columnsAttributes;
        private int maxRow;
        private int maxCol;
        private int headerIndex=1;                
        private int insuranceColumn;
        private int initialsColumn;
        private int surnameColumn;
        private int nameColumn;
        private int patronymicColumn;      

        //открывает файл
        public async Task Open(string filePath, ColumnAttribute[] columnsAttributes = null)
            {
            await Task.Run(() =>
                {
                    excel = new ExcelPackage(new FileInfo(filePath));
                    sheet = excel.Workbook.Worksheets[1];
                    this.columnsAttributes = columnsAttributes??new ColumnAttribute[0];
                    maxRow = sheet.Dimension.Rows;
                    maxCol = sheet.Dimension.Columns;
                    insuranceColumn = GetColumnIndex("ENP");
                    initialsColumn = GetColumnIndex("FIO");
                    surnameColumn = GetColumnIndex("Фамилия");
                    nameColumn = GetColumnIndex("Имя");
                    patronymicColumn = GetColumnIndex("Отчество");                   

                    CheckStructure();
                });


            }

        //сохраняет изменения в файл
        public async Task Save()
            {
            await Task.Run(() => excel.Save());
            }

        //добавляет фильтр
        public async Task SetAutoFilter()
            {
            await Task.Run(() => sheet.Cells[sheet.Dimension.Address].AutoFilter = true);
            }

        //подстраивает ширину столбцов под содержимое
        public async Task FitColumnWidth()
            {
            await Task.Run(() => sheet.Cells.AutoFitColumns());
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
        public async Task RenameSex()
            {
            await Task.Run(() =>
            {
                string str;
                int columnSex = GetColumnIndex("SEX");

                if (columnSex != -1)
                    for (int i = 1; i <= maxRow; i++)
                        {
                        if (sheet.Cells[i, columnSex].Value == null)
                            continue;

                        str = sheet.Cells[i, columnSex].Value.ToString();
                        if (str == "1")
                            sheet.Cells[i, columnSex].Value = "Мужской";
                        else if (str == "2")
                            sheet.Cells[i, columnSex].Value = "Женский";
                        }
            });
            }

        //переименовывает названия столбцов в нормальные названия, скрывает и удаляет столбцы
        public async Task ProcessColumns()
            {
            await Task.Run(() =>
            {
                string name;
                ColumnAttribute synonim;

                for (int i = 1; i <= maxCol; i++)
                    {
                    if (sheet.Cells[1, i].Value == null)
                        continue;

                    name = sheet.Cells[1, i].Value.ToString();
                    synonim = GetColumnAttribute(name);
                    if (name != synonim.alternativeName)
                        sheet.Cells[1, i].Value = synonim.alternativeName;

                    sheet.Column(i).Hidden = synonim.hide;
                    if (synonim.delete)
                        {
                        sheet.DeleteColumn(i);
                        maxCol--;
                        i--;
                        }
                    }
            });
            }

        //преобразует строки из файла в список пациентов
        public async Task<List<Patient>> GetVerifedPatients()
            {
            return await Task.Run(() =>
            {
                var patients = new List<Patient>();
                for (int row = headerIndex + 1; row < maxRow; row++)
                    {
                    var insuranceValue = sheet.Cells[row, insuranceColumn].Value;
                    var surnameValue = sheet.Cells[row, surnameColumn].Value;
                    var nameValue = sheet.Cells[row, nameColumn].Value;
                    var patronymicValue = sheet.Cells[row, patronymicColumn].Value ?? "";

                    if (insuranceValue != null && surnameValue != null && nameValue != null)
                        {
                        var patient = new Patient(insuranceValue.ToString(), surnameValue.ToString(), nameValue.ToString(), patronymicValue.ToString());
                        patient.Normalize();

                        patients.Add(patient);
                        }
                    }

                return patients;
            });
            }

        //Находит строки без полных ФИО
        public async Task<ConcurrentStack<string>> GetUnverifiedPatientsInsuaranceNumber(int limitCount)
            {
            if (initialsColumn == -1)
                throw new Exception("Не найден столбец с инициалами ФИО");

            return await Task.Run(() =>
            {
                var patients = new ConcurrentStack<string>();

                Parallel.For(headerIndex + 1, maxRow + 1, (row, state) =>
                {
                    var insuranceValue = sheet.Cells[row, insuranceColumn].Value;
                    var initialsValue = sheet.Cells[row, initialsColumn].Value;
                    var surnameValue = sheet.Cells[row, surnameColumn].Value;

                    if (insuranceValue != null && initialsValue != null && surnameValue == null)
                        {                        
                        if (patients.Count < limitCount)
                            patients.Push(insuranceValue.ToString());
                        else
                            state.Break();
                        }
                });

                //из-за асинхронного выполнения, размер стэка может получиться больше чем надо, выкидываем лишнее
                while (patients.Count > limitCount)
                    patients.TryPop(out string _);

                return patients;
            });
            }

        //вставляет полные ФИО в файл
        public async Task SetFullNames(List<Patient> cachedPatients)
            {
            await Task.Run(() =>
                {
                    Parallel.For(headerIndex + 1, maxRow + 1, (row, state) =>
                        {
                            var insuranceValue = sheet.Cells[row, insuranceColumn].Value;
                            var initialsValue = sheet.Cells[row, initialsColumn].Value;
                            var surnameValue = sheet.Cells[row, surnameColumn].Value;

                            if (insuranceValue != null && initialsValue != null && surnameValue == null)
                                {
                                var patient = cachedPatients.Where(x => x.InsuranceNumber == insuranceValue.ToString() && x.Initials == initialsValue.ToString()).FirstOrDefault();

                                if (patient != null)
                                    lock (lockSync)
                                        {
                                        sheet.Cells[row, surnameColumn].Value = patient.Surname;
                                        sheet.Cells[row, nameColumn].Value = patient.Name;
                                        sheet.Cells[row, patronymicColumn].Value = patient.Patronymic;
                                        }
                                }
                        });
                });
            }


        public void Dispose()
            {
            if (sheet != null)
                sheet.Dispose();
            if (excel != null)
                excel.Dispose();
            }


        //возвращает альтернативное название столбца, если синонима нет возвращает это же название
        private string GetColumnAlternativeName(string columnName)
            {
            string altName = columnsAttributes.Where(x => x.name == columnName).FirstOrDefault().alternativeName;

            if (altName == null)
                altName = columnName;

            return altName;
            }

        //возвращает атрибуты столбца по имени, если такого нет - возвращает новый экземпляр с таким именем
        private ColumnAttribute GetColumnAttribute(string name)
            {
            foreach (var attribute in columnsAttributes)
                if ((attribute.name == name) || (attribute.alternativeName == name))
                    return attribute;

            return new ColumnAttribute { name = name, alternativeName = name, hide = false, delete = false };
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
        private int GetColumnIndex(ColumnAttribute column)
            {
            for (int col = 1; col <= maxCol; col++)
                {
                var cellValue = sheet.Cells[headerIndex, col].Value;

                if (cellValue == null)
                    continue;

                var cellText = cellValue.ToString();
                if ((cellText == column.name) || (cellText == column.alternativeName))
                    return col;
                }
            return -1;
            }


        /// <summary>
        /// Аттрибуты столбца файла пациентов
        /// </summary>
        [Serializable]
        public struct ColumnAttribute
            {
            public string name;
            public string alternativeName;
            public bool hide;
            public bool delete;
            }
        }


    }
