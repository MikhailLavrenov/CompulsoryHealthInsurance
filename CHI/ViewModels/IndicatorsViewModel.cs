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
    public class IndicatorsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Indicator> indicators;
        Component component;
        Indicator currentIndicator;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public Indicator CurrentIndicator { get => currentIndicator; set => SetProperty(ref currentIndicator, value); }
        public Component Component { get => component; set => SetProperty(ref component, value); }
        public ObservableCollection<Indicator> Indicators { get => indicators; set => SetProperty(ref indicators, value); }
        public List<KeyValuePair<Enum, string>> IndicatorKinds { get; } = IndicatorKind.None.GetAllValuesAndDescriptions().ToList();

        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand<Type> EditExpressionsCommand { get; }

        public IndicatorsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;
            mainRegionService.Header = "Показатели";

            dbContext = new ServiceAccountingDBContext();

            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentIndicator);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentIndicator);
            EditExpressionsCommand = new DelegateCommand<Type>(EditIndicatorsExecute);
        }

        private bool MoveUpCanExecute()
        {
            return CurrentIndicator != null
                && CurrentIndicator.Order != 0;
        }

        private void MoveUpExecute()
        {
            var itemIndex = Indicators.IndexOf(CurrentIndicator);

            var previousIndicator = Indicators[itemIndex - 1];

            previousIndicator.Order++;
            CurrentIndicator.Order--;

            Indicators.Move(itemIndex, itemIndex - 1);
        }

        private bool MoveDownCanExecute()
        {
            return CurrentIndicator != null
                && CurrentIndicator != Indicators.Last();
        }

        private void MoveDownExecute()
        {
            var itemIndex = Indicators.IndexOf(CurrentIndicator);

            var nextIndicator = Indicators[itemIndex - 1];
           
            CurrentIndicator.Order++;
            nextIndicator.Order--;

            Indicators.Move(itemIndex, itemIndex + 1);
        }

        private void EditIndicatorsExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Indicator), CurrentIndicator);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            KeepAlive = false;

            if (navigationContext.Parameters.ContainsKey(nameof(Component)))
            {

                Component = navigationContext.Parameters.GetValue<Component>(nameof(Component));



                var t=dbContext.Indicators.Include(x=>x.Component).Where(x => x.Component.Id == Component.Id).ToList();

                Indicators = dbContext.Indicators.Local.ToObservableCollection();
                Indicators = new ObservableCollection<Indicator>();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Component.Indicators = Indicators.ToList();

            dbContext.SaveChanges();
        }

    }
}
