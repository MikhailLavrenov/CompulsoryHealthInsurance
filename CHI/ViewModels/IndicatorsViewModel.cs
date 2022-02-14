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
    public class IndicatorsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        ObservableCollection<IndicatorBase> indicators;
        Component currentComponent;
        IndicatorBase currentIndicator;
        Type newKind;
        AppSettings settings;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public IndicatorBase CurrentIndicator { get => currentIndicator; set => SetProperty(ref currentIndicator, value); }
        public Component CurrentComponent { get => currentComponent; set => SetProperty(ref currentComponent, value); }
        public ObservableCollection<IndicatorBase> Indicators { get => indicators; set => SetProperty(ref indicators, value); }
        public Type NewKind { get => newKind; set => SetProperty(ref newKind, value); }
        public List<Tuple<Type, string>> Kinds { get; }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand<Type> NavigateCommand { get; }

        public IndicatorsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;
            Kinds = new List<Tuple<Type, string>>
            {
                new Tuple<Type, string>(typeof(CasesIndicator), new CasesIndicator().Description),
                new Tuple<Type, string>(typeof(VisitsIndicator), new VisitsIndicator().Description),
                new Tuple<Type, string>(typeof(LaborCostIndicator), new LaborCostIndicator().Description),                                
                new Tuple<Type, string>(typeof(CasesLaborCostIndicator), new CasesLaborCostIndicator().Description),                                                
                new Tuple<Type, string>(typeof(VisitsLaborCostIndicator), new VisitsLaborCostIndicator().Description),
                new Tuple<Type, string>(typeof(BedDaysIndicator), new BedDaysIndicator().Description),
                new Tuple<Type, string>(typeof(CostIndicator), new CostIndicator().Description),
            };

            dbContext = new AppDBContext(settings.Common.SqlServer, settings.Common.SqlDatabase, settings.Common.SqlLogin, settings.Common.SqlPassword);

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentIndicator != null).ObservesProperty(() => CurrentIndicator);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentIndicator);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentIndicator);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);
        }


        void AddExecute()
        {
            var newIndicator = (IndicatorBase)Activator.CreateInstance(newKind);
            newIndicator.Component = CurrentComponent;
            newIndicator.Order = Indicators.Count == 0 ? 0 : Indicators.Last().Order + 1;

            Indicators.Add(newIndicator);

            CurrentComponent.Indicators.Add(newIndicator);
        }

        void DeleteExecute()
        {
            var offset = Indicators.IndexOf(CurrentIndicator);

            CurrentComponent.Indicators.Remove(CurrentIndicator);

            Indicators.Remove(CurrentIndicator);

            for (int i = offset; i < Indicators.Count; i++)
                Indicators[i].Order--;
        }

        bool MoveUpCanExecute()
        {
            return CurrentIndicator != null
                && CurrentIndicator.Order != 0;
        }

        void MoveUpExecute()
        {
            var itemIndex = Indicators.IndexOf(CurrentIndicator);

            var previousIndicator = Indicators[itemIndex - 1];

            previousIndicator.Order++;
            CurrentIndicator.Order--;

            Indicators.Move(itemIndex, itemIndex - 1);

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        bool MoveDownCanExecute()
        {
            return CurrentIndicator != null
                && CurrentIndicator != Indicators.Last();
        }

        void MoveDownExecute()
        {
            var itemIndex = Indicators.IndexOf(CurrentIndicator);

            var nextIndicator = Indicators[itemIndex + 1];

            CurrentIndicator.Order++;
            nextIndicator.Order--;

            Indicators.Move(itemIndex, itemIndex + 1);

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        void NavigateExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(IndicatorBase), CurrentIndicator);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(Component)))
            {
                CurrentComponent = navigationContext.Parameters.GetValue<Component>(nameof(Component));

                CurrentComponent = dbContext.Components.Where(x => x.Id == CurrentComponent.Id).Include(x => x.Indicators).First();

                Indicators = new ObservableCollection<IndicatorBase>(CurrentComponent.Indicators?.OrderBy(x => x.Order).ToList() ?? new List<IndicatorBase>());
            }

            mainRegionService.Header = $"{CurrentComponent.Name} > Показатели";

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
