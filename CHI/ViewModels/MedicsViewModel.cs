using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.AppSettings;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace CHI.ViewModels
{
    public class MedicsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        ObservableCollection<Medic> medics;
        private readonly AppSettings settings;
        IMainRegionService mainRegionService;
        IFileDialogService fileDialogService;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Medic> Medics { get => medics; set => SetProperty(ref medics, value); }

        public DelegateCommandAsync LoadCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }

        public MedicsViewModel(AppSettings settings, IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;

            mainRegionService.Header = "Медицинские работники";

            dbContext = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);
            dbContext.Medics.Load();
            Medics = dbContext.Medics.Local.ToObservableCollection();

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
                var loadedFomsId = sheet.Cells[i, 1].Value.ToString();
                var loadedFullName = sheet.Cells[i, 2].Value.ToString();

                var medic = Medics.FirstOrDefault(x => x.FomsId == loadedFomsId);

                if (medic != null)
                    medic.FullName = loadedFullName;
            }

            dbContext.SaveChanges();

            dbContext = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);
            dbContext.Medics.Load();
            Medics = dbContext.Medics.Local.ToObservableCollection();

            mainRegionService.HideProgressBar("Успешно загружено");
        }

        private void SaveExampleExecute()
        {
            mainRegionService.ShowProgressBar("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Пример для загрузки мед. персонала";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FileName;

            mainRegionService.ShowProgressBar("Сохранение файла");

            using var excel = new ExcelPackage();

            var sheet = excel.Workbook.Worksheets.Add("Лист1");

            sheet.Cells.LoadFromArrays(new string[][] { new[] { "Код ФОМС", "Фамилия И.О." } });

            var collection = new List<Tuple<string, string>> {

                new Tuple<string,string> ( "001",     "Иванов П.Т."    ),
                new Tuple<string,string> ( "002",     "Петров Е.К."    ),
                new Tuple<string,string> ( "020",     "Смирнов В.П."   ),
                new Tuple<string,string> ( "004",     "Сидоров Н.Л."   ),
                new Tuple<string,string> ( "015",     "Фокин Д.А."     ),
                new Tuple<string,string> ( "063",     "Серов К.К."     ),
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
            dbContext.Employees.AsEnumerable().Where(x => x.Medic.IsArchive && !x.IsArchive).ToList().ForEach(x => x.IsArchive = true);

            dbContext.SaveChanges();
        }
    }
}
