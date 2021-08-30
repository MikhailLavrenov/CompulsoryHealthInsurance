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
        ObservableCollection<Indicator> indicators;
        Component currentComponent;
        Indicator currentIndicator;
        AppSettings settings;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public Indicator CurrentIndicator { get => currentIndicator; set => SetProperty(ref currentIndicator, value); }
        public Component CurrentComponent { get => currentComponent; set => SetProperty(ref currentComponent, value); }
        public ObservableCollection<Indicator> Indicators { get => indicators; set => SetProperty(ref indicators, value); }
        public List<KeyValuePair<Enum, string>> IndicatorKinds { get; } = Helpers.GetAllValuesAndDescriptions(typeof(IndicatorKind));

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand<Type> NavigateCommand { get; }

        public IndicatorsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;

            dbContext = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentIndicator != null).ObservesProperty(() => CurrentIndicator);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentIndicator);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentIndicator);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);
        }

        private void AddExecute()
        {
            var newIndicator = new Indicator
            {
                Component = CurrentComponent,
                Order = Indicators.Count == 0 ? 0 : Indicators.Last().Order + 1
            };

            Indicators.Add(newIndicator);

            CurrentComponent.Indicators.Add(newIndicator);
        }

        private void DeleteExecute()
        {
            var offset = Indicators.IndexOf(CurrentIndicator);

            CurrentComponent.Indicators.Remove(CurrentIndicator);

            Indicators.Remove(CurrentIndicator);

            for (int i = offset; i < Indicators.Count; i++)
                Indicators[i].Order--;
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

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private bool MoveDownCanExecute()
        {
            return CurrentIndicator != null
                && CurrentIndicator != Indicators.Last();
        }

        private void MoveDownExecute()
        {
            var itemIndex = Indicators.IndexOf(CurrentIndicator);

            var nextIndicator = Indicators[itemIndex + 1];

            CurrentIndicator.Order++;
            nextIndicator.Order--;

            Indicators.Move(itemIndex, itemIndex + 1);

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private void NavigateExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Indicator), CurrentIndicator);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(Component)))
            {
                CurrentComponent = navigationContext.Parameters.GetValue<Component>(nameof(Component));

                CurrentComponent = dbContext.Components.Where(x => x.Id == CurrentComponent.Id).Include(x => x.Indicators).First();

                Indicators = new ObservableCollection<Indicator>(CurrentComponent.Indicators?.OrderBy(x => x.Order).ToList() ?? new List<Indicator>());
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
