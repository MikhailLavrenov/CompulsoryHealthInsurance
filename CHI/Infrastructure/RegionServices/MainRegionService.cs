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
        bool isLocked;
        bool isShowProgressBar;
        bool isShowDialog;
        bool showStatus;
        IRegionManager regionManager;
        string lastNavigatedView;
        Stack<string> navigateBackCollection;
        bool canNavigateBack;

        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Message
        {
            get => status;
            set
            {
                SetProperty(ref status, value);
                IsShowStatus = !string.IsNullOrEmpty(value);
            }
        }
        public bool IsLocked { get => isLocked; set => SetProperty(ref isLocked, value); }
        public bool IsShowProgressBar { get => isShowProgressBar; set => SetProperty(ref isShowProgressBar, value, SwitchProgressBar); }
        public bool IsShowDialog { get => isShowDialog; set => SetProperty(ref isShowDialog, value); }
        public bool IsShowStatus { get => showStatus; private set => SetProperty(ref showStatus, value); }
        public bool CanNavigateBack { get => canNavigateBack; private set => SetProperty(ref canNavigateBack, value); }

        public DelegateCommand CloseStatusCommand { get; }


        public MainRegionService(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            navigateBackCollection = new Stack<string>();

            CloseStatusCommand = new DelegateCommand(() => Message = string.Empty);
        }

        public void HideProgressBarWithhMessage(string statusMessage)
        {
            Message = statusMessage;
            IsShowProgressBar = false;
        }
        public void ShowProgressBarWithMessage(string statusMessage)
        {
            Message = statusMessage;
            IsShowProgressBar = true;
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

            IsLocked = false;
            Message = string.Empty;
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
                if (IsShowProgressBar)
                    regionManager.RequestNavigate(RegionNames.ProgressBarRegion, nameof(ProgressBarView));
                else
                    regionManager.Regions[RegionNames.ProgressBarRegion].RemoveAll();
            });
        }

        public void ClearNavigationBack()
        {
            CanNavigateBack = false;

            var views = regionManager.Regions[RegionNames.MainRegion].Views
                .Where(x => navigateBackCollection.Contains(x.GetType().Name))
                .ToList();

            foreach (var view in views)
                regionManager.Regions[RegionNames.MainRegion].Remove(view);


            navigateBackCollection.Clear();
        }
    }
}
