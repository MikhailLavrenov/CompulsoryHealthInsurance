using CHI.Views;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Сервис MainRegion
    /// </summary>
    public class MainRegionService : DomainObject, IMainRegionService
    {
        string header;
        string status;
        bool isBusy;
        bool showStatus;
        IRegionManager regionManager;
        string lastNavigatedView;
        Stack<string> navigateBackCollection;
        bool canNavigateBack;

        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Status
        {
            get => status;
            set
            {
                SetProperty(ref status, value);
                ShowStatus = !string.IsNullOrEmpty(value);
            }
        }
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, SwitchProgressBar); }
        public bool ShowStatus { get => showStatus; private set => SetProperty(ref showStatus, value); }
        public bool CanNavigateBack { get => canNavigateBack; private set => SetProperty(ref canNavigateBack, value); }

        public DelegateCommand CloseStatusCommand { get; }

        public MainRegionService(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            navigateBackCollection = new Stack<string>();

            CloseStatusCommand = new DelegateCommand(()=> Status = string.Empty);
        }

        public void SetCompleteStatus(string statusMessage)
        {
            Status = statusMessage;
            IsBusy = false;
        }
        public void SetBusyStatus(string statusMessage)
        {
            Status = $"{statusMessage}";
            IsBusy = true;
        }
        public void RequestNavigate(string targetName, bool canNavigateBack = false)
        {
            RequestNavigate(targetName, new NavigationParameters(), canNavigateBack);
        }
        public void RequestNavigate(string targetName, NavigationParameters navigationParameters, bool canNavigateBack = false)
        {
            if (canNavigateBack)
            {
                navigateBackCollection.Push(lastNavigatedView);
                CanNavigateBack = true;
            }

            IsBusy = false;
            Status = string.Empty;
            regionManager.RequestNavigate(RegionNames.MainRegion, targetName, navigationParameters);
            lastNavigatedView = targetName;
        }
        public void RequestNavigateBack()
        {
            if (navigateBackCollection.Count > 0)
                RequestNavigate(navigateBackCollection.Pop());

            CanNavigateBack = navigateBackCollection.Count > 0;
        }
        private void SwitchProgressBar()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsBusy)
                    regionManager.RequestNavigate(RegionNames.ProgressBarRegion, nameof(ProgressBarView));
                else
                    regionManager.Regions[RegionNames.ProgressBarRegion].RemoveAll();
            });
        }

        public void ClearNavigationBack()
        {            
            CanNavigateBack = false;

            var views = regionManager.Regions[RegionNames.MainRegion].Views
                .Where(x=> navigateBackCollection.Contains(x.GetType().Name))
                .ToList();

            foreach (var view in views)
                regionManager.Regions[RegionNames.MainRegion].Remove(view);
            
           
            navigateBackCollection.Clear();
        }
    }
}
