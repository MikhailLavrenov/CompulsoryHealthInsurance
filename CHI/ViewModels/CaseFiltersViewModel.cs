using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Settings;
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
        AppDBContext dbContext;
        ObservableCollection<Tuple<string, CaseFilter>> caseFilters;
        Component currentComponent;
        Tuple<string, CaseFilter> currentCaseFilter;
        Type newKind;
        AppSettings settings;
        IMainRegionService mainRegionService;


        public bool KeepAlive { get => false; }
        public Tuple<string, CaseFilter> CurrentCaseFilter { get => currentCaseFilter; set => SetProperty(ref currentCaseFilter, value); }
        public Component CurrentComponent { get => currentComponent; set => SetProperty(ref currentComponent, value); }
        public ObservableCollection<Tuple<string, CaseFilter>> CaseFilters { get => caseFilters; set => SetProperty(ref caseFilters, value); }
        public Type NewKind { get => newKind; set => SetProperty(ref newKind, value); }
        public List<Tuple<Type, string>> Kinds { get; }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }


        public CaseFiltersViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;

            Kinds = new List<Tuple<Type, string>>
            {
                new Tuple<Type, string>(typeof(TreatmentPurposeCaseFiltersCollection), new TreatmentPurposeCaseFiltersCollection().Description),
                new Tuple<Type, string>(typeof(VisitPurposeCaseFiltersCollection), new VisitPurposeCaseFiltersCollection().Description),
                new Tuple<Type, string>(typeof(ServiceCodeCaseFiltersCollection), new ServiceCodeCaseFiltersCollection().Description),
                new Tuple<Type, string>(typeof(ExcludingServiceCodeCaseFiltersCollection), new ExcludingServiceCodeCaseFiltersCollection().Description),
            };

            dbContext = new AppDBContext(settings.Common.SqlServer, settings.Common.SqlDatabase, settings.Common.SqlLogin, settings.Common.SqlPassword);

            AddCommand = new DelegateCommand(AddExecute, () => NewKind != null).ObservesProperty(() => NewKind);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentCaseFilter != null).ObservesProperty(() => CurrentCaseFilter);
        }


        private void AddExecute()
        {
            var newCaseFilter = new CaseFilter();

            var description = ((CaseFiltersCollectionBase)Activator.CreateInstance(newKind)).Description;

            CaseFilters.Add(new Tuple<string, CaseFilter>(description, newCaseFilter));

            CurrentComponent.AddCaseFilter(NewKind, newCaseFilter);
        }

        private void DeleteExecute()
        {
            CurrentComponent.RemoveCaseFilter(CurrentCaseFilter.Item2);

            CaseFilters.Remove(CurrentCaseFilter);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(Component)))
            {
                CurrentComponent = navigationContext.Parameters.GetValue<Component>(nameof(Component));

                CurrentComponent = dbContext.Components
                    .Where(x => x.Id == CurrentComponent.Id)
                    .Include(x => x.CaseFiltersCollections)
                    .ThenInclude(x=>x.Filters)
                    .First();

                   var caseFilterTuples = CurrentComponent.CaseFiltersCollections?
                        .SelectMany(x => x.Filters.Select(y=>new Tuple<string, CaseFilter> (x.Description,y)))
                        .ToList()
                        ?? new ();

                CaseFilters = new ObservableCollection<Tuple<string, CaseFilter>>(caseFilterTuples);
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
