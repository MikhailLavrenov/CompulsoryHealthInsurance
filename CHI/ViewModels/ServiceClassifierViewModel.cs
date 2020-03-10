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
using System.Windows;

namespace CHI.ViewModels
{
    class ServiceClassifierViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<ServiceClassifier> serviceClassifier;
        IFileDialogService fileDialogService;
        IMainRegionService mainRegionService;
        int codeColumn = 1;
        int laborCostColumn = 2;

        public bool KeepAlive { get => false; }
        public ObservableCollection<ServiceClassifier> ServiceClassifier { get => serviceClassifier; set => SetProperty(ref serviceClassifier, value); }
        public DelegateCommandAsync LoadCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }

        public ServiceClassifierViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Классификатор Услуг";

            dbContext = new ServiceAccountingDBContext();
            dbContext.ServiceClassifier.Load();
            ServiceClassifier = dbContext.ServiceClassifier.Local.ToObservableCollection();

            LoadCommand = new DelegateCommandAsync(LoadExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
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

        public void LoadExecute()
        {
            mainRegionService.SetBusyStatus("Выбор файла");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Excel files (*.xlsx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                mainRegionService.SetCompleteStatus("Отменено");
                return;
            }

            mainRegionService.SetBusyStatus("Загрузка классификатора услуг");

            using var excel = new ExcelPackage(new FileInfo(fileDialogService.FileName));
            var sheet = excel.Workbook.Worksheets.First();

            var loadedClassifier = new List<ServiceClassifier>();

            for (int i = 2; i <= sheet.Dimension.Rows; i++)
            {
                var codeValue = sheet.Cells[i, codeColumn].Value;
                var laborCostValue = sheet.Cells[i, laborCostColumn].Value;

                if (codeValue != null && laborCostValue != null
                    && int.TryParse(codeValue.ToString(), out var code)
                    && double.TryParse(laborCostValue.ToString(), out var laborCost))
                    loadedClassifier.Add(new ServiceClassifier(code, laborCost));
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                dbContext.Database.ExecuteSqlRaw($"DELETE FROM [{nameof(dbContext.ServiceClassifier)}]");
                dbContext.ServiceClassifier.AddRange(loadedClassifier);
                dbContext.SaveChanges();
                dbContext.ServiceClassifier.Load();
                ServiceClassifier = dbContext.ServiceClassifier.Local.ToObservableCollection();
            });


            mainRegionService.SetCompleteStatus("Успешно загружено");
        }

        public void SaveExampleExecute()
        {
            mainRegionService.SetBusyStatus("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Пример классификатора услуг";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FileName;

            mainRegionService.SetBusyStatus("Сохранение файла");

            using var excel = new ExcelPackage();

            var sheet = excel.Workbook.Worksheets.Add("Лист1");

            var c = new[] {
                new  { Code="Код услуги",   LaborCost="УЕТ" },
                new  { Code="611002",       LaborCost="0,87" },
                new  { Code="611007",       LaborCost="1,57" },
                new  { Code="611009",       LaborCost="1,30" },
                new  { Code="611011",       LaborCost="1,30" },
                new  { Code="611012",       LaborCost="0,30" },
                new  { Code="611013",       LaborCost="0,70" },
                new  { Code="611014",       LaborCost="1,00" },
                new  { Code="611015",       LaborCost="1,30" },
                new  { Code="611016",       LaborCost="1,57" },
                new  { Code="612001",       LaborCost="1,10" },
                new  { Code="612002",       LaborCost="2,00" },
                new  { Code="612003",       LaborCost="0,63" },
                new  { Code="612004",       LaborCost="0,42" },
                new  { Code="612005",       LaborCost="0,93" },
                new  { Code="612006",       LaborCost="1,12" },
                new  { Code="612007",       LaborCost="1,15" },
                new  { Code="612008",       LaborCost="1,15" },
                new  { Code="612009",       LaborCost="1,15" },
                new  { Code="612010",       LaborCost="1,15" }
            };
            sheet.Cells.LoadFromCollection(c);
            sheet.Cells.AutoFitColumns();
            sheet.SelectedRange[1, 1, 1, Math.Max(codeColumn, laborCostColumn)].Style.Font.Bold = true;
            excel.SaveAs(new FileInfo(fileDialogService.FileName));


            mainRegionService.SetCompleteStatus($"Файл сохранен: {saveExampleFilePath}");


        }
    }
}
