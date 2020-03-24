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
    public class ExpressionsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Expression> expressions;
        Indicator currentIndicator;
        Expression currentExpression;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }
        public Indicator CurrentIndicator { get => currentIndicator; set => SetProperty(ref currentIndicator, value); }
        public Expression CurrentExpression { get => currentExpression; set => SetProperty(ref currentExpression, value); }
        public ObservableCollection<Expression> Expressions { get => expressions; set => SetProperty(ref expressions, value); }
        public List<KeyValuePair<Enum, string>> ExpressionKinds { get; } = ExpressionKind.None.GetAllValuesAndDescriptions().ToList();

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }

        public ExpressionsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;          

            dbContext = new ServiceAccountingDBContext();

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentExpression != null).ObservesProperty(() => CurrentExpression);
        }

        private void AddExecute()
        {
            var newExpression = new Expression();

            Expressions.Add(newExpression);

            CurrentIndicator.Expressions.Add(newExpression);
        }

        private void DeleteExecute()
        {
            CurrentIndicator.Expressions.Remove(CurrentExpression);

            Expressions.Remove(CurrentExpression);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            CurrentIndicator = navigationContext.Parameters.GetValue<Indicator>(nameof(Indicator));

            dbContext.Attach(CurrentIndicator).State = EntityState.Unchanged;

            CurrentIndicator = dbContext.Indicators.Where(x => x.Id == CurrentIndicator.Id).Include(x => x.Expressions).First();

            Expressions = new ObservableCollection<Expression>(CurrentIndicator.Expressions?.ToList() ?? new List<Expression>());

            mainRegionService.Header = $"{CurrentIndicator.Component.Name} > {CurrentIndicator.Name} > Выражения";
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            CurrentIndicator.Expressions = Expressions.ToList();

            dbContext.SaveChanges();
        }
    }
}
