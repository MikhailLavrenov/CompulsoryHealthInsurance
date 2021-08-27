﻿using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.AppSettings;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class RatiosViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        ObservableCollection<Ratio> ratios;
        Indicator currentIndicator;
        Ratio currentRatio;
        private readonly AppSettings settings;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }
        public Ratio CurrentRatio { get => currentRatio; set => SetProperty(ref currentRatio, value); }
        public Indicator CurrentIndicator { get => currentIndicator; set => SetProperty(ref currentIndicator, value); }
        public ObservableCollection<Ratio> Ratios { get => ratios; set => SetProperty(ref ratios, value); }


        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }

        public RatiosViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;

            dbContext = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);

            AddCommand = new DelegateCommand(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentRatio != null).ObservesProperty(() => CurrentRatio);
        }

        private void AddExecute()
        {
            var newRatio = new Ratio();

            Ratios.Add(newRatio);

            CurrentIndicator.Ratios.Add(newRatio);
        }

        private void DeleteExecute()
        {
            CurrentIndicator.Ratios.Remove(CurrentRatio);

            Ratios.Remove(CurrentRatio);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(Indicator)))
            {
                CurrentIndicator = navigationContext.Parameters.GetValue<Indicator>(nameof(Indicator));

                CurrentIndicator = dbContext.Indicators.Where(x => x.Id == CurrentIndicator.Id).Include(x => x.Ratios).First();

                if (CurrentIndicator.Ratios == null)
                    CurrentIndicator.Ratios = new List<Ratio>();

                Ratios = new ObservableCollection<Ratio>(CurrentIndicator.Ratios);
            }

            mainRegionService.Header = $"{CurrentIndicator.FacadeKind.GetDescription()} > Коэффициенты";
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
