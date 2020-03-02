using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.Services.AttachedPatients
{
    /// <summary>
    /// Представляет сервис для работа с файлом прикрепленных пациентов
    /// </summary>
    public class PatientsFileService : IDisposable
    {
        #region Поля
        private ExcelPackage excel;
        private ExcelWorksheet sheet;
        private List<IColumnProperties> columnProperties;
        private int maxRow;
        private int maxCol;
        private int headerIndex = 1;
        private int insuranceColumn;
        private int initialsColumn;
        private int surnameColumn;
        private int nameColumn;
        private int patronymicColumn;
        //Кэширует данные пациентов из файла для увеличения производительности
        private Patient[] patients;
        private bool patientsChanged = false;
        #endregion

        #region Методы
        /// <summary>
        /// Открывает файл прикрепленных пациентов
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <param name="columnProperties">Коллекиця настроиваемых свойств столбоц файла.</param>
        public void Open(string filePath, IEnumerable<IColumnProperties> columnProperties)
        {
            excel = new ExcelPackage(new FileInfo(filePath));
            sheet = excel.Workbook.Worksheets[1];
            this.columnProperties = columnProperties.ToList();
            maxRow = sheet.Dimension.Rows;
            maxCol = sheet.Dimension.Columns;

            SetColumnsIndexes();
            SetPatients();
        }
        /// <summary>
        /// Находит столбцы в файле и устанавливает их индексы. При необходимости может добавлять отсутствующие столбцы.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает в случае если невозможно исправить структуру файла.</exception>
        private void SetColumnsIndexes()
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

            if (surnameColumn == -1)
            {
                surnameColumn = initialsColumn + 1;
                sheet.InsertColumn(surnameColumn, 1);
                sheet.Cells[headerIndex, surnameColumn].Value = "Фамилия";
                maxCol++;
            }

            if (nameColumn == -1)
            {
                nameColumn = surnameColumn + 1;
                sheet.InsertColumn(nameColumn, 1);
                sheet.Cells[headerIndex, nameColumn].Value = "Имя";
                maxCol++;
            }

            if (patronymicColumn == -1)
            {
                patronymicColumn = nameColumn + 1;
                sheet.InsertColumn(patronymicColumn, 1);
                sheet.Cells[headerIndex, patronymicColumn].Value = "Отчество";
                maxCol++;
            }
        }
        /// <summary>
        /// Создает масив типа Patient соответствующий файлу прикрепленных пациентов
        /// </summary>
        private void SetPatients()
        {
            patients = new Patient[maxRow - headerIndex];

            for (int i = 0; i < patients.Length; i++)
            {
                var sheetRow = i + headerIndex + 1;

                var patient = new Patient
                {
                    InsuranceNumber = sheet.Cells[sheetRow, insuranceColumn].Value.ToString(),
                    Initials = sheet.Cells[sheetRow, initialsColumn].Value.ToString()
                };

                if (sheet.Cells[sheetRow, surnameColumn].Value != null)
                {
                    patient.Surname = sheet.Cells[sheetRow, surnameColumn].Value.ToString();
                    patient.Name = sheet.Cells[sheetRow, nameColumn].Value.ToString();
                    patient.Patronymic = sheet.Cells[sheetRow, patronymicColumn].Value.ToString();
                    patient.FullNameExist = true;
                }

                patients[i] = patient;
            }
        }
        /// <summary>
        /// Cохраняет изменения в файле
        /// </summary>
        public void Save()
        {
            WritePatientsToFile();

            //Задаем всему листу прозрачную заливку, т.к. EpPlus в некоторых случаях устанавливает серую заливку.
            sheet.Cells.Style.Fill.PatternType = ExcelFillStyle.None;

            excel.Save();
        }
        /// <summary>
        /// Записывает ФИО кэшированных пациентов в файл если были изменения с момента последней записи
        /// </summary>
        private void WritePatientsToFile()
        {
            if (patientsChanged)
            {
                for (int i = 0; i < patients.Length; i++)
                {
                    var sheetRow = i + headerIndex + 1;

                    if (sheet.Cells[sheetRow, surnameColumn].Value == null && patients[i].FullNameExist)
                    {
                        sheet.Cells[sheetRow, surnameColumn].Value = patients[i].Surname;
                        sheet.Cells[sheetRow, nameColumn].Value = patients[i].Name;
                        sheet.Cells[sheetRow, patronymicColumn].Value = patients[i].Patronymic;
                    }
                }

                patientsChanged = false;
            }
        }
        /// <summary>
        /// Получает список серии и/или номера полиса пациентов без полных ФИО
        /// </summary>
        /// <param name="limitCount">Предельное кол-во возвращаемого списка</param>
        /// <returns>Список серии и/или номера полиса пациентов без полных ФИО</returns>
        public List<string> GetUnknownInsuaranceNumbers(long limitCount)
        {
            return patients.Where(x => !x.FullNameExist).Take((int)limitCount).Select(x => x.InsuranceNumber).ToList();
        }
        /// <summary>
        /// Вставляет полные ФИО в файл.
        /// </summary>
        /// <param name="patientsWithFullName">Коллекиця сведений о пациентах.</param>
        public void AddFullNames(IEnumerable<Patient> patientsWithFullName)
        {
            var fullNamePatients = new Dictionary<string, Patient>();

            foreach (var item in patientsWithFullName)
                fullNamePatients.Add(item.InsuranceNumber, item);

            for (int i = 0; i < patients.Length; i++)
            {
                if (patients[i].FullNameExist)
                    continue;

                fullNamePatients.TryGetValue(patients[i].InsuranceNumber, out var fullNamePatient);

                if (fullNamePatient != null && patients[i].Initials == fullNamePatient.Initials)
                    patients[i] = fullNamePatient;
            }

            patientsChanged = true;
        }
        /// <summary>
        /// Применяет форматирования к файлу в соотвествии с настройками свойств столбцов
        /// </summary>
        public void Format()
        {
            WritePatientsToFile();

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
        private int GetColumnIndex(string columnName)
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
        private int GetColumnIndex(string name, string altName)
        {
            for (int col = 1; col <= maxCol; col++)
            {
                var cellValue = sheet.Cells[headerIndex, col].Value;

                if (cellValue == null)
                    continue;

                var cellText = cellValue.ToString();
                if ((cellText == name) || (cellText == altName))
                    return col;
            }
            return -1;
        }
        /// <summary>
        /// Ищет номер столбца в файле по названию заголовка
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="sheet">Ссылка на лист excel файла</param>
        /// <param name="headerIndex">Номер строки с заголовками.</param>
        /// <returns>Индекс искомого столбца, если столбец не найден возвращает -1.</returns>
        private static int GetColumnIndex(string columnName, ExcelWorksheet sheet, int headerIndex)
        {
            for (int col = 1; col <= sheet.Dimension.Columns; col++)
            {
                var cellValue = sheet.Cells[headerIndex, col].Value;

                if (cellValue == null)
                    continue;

                string cellText = cellValue.ToString();

                if (cellText == columnName)
                    return col;
            }

            return -1;
        }
        /// <summary>
        /// возвращает атрибуты столбца по имени, если такого нет - null
        /// </summary>
        /// <param name="name">Имя столбца</param>
        /// <returns>Атрибуты столбца</returns>
        private IColumnProperties GetColumnProperty(string name)
        {
            foreach (var attribute in columnProperties)
                if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase) || string.Equals(attribute.AltName, name, StringComparison.OrdinalIgnoreCase))
                    return attribute;

            return null;
        }
        /// <summary>
        /// Изменяет порядок столбоцов в соотвествии с порядком следования свойств настроек столбцов.
        /// </summary>
        private void SetColumnsOrder()
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
                    var currentRange = sheet.Cells[headerIndex, currentIndex, maxRow, currentIndex];
                    var correctRange = sheet.Cells[headerIndex, correctIndex, maxRow, correctIndex];
                    currentRange.Copy(correctRange);
                    sheet.DeleteColumn(currentIndex);
                    correctIndex++;
                }
            }
        }
        /// <summary>
        /// Заменяет цифры с полом в понятные названия
        /// </summary>
        private void RenameSex()
        {
            int sexColumn = GetColumnIndex("SEX");

            if (sexColumn == -1)
                return;

            var cells = sheet.Cells[headerIndex + 1, sexColumn, maxRow, sexColumn]
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
        private void ApplyColumnProperty()
        {
            for (int i = 1; i <= maxCol; i++)
            {
                var cellValue = sheet.Cells[headerIndex, i].Value;

                if (cellValue == null)
                    continue;

                var name = cellValue.ToString();
                var columnProperty = GetColumnProperty(name);

                if (columnProperty?.AltName != string.Empty)
                    sheet.Cells[headerIndex, i].Value = columnProperty.AltName;

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
        /// <summary>
        /// Читает файл для загрузки пациентов в локальную БД и возвращает список сведений о них
        /// </summary>
        /// <param name="filePath">Путь к файлу импорта пациентов</param>
        /// <returns>Список сведений о пациентах</returns>
        public static List<Patient> ReadImportPatientsFile(string filePath)
        {
            var patients = new List<Patient>();

            using (var excel = new ExcelPackage(new FileInfo(filePath)))
            using (var sheet = excel.Workbook.Worksheets.First())
            {
                var headerIndex = 1;
                var insuranceColumn = GetColumnIndex("Полис", sheet, headerIndex);
                var surnameColumn = GetColumnIndex("Фамилия", sheet, headerIndex);
                var nameColumn = GetColumnIndex("Имя", sheet, headerIndex);
                var patronymicColumn = GetColumnIndex("Отчество", sheet, headerIndex);

                //проверяем структуру файла
                if (insuranceColumn == -1)
                    throw new InvalidOperationException("Не найден столбец  \"Полис\"");
                if (surnameColumn == -1)
                    throw new InvalidOperationException("Не найден столбец  \"Фамилия\"");
                if (nameColumn == -1)
                    throw new InvalidOperationException("Не найден столбец  \"Имя\"");
                if (patronymicColumn == -1)
                    throw new InvalidOperationException("Не найден столбец  \"Отчество\"");

                for (int row = headerIndex + 1; row < sheet.Dimension.Rows; row++)
                {
                    var insurance = sheet.Cells[row, insuranceColumn].Value;
                    var surname = sheet.Cells[row, surnameColumn].Value;
                    var name = sheet.Cells[row, nameColumn].Value;
                    var patronymic = sheet.Cells[row, patronymicColumn].Value ?? string.Empty;

                    if (insurance == null || surname == null || name == null)
                        continue;

                    var patient = new Patient
                    {
                        InsuranceNumber = insurance.ToString().Replace(" ", "").ToUpper(),
                        Surname = surname.ToString().Replace("  ", " ").Trim().ToUpper(),
                        Name = name.ToString().Replace("  ", " ").Trim().ToUpper(),
                        Patronymic = patronymic.ToString().Replace("  ", " ").Trim().ToUpper(),
                        FullNameExist = true
                    };
                    patient.DefineInitilas();
                    patients.Add(patient);
                }
            }

            return patients;
        }
        /// <summary>
        /// Генерирует и сохраняет пример файла для загрузки пациентов в локальную БД
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void SaveImportFileExample(string path)
        {
            using (var excel = new ExcelPackage())
            {
                var sheet = excel.Workbook.Worksheets.Add("Лист1");

                var c = new[] {
                    new  { Polis="Полис",            Surname="Фамилия",      Name="Имя",        Patronymic="Отчество"       },
                    new  { Polis="2770260871000075", Surname="УДОЕНКО",      Name="СЕРГЕЙ",     Patronymic="ГРИГОРЬЕВИЧ"    },
                    new  { Polis="2793699740000035", Surname="СОБОЛЕВСКИЙ",  Name="ДМИТРИЙ",    Patronymic="ЮРЬЕВИЧ"        },
                    new  { Polis="2752930829000110", Surname="ГААС",         Name="ЕЛЕНА",      Patronymic="НИКОЛАЕВНА"     },
                    new  { Polis="2751640821000288", Surname="НАУМОВ",       Name="ВАСИЛИЙ",    Patronymic="ЕВГЕНЬЕВИЧ"     },
                    new  { Polis="2789589718000057", Surname="ИВАНОВА",      Name="ЕКАТЕРИНА",  Patronymic="СЕРГЕЕВНА"      },
                    new  { Polis="2776650874000037", Surname="ВЕСЕЛОВСКИЙ",  Name="СЕРГЕЙ",     Patronymic="ПЕТРОВИЧ"       },
                    new  { Polis="2773940823000086", Surname="ЗАХАРОВ",      Name="ТУРКАН",     Patronymic="ИВАНОВИЧ"       },
                    new  { Polis="2755500882000054", Surname="БУРДУКОВСКИЙ", Name="ЮРИЙ",       Patronymic="ФЕДОРОВИЧ"      },
                    new  { Polis="2757440881000383", Surname="ПАНАРИН",      Name="МАКСИМ",     Patronymic="ПЕТРОВИЧ"       },
                    new  { Polis="2748540839000021", Surname="САТЛЫКОВА",    Name="НАТАЛЬЯ",    Patronymic=""               },
                    new  { Polis="2775460894000111", Surname="СОРОКА",       Name="АРИНА",      Patronymic="ВЛАДИМИРОВНА"   },
                    new  { Polis="2749120870000112", Surname="БАЙДУРОВА",    Name="ИРИНА",      Patronymic="ЕВГЕНЬЕВНА"     },
                    new  { Polis="7948610823000076", Surname="КАПУСТИН",     Name="ДМИТРИЙ",    Patronymic="АЛЕКСАНДРОВИЧ"  },
                    new  { Polis="2754920842000294", Surname="БОРОДИН",      Name="АРКАДИЙ",    Patronymic="ЭРНЕСТОВИЧ"     },
                    new  { Polis="2749000826000046", Surname="КУЛИКОВА",     Name="ОКСАНА",     Patronymic="ЭРНЕСТОВНА"     },
                    new  { Polis="2777560875000119", Surname="ДОБРОВОЛЬНАЯ", Name="АНГЕЛИНА",   Patronymic="ДМИТРИЕВНА"     },
                    new  { Polis="2787489789000205", Surname="ДОБРОВОЛЬНАЯ", Name="ЕЛЕНА",      Patronymic="ВЯЧЕСЛАВОВНА"   },
                    new  { Polis="2770650824000125", Surname="МЕЩЕРЯКОВА",   Name="ЕКАТЕРИНА",  Patronymic="ДМИТРИЕВНА"     }
                };
                sheet.Cells.LoadFromCollection(c);
                sheet.Cells.AutoFitColumns();
                sheet.SelectedRange[1, 1, 1, 4].Style.Font.Bold = true;
                excel.SaveAs(new FileInfo(path));
            }
        }
        /// <summary>
        /// Освобождает неуправляемые ресурсы
        /// </summary>
        public void Dispose()
        {
            sheet?.Dispose();
            excel?.Dispose();
        }
        #endregion
    }
}
