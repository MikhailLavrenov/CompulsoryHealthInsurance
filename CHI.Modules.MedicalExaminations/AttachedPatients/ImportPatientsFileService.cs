using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace CHI.Services.AttachedPatients
{
    /// <summary>
    /// Работа с excel файлом для загрузки пациентов пациентов
    /// </summary>
    public class ImportPatientsFileService : IDisposable
    {
        #region Поля
        private ExcelPackage excel;
        private ExcelWorksheet sheet;
        private int maxRow;
        private int maxCol;
        private int headerIndex = 1;
        private int insuranceColumn;
        private int surnameColumn;
        private int nameColumn;
        private int patronymicColumn;
        #endregion

        #region Методы
        //открывает файл
        public void Open(string filePath)
        {
            excel = new ExcelPackage(new FileInfo(filePath));
            sheet = excel.Workbook.Worksheets[1];

            maxRow = sheet.Dimension.Rows;
            maxCol = sheet.Dimension.Columns;
            insuranceColumn = GetColumnIndex("Полис");
            surnameColumn = GetColumnIndex("Фамилия");
            nameColumn = GetColumnIndex("Имя");
            patronymicColumn = GetColumnIndex("Отчество");

            CheckStructure();
        }
        //преобразует строки из файла в список пациентов
        public List<Patient> GetPatients()
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
        }
        //Сорханяет пример файла для загрузки
        public static void SaveExample(string path)
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
        //освобождает неуправляемые ресурсы
        public void Dispose()
        {
            sheet?.Dispose();
            excel?.Dispose();
        }
        //проверяет структуру файла, вызывает исключение если структура не правильная
        private void CheckStructure()
        {
            if (insuranceColumn == -1)
                throw new Exception("Не найден столбец  \"Полис\"");

            if (surnameColumn == -1)
                throw new Exception("Не найден столбец  \"Фамилия\"");

            if (nameColumn == -1)
                throw new Exception("Не найден столбец  \"Имя\"");

            if (patronymicColumn == -1)
                throw new Exception("Не найден столбец  \"Отчество\"");
        }
        //ищет номер столбца по названию заголовка, если столбец не найден возвращает -1
        private int GetColumnIndex(string columnName)
        {
            for (int col = 1; col <= maxCol; col++)
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
        #endregion

    }


}
