using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Views;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CHI.ViewModels
{
    public class ComponentsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Component> components;
        List<Component> parents;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public List<Component> Parents { get => parents; set => SetProperty(ref parents, value); }
        public ObservableCollection<Component> Components { get => components; set => SetProperty(ref components, value); }

        public DelegateCommand<Component> EditIndicatorsCommand { get; }

        public ComponentsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;            
            mainRegionService.Header = "Показатели";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Components.Load();
            Components =dbContext.Components.Local.ToObservableCollection();
            Components.CollectionChanged += UpdateParentsCollection;
            UpdateParentsCollection(null, null);

            EditIndicatorsCommand = new DelegateCommand<Component>(EditIndicatorsExecute);
        }

        private void EditIndicatorsExecute(Component component)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Component), component);
            mainRegionService.RequestNavigate(nameof(IndicatorsView), navigationParameters,true);
        }

        private void UpdateParentsCollection(object sender, NotifyCollectionChangedEventArgs e)
        {

            Parents = Components.ToList();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
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
