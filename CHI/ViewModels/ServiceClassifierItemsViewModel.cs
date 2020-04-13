using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CHI.ViewModels
{
    public class ServiceClassifierItemsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<ServiceClassifierItem> serviceClassifierItems;
        ServiceClassifier currentServiceClassifier;
        ServiceClassifierItem currentServiceClassifierItem;
        IMainRegionService mainRegionService;
        IFileDialogService fileDialogService;

        public bool KeepAlive { get => false; }
        public ServiceClassifierItem CurrentServiceClassifierItem { get => currentServiceClassifierItem; set => SetProperty(ref currentServiceClassifierItem, value); }
        public ServiceClassifier CurrentServiceClassifier { get => currentServiceClassifier; set => SetProperty(ref currentServiceClassifier, value); }
        public ObservableCollection<ServiceClassifierItem> ServiceClassifierItems { get => serviceClassifierItems; set => SetProperty(ref serviceClassifierItems, value); }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommandAsync LoadCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }


        public ServiceClassifierItemsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.mainRegionService = mainRegionService;
            this.fileDialogService = fileDialogService;

            mainRegionService.Header = "Классификатор услуг";

            dbContext = new ServiceAccountingDBContext();

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentServiceClassifierItem != null).ObservesProperty(() => CurrentServiceClassifierItem);
            LoadCommand = new DelegateCommandAsync(LoadExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
        }

        private void AddExecute()
        {
            var newServiceClassifierItem = new ServiceClassifierItem();

            CurrentServiceClassifier.ServiceClassifierItems.Add(newServiceClassifierItem);

            ServiceClassifierItems.Add(newServiceClassifierItem);
        }

        private void DeleteExecute()
        {
            CurrentServiceClassifier.ServiceClassifierItems.Remove(CurrentServiceClassifierItem);

            ServiceClassifierItems.Remove(CurrentServiceClassifierItem);
        }

        private void LoadExecute()
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

            var loadedClassifierItems = new List<ServiceClassifierItem>();

            for (int i = 2; i <= sheet.Dimension.Rows; i++)
            {
                var serviceClassifier = new ServiceClassifierItem
                {
                    Code = int.Parse(sheet.Cells[i, 1].Value.ToString(), CultureInfo.InvariantCulture),
                    LaborCost = double.Parse(sheet.Cells[i, 2].Value.ToString(), CultureInfo.InvariantCulture),
                    Price = double.Parse(sheet.Cells[i, 3].Value.ToString(), CultureInfo.InvariantCulture),
                    IsCaseClosing = sheet.Cells[i, 4].Value.ToString() == "1" ? true : false
                };

                loadedClassifierItems.Add(serviceClassifier);
            }

            ServiceClassifierItems.Where(x => loadedClassifierItems.Any(y => y.Code == x.Code))
                .ToList()
                .ForEach(y => CurrentServiceClassifier.ServiceClassifierItems.Remove(y));

            CurrentServiceClassifier.ServiceClassifierItems.AddRange(loadedClassifierItems);
            ServiceClassifierItems = new ObservableCollection<ServiceClassifierItem>(CurrentServiceClassifier.ServiceClassifierItems);

            mainRegionService.SetCompleteStatus("Успешно загружено");
        }

        private void SaveExampleExecute()
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

            sheet.Cells.LoadFromArrays(new string[][] { new[] { "Код услуги", "УЕТ", "Цена", "Закрывает случай (0-Нет, 1-Да)" } });

            var collection = new List<Tuple<int, double, double, int>> {

                new Tuple<int, double, double, int> ( 622111,     0.5,          0,          0 ),
                new Tuple<int, double, double, int> ( 622110,     0.31,         0,          1 ),
                new Tuple<int, double, double, int> ( 612112,     1.5,          0,          0 ),
                new Tuple<int, double, double, int> ( 622112,     1.5,          0,          1 ),
                new Tuple<int, double, double, int> ( 612113,     4.21,         0,          0 ),
                new Tuple<int, double, double, int> ( 622113,     4.21,         0,          0 ),
                new Tuple<int, double, double, int> ( 612117,     1.68,         0,          1 ),
                new Tuple<int, double, double, int> ( 612111,     0.5,          0,          1 ),
                new Tuple<int, double, double, int> ( 612120,     1.18,         0,          1 ),
                new Tuple<int, double, double, int> ( 622120,     1.18,         0,          0 ),
                new Tuple<int, double, double, int> ( 612154,     1.37,         0,          1 ),
                new Tuple<int, double, double, int> ( 622154,     1.37,         0,          1 ),
                new Tuple<int, double, double, int> ( 612121,     1.5,          0,          0 ),
                new Tuple<int, double, double, int> ( 622121,     1.5,          0,          1 ),
                new Tuple<int, double, double, int> ( 22048,      0,            1501.75,    0 ),
                new Tuple<int, double, double, int> ( 22148,      0,            2065.77,    1 ),
                new Tuple<int, double, double, int> ( 22049,      0,            3611,       0 ),
                new Tuple<int, double, double, int> ( 22149,      0,            1263,       1 ),
                new Tuple<int, double, double, int> ( 22050,      0,            985.5,      1 ),
            };

            sheet.Cells[2, 1].LoadFromCollection(collection);
            sheet.Cells.AutoFitColumns();
            sheet.SelectedRange[1, 1, 1, 4].Style.Font.Bold = true;

            excel.SaveAs(new FileInfo(fileDialogService.FileName));

            mainRegionService.SetCompleteStatus($"Файл сохранен: {saveExampleFilePath}");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(ServiceClassifier)))
            {
                CurrentServiceClassifier = navigationContext.Parameters.GetValue<ServiceClassifier>(nameof(ServiceClassifier));

                CurrentServiceClassifier = dbContext.ServiceClassifiers.Where(x => x.Id == CurrentServiceClassifier.Id).Include(x => x.ServiceClassifierItems).First();

                if (CurrentServiceClassifier.ServiceClassifierItems == null)
                    CurrentServiceClassifier.ServiceClassifierItems = new List<ServiceClassifierItem>();

                ServiceClassifierItems = new ObservableCollection<ServiceClassifierItem>(CurrentServiceClassifier.ServiceClassifierItems);
            }
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
