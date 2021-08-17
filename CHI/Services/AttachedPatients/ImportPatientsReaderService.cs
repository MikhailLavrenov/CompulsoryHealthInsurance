using CHI.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.Services
{
    /// <summary>
    /// Загружает данные пациентов из файла импорта
    /// </summary>
    public class ImportPatientsFileService : IDisposable
    {
        int rowHeaderIndex = 1;
        ExcelPackage excel;
        ExcelWorksheet sheet;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">Путь к файлу импорта пациентов</param>
        public ImportPatientsFileService(string filePath)
        {
            excel = new ExcelPackage(new FileInfo(filePath));
            sheet = excel.Workbook.Worksheets.First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Список сведений о пациентах</returns>
        public List<Patient> GetPatients()
        {
            var patients = new List<Patient>();

            var insuranceColumn = FindColumnIndexByHeaderName("Полис");
            var surnameColumn = FindColumnIndexByHeaderName("Фамилия");
            var nameColumn = FindColumnIndexByHeaderName("Имя");
            var patronymicColumn = FindColumnIndexByHeaderName("Отчество");

            //проверяем структуру файла
            if (insuranceColumn == -1)
                throw new InvalidOperationException("Не найден столбец  \"Полис\"");
            if (surnameColumn == -1)
                throw new InvalidOperationException("Не найден столбец  \"Фамилия\"");
            if (nameColumn == -1)
                throw new InvalidOperationException("Не найден столбец  \"Имя\"");
            if (patronymicColumn == -1)
                throw new InvalidOperationException("Не найден столбец  \"Отчество\"");

            for (int row = rowHeaderIndex + 1; row < sheet.Dimension.Rows; row++)
            {
                var insurance = sheet.Cells[row, insuranceColumn].Value;
                var surname = sheet.Cells[row, surnameColumn].Value;
                var name = sheet.Cells[row, nameColumn].Value;
                var patronymic = sheet.Cells[row, patronymicColumn].Value ?? string.Empty;

                if (insurance is null || surname is null || name is null)
                    continue;

                var patient = new Patient
                {
                    InsuranceNumber = ToStringAndFormat(insurance),
                    Surname = ToStringAndFormat(surname),
                    Name = ToStringAndFormat(name),
                    Patronymic = ToStringAndFormat(patronymic),
                    FullNameExist = true
                };
                patient.DefineInitilas();

                patients.Add(patient);
            }

            return patients;
        }

        int FindColumnIndexByHeaderName(string columnName)
        {
            var cell = sheet.Cells[rowHeaderIndex, 1, rowHeaderIndex, sheet.Dimension.Columns]
                .Where(x => x.Value != null && x.Value.ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            return cell?.Start.Column ?? -1;
        }

        static string ToStringAndFormat(object text)
            => text.ToString().Replace(" ", "").Trim().ToUpper();

        public void Dispose()
        {
            excel?.Dispose();
        }

        /// <summary>
        /// Cохраняет пример файла для загрузки пациентов в локальную БД
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void SaveExample(string path)
        {
            using var excel = new ExcelPackage();

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
}
