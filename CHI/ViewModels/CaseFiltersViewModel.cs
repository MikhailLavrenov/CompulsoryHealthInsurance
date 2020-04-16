using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class CaseFiltersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<CaseFilter> caseFilters;
        Component currentComponent;
        CaseFilter currentCaseFilter;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }
        public CaseFilter CurrentCaseFilter { get => currentCaseFilter; set => SetProperty(ref currentCaseFilter, value); }
        public Component CurrentComponent { get => currentComponent; set => SetProperty(ref currentComponent, value); }
        public ObservableCollection<CaseFilter> CaseFilters { get => caseFilters; set => SetProperty(ref caseFilters, value); }
        public List<KeyValuePair<Enum, string>> CaseFilterKinds { get; } = Helpers.GetAllValuesAndDescriptions(typeof(CaseFilterKind));

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }

        public CaseFiltersViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            dbContext = new ServiceAccountingDBContext();

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentCaseFilter != null).ObservesProperty(() => CurrentCaseFilter);
        }

        private void AddExecute()
        {
            var newCaseFilter = new CaseFilter();

            CaseFilters.Add(newCaseFilter);

            CurrentComponent.CaseFilters.Add(newCaseFilter);
        }

        private void DeleteExecute()
        {
            CurrentComponent.CaseFilters.Remove(CurrentCaseFilter);

            CaseFilters.Remove(CurrentCaseFilter);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(Component)))
            {
                CurrentComponent = navigationContext.Parameters.GetValue<Component>(nameof(Component));

                CurrentComponent = dbContext.Components.Where(x => x.Id == CurrentComponent.Id).Include(x => x.CaseFilters).First();

                CaseFilters = new ObservableCollection<CaseFilter>(CurrentComponent.CaseFilters?.OrderBy(x => x.Code).OrderBy(x => x.Kind).ToList() ?? new List<CaseFilter>());
            }

            mainRegionService.Header = $"{CurrentComponent.Name} > Фильтр случаев";
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
