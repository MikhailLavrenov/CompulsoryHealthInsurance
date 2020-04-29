using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class ServiceClassifiersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<ServiceClassifier> serviceClassifiers;
        ServiceClassifier currentServiceClassifier;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public ServiceClassifier CurrentServiceClassifier { get => currentServiceClassifier; set => SetProperty(ref currentServiceClassifier, value); }
        public ObservableCollection<ServiceClassifier> ServiceClassifiers { get => serviceClassifiers; set => SetProperty(ref serviceClassifiers, value); }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommandAsync UpdateCasesCommand { get; }
        public DelegateCommand<Type> NavigateCommand { get; }


        public ServiceClassifiersViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            dbContext = new ServiceAccountingDBContext();

            dbContext.ServiceClassifiers.Load();

            ServiceClassifiers = dbContext.ServiceClassifiers.Local.ToObservableCollection();

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentServiceClassifier != null).ObservesProperty(() => CurrentServiceClassifier);
            UpdateCasesCommand = new DelegateCommandAsync(UpdateCasesExecute, () => CurrentServiceClassifier != null).ObservesProperty(() => CurrentServiceClassifier);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);

            //DeleteCommand.RaiseCanExecuteChanged();
        }


        private void AddExecute()
        {
            var year = DateTime.Now.Year;

            var newServiceClassifier = new ServiceClassifier()
            {
                ValidFrom = new DateTime(year, 1, 1),
                ValidTo = new DateTime(year, 12, 31)
            };

            ServiceClassifiers.Add(newServiceClassifier);
        }

        private void DeleteExecute()
        {
            ServiceClassifiers.Remove(CurrentServiceClassifier);
        }

        private void UpdateCasesExecute()
        {
            mainRegionService.ShowProgressBar("Пересчет стоимости");

            var tempContext = new ServiceAccountingDBContext();

            var classifierItems = tempContext.ServiceClassifiers
                .Where(x => x.Id == CurrentServiceClassifier.Id)
                .Include(x => x.ServiceClassifierItems)
                .First()
                .ServiceClassifierItems
                .ToLookup(x => x.Code);

            var cases = tempContext.Cases
                 .Where(x => x.DateEnd >= CurrentServiceClassifier.ValidFrom && x.DateEnd <= CurrentServiceClassifier.ValidTo)
                 .Include(x => x.Services)
                 .ToList();

            foreach (var mCase in cases)
                mCase.Services.ForEach(x => x.ClassifierItem = classifierItems[x.Code].FirstOrDefault());

            //cases.Where(x => x.PaidStatus == PaidKind.None).ToList().ForEach(x => x.AmountPaid = x.Services.Sum(y => y.ClassifierItem.Price));

            tempContext.SaveChanges();

            mainRegionService.ShowProgressBar($"Завершено. Обновлено {cases.Count} случая(ев)");
        }

        private void NavigateExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(ServiceClassifier), CurrentServiceClassifier);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            mainRegionService.Header = "Классификаторы услуг";

            KeepAlive = false;
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
