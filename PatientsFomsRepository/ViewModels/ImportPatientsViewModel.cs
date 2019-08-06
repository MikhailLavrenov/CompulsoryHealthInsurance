using FomsPatientsDB.Models;
using OfficeOpenXml;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
{
    class ImportPatientsViewModel : BindableBase, IViewModel
    {
        #region Поля
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public RelayCommand ImportCommand { get; }
        public RelayCommand GetExampleCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
            ShortCaption = "Загрузить из excel";
            FullCaption = "Загрузить известные ФИО из excel в базу данных";
            ImportCommand = new RelayCommand(ImportExecute, ImportCanExecute);
            GetExampleCommand = new RelayCommand(GetExampleExecute);

        }
        #endregion

        #region Методы
        private void GetExampleExecute(object parameter)
        {
            string destinationFile = parameter as string;
            var excel = new ExcelPackage();
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
            excel.SaveAs(new FileInfo(destinationFile));
        }
        private async void ImportExecute(object parameter)
        {
            string filePath = (string)parameter;
            var columnsProperty = Settings.Instance.ColumnProperties.ToArray();

            var patientsFile = new PatientsFile();
            await patientsFile.Open(filePath, columnsProperty);
            var newPatients = await patientsFile.GetVerifedPatientsAsync();

            var db = new Models.Database();
            db.Patients.Load();
            var existenInsuaranceNumbers = db.Patients.Select(x => x.InsuranceNumber).ToHashSet();
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();
        }
        private bool ImportCanExecute(object parameter)
        {
            string filePath = parameter as string;
            return File.Exists(filePath);
        }
        #endregion
    }
}
