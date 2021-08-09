using CHI.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services
{
    public class ImportPatientsReaderService:IDisposable
    {
        /// <summary>
        /// Читает файл для загрузки пациентов в локальную БД и возвращает список сведений о них
        /// </summary>
        /// <param name="filePath">Путь к файлу импорта пациентов</param>
        /// <returns>Список сведений о пациентах</returns>
        public List<Patient> Read(string filePath)
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
        /// Ищет номер столбца в файле по названию заголовка
        /// </summary>
        /// <param name="columnName">Название столбца.</param>
        /// <param name="sheet">Ссылка на лист excel файла</param>
        /// <param name="headerIndex">Номер строки с заголовками.</param>
        /// <returns>Индекс искомого столбца, если столбец не найден возвращает -1.</returns>
        int GetColumnIndex(string columnName, ExcelWorksheet sheet, int headerIndex)
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
        /// Генерирует и сохраняет пример файла для загрузки пациентов в локальную БД
        /// </summary>
        /// <param name="path">Путь к файлу</param>
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
