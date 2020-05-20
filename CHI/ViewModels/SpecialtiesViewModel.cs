using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CHI.ViewModels
{
    public class SpecialtiesViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        ObservableCollection<Specialty> specialties;
        IMainRegionService mainRegionService;
        IFileDialogService fileDialogService;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Specialty> Specialties { get => specialties; set => SetProperty(ref specialties, value); }

        public DelegateCommandAsync LoadCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }


        public SpecialtiesViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;

            mainRegionService.Header = "Специальности";

            dbContext = new AppDBContext();
            dbContext.Specialties.Load();
            Specialties = dbContext.Specialties.Local.ToObservableCollection();

            LoadCommand = new DelegateCommandAsync(LoadExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
        }

        private void LoadExecute()
        {
            mainRegionService.ShowProgressBar("Выбор файла");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Excel files (*.xlsx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.HideProgressBar("Отменено");
                return;
            }

            mainRegionService.ShowProgressBar("Загрузка файла");

            using var excel = new ExcelPackage(new FileInfo(fileDialogService.FileName));
            var sheet = excel.Workbook.Worksheets.First();

            for (int i = 2; i <= sheet.Dimension.Rows; i++)
            {
                var loadedFomsId = int.Parse(sheet.Cells[i, 1].Value.ToString(), CultureInfo.InvariantCulture);
                var loadedName = sheet.Cells[i, 2].Value.ToString();

                var specialty = Specialties.FirstOrDefault(x => x.FomsId == loadedFomsId);

                if (specialty != null)
                    specialty.Name = loadedName;
            }

            dbContext.SaveChanges();

            dbContext =new AppDBContext();
            dbContext.Specialties.Load();
            Specialties = dbContext.Specialties.Local.ToObservableCollection();

            mainRegionService.HideProgressBar("Успешно загружено");
        }

        private void SaveExampleExecute()
        {
            mainRegionService.ShowProgressBar("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Пример для загрузки специальностей";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FileName;

            mainRegionService.ShowProgressBar("Сохранение файла");

            using var excel = new ExcelPackage();

            var sheet = excel.Workbook.Worksheets.Add("Лист1");

            sheet.Cells.LoadFromArrays(new string[][] { new[] { "Код ФОМС", "Специальность" } });

            var collection = new List<Tuple<int, string>> {

                new Tuple<int, string> ( 11,     "Терапевт"    ),
                new Tuple<int, string> ( 23,     "Хирург"      ),
                new Tuple<int, string> ( 16,     "Педиатр"     ),
                new Tuple<int, string> ( 57,     "Офтальмолог" ),
                new Tuple<int, string> ( 3,      "Гинеколог"   ),
                new Tuple<int, string> ( 55,     "Фельдшер"    ),
            };

            sheet.Cells[2, 1].LoadFromCollection(collection);
            sheet.Cells.AutoFitColumns();
            sheet.SelectedRange[1, 1, 1, 2].Style.Font.Bold = true;

            excel.SaveAs(new FileInfo(fileDialogService.FileName));

            mainRegionService.HideProgressBar($"Файл сохранен: {saveExampleFilePath}");
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            dbContext.Employees.AsEnumerable().Where(x => x.Specialty.IsArchive && !x.IsArchive).ToList().ForEach(x => x.IsArchive = true);

            dbContext.SaveChanges();
        }
    }
}
