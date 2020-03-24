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
        int priceColumn = 3;

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
            dbContext.ServicesClassifier.Load();
            ServiceClassifier = dbContext.ServicesClassifier.Local.ToObservableCollection();

            LoadCommand = new DelegateCommandAsync(LoadExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
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
                var serviceClassifier = new ServiceClassifier
                {
                    Code = int.Parse(sheet.Cells[i, codeColumn].Value.ToString()),
                    LaborCost = double.Parse(sheet.Cells[i, laborCostColumn].Value.ToString()),
                    Price = double.Parse(sheet.Cells[i, priceColumn].Value.ToString())
                };

                loadedClassifier.Add(serviceClassifier);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                dbContext.Database.ExecuteSqlRaw($"DELETE FROM [{nameof(dbContext.ServicesClassifier)}]");
                dbContext = new ServiceAccountingDBContext();
                dbContext.ServicesClassifier.AddRange(loadedClassifier);
                dbContext.SaveChanges();
                ServiceClassifier = dbContext.ServicesClassifier.Local.ToObservableCollection();
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

            var collection = new List<Tuple<int, double, double>> {

                new Tuple<int, double, double> ( 612111,     0.5,          0       ),
                new Tuple<int, double, double> ( 622110,     0.31,         0       ),
                new Tuple<int, double, double> ( 622111,     0.5,          0       ),
                new Tuple<int, double, double> ( 612112,     1.5,          0       ),
                new Tuple<int, double, double> ( 622112,     1.5,          0       ),
                new Tuple<int, double, double> ( 612113,     4.21,         0       ),
                new Tuple<int, double, double> ( 622113,     4.21,         0       ),
                new Tuple<int, double, double> ( 612117,     1.68,         0       ),
                new Tuple<int, double, double> ( 612120,     1.18,         0       ),
                new Tuple<int, double, double> ( 622120,     1.18,         0       ),
                new Tuple<int, double, double> ( 612154,     1.37,         0       ),
                new Tuple<int, double, double> ( 622154,     1.37,         0       ),
                new Tuple<int, double, double> ( 612121,     1.5,          0       ),
                new Tuple<int, double, double> ( 622121,     1.5,          0       ),
                new Tuple<int, double, double> ( 22048,      0,            1501.75 ),
                new Tuple<int, double, double> ( 22148,      0,            2065.77 ),
                new Tuple<int, double, double> ( 22049,      0,            3611    ),
                new Tuple<int, double, double> ( 22149,      0,            1263    ),
                new Tuple<int, double, double> ( 22050,      0,            985.5   ),
            };
            sheet.Cells.LoadFromArrays(new string[][] { new[] { "Код услуги", "УЕТ", "Цена" } });
            sheet.Cells[2, 1].LoadFromCollection(collection);
            sheet.Cells.AutoFitColumns();
            sheet.SelectedRange[1, 1, 1, priceColumn].Style.Font.Bold = true;
            excel.SaveAs(new FileInfo(fileDialogService.FileName));


            mainRegionService.SetCompleteStatus($"Файл сохранен: {saveExampleFilePath}");


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
