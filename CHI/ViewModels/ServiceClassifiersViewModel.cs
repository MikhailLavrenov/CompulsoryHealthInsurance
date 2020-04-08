using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;

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
        public DelegateCommand<Type> NavigateCommand { get; }

        public ServiceClassifiersViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            dbContext = new ServiceAccountingDBContext();

            dbContext.ServiceClassifiers.Load();

            ServiceClassifiers = dbContext.ServiceClassifiers.Local.ToObservableCollection();

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentServiceClassifier != null).ObservesProperty(() => CurrentServiceClassifier);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);

            //DeleteCommand.RaiseCanExecuteChanged();
        }

        private void AddExecute()
        {
            var newServiceClassifier = new ServiceClassifier();

            //newServiceClassifier.ValidFrom.Date.AddDays(-newServiceClassifier.ValidFrom.Date.Day + 1);
            //newServiceClassifier.ValidTo.Date.AddDays(-newServiceClassifier.ValidFrom.Date.Day + 1);

            ServiceClassifiers.Add(newServiceClassifier);
        }

        private void DeleteExecute()
        {
            ServiceClassifiers.Remove(CurrentServiceClassifier);
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
