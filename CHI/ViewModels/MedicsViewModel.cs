using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
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
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Medic> medics;
        IMainRegionService mainRegionService;
        IFileDialogService fileDialogService;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Medic> Medics { get => medics; set => SetProperty(ref medics, value); }

        public DelegateCommandAsync LoadCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }

        public MedicsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;

            mainRegionService.Header = "Медицинские работники";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Medics.Load();
            Medics = dbContext.Medics.Local.ToObservableCollection();

            LoadCommand = new DelegateCommandAsync(LoadExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
        }


        private void LoadExecute()
        {
            mainRegionService.ShowProgressBarWithMessage("Выбор файла");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Excel files (*.xlsx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.HideProgressBarWithhMessage("Отменено");
                return;
            }

            mainRegionService.ShowProgressBarWithMessage("Загрузка файла");

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

            dbContext = new ServiceAccountingDBContext();
            dbContext.Medics.Load();
            Medics = dbContext.Medics.Local.ToObservableCollection();

            mainRegionService.HideProgressBarWithhMessage("Успешно загружено");
        }

        private void SaveExampleExecute()
        {
            mainRegionService.ShowProgressBarWithMessage("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Пример для загрузки мед. персонала";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FileName;

            mainRegionService.ShowProgressBarWithMessage("Сохранение файла");

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

            mainRegionService.HideProgressBarWithhMessage($"Файл сохранен: {saveExampleFilePath}");
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
            dbContext.SaveChanges();
        }
    }
}
