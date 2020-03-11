using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
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

        public bool KeepAlive { get => false; }
        public List<Component> Parents { get => parents; set => SetProperty(ref parents, value); }
        public ObservableCollection<Component> Components { get => components; set => SetProperty(ref components, value); }

        public ComponentsViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Показатели";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Components.Load();
            Components =dbContext.Components.Local.ToObservableCollection();
            Components.CollectionChanged += UpdateParentsCollection;
            UpdateParentsCollection(null, null);
        }

        private void UpdateParentsCollection(object sender, NotifyCollectionChangedEventArgs e)
        {

            Parents = Components.ToList();
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
