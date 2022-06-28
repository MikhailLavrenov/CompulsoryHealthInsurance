using CHI.Views;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Сервис MainRegion
    /// </summary>
    public class MainRegionService : DomainObject, IMainRegionService
    {
        string header;
        string status;
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
        public bool IsLocked { get => IsShowProgressBar || IsShowDialog; }
        public bool IsShowProgressBar
        {
            get => isShowProgressBar;
            set
            {
                if (isShowProgressBar != value)
                {
                    SetProperty(ref isShowProgressBar, value);

                    RaisePropertyChanged(nameof(IsLocked));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (IsShowProgressBar)
                            regionManager.RequestNavigate(RegionNames.MainRegionOverlay, nameof(ProgressBarView));
                        else if (!IsLocked)
                            regionManager.Regions[RegionNames.MainRegionOverlay].RemoveAll();
                    });
                }
            }
        }
        public bool IsShowDialog
        {
            get => isShowDialog;
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isShowDialog != value)
                    {
                        SetProperty(ref isShowDialog, value);

                        RaisePropertyChanged(nameof(IsLocked));

                        if (!IsLocked)
                            regionManager.Regions[RegionNames.MainRegionOverlay].RemoveAll();
                    }
                });
            }
        }
        public bool IsShowStatus { get => showStatus; private set => SetProperty(ref showStatus, value); }
        public bool CanNavigateBack { get => canNavigateBack; private set => SetProperty(ref canNavigateBack, value); }

        public DelegateCommand CloseStatusCommand { get; }


        public MainRegionService(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            navigateBackCollection = new Stack<string>();

            CloseStatusCommand = new DelegateCommand(() => Message = string.Empty);
        }


        public void HideProgressBar(string statusMessage)
        {
            Message = statusMessage;
            IsShowProgressBar = false;
        }

        public void ShowProgressBar(string statusMessage)
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

            IsShowDialog = false;
            IsShowProgressBar = false;
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

        public void RequestNavigateHome()
        {
            RequestNavigate(nameof(NavigationMenuView));

            ClearNavigationBack();
        }

        public async Task<Color> ShowColorDialog(Color defaultColor)
        {
            IsShowDialog = true;

            var selectedColor = await ShowDialog<Color>(defaultColor, nameof(ColorDialogView));

            IsShowDialog = false;

            return (Color)selectedColor;
        }

        public async Task<bool> ShowNotificationDialog(string message)
        {
            IsShowDialog = true;

            var dialogResult = await ShowDialog<bool>(message, nameof(NotificationDialogView));

            IsShowDialog = false;

            return (bool)dialogResult;
        }

        private async Task<object> ShowDialog<T>(object content, string viewName)
        {
            return await Task.Run(() =>
            {
                object result = null;

                var autoResetEvent = new AutoResetEvent(false);

                Action<T> callback = x =>
                {
                    result = x;
                    autoResetEvent.Set();
                };

                var parameters = new NavigationParameters();

                parameters.Add("onClose", callback);
                parameters.Add("content", content);

                Application.Current.Dispatcher.InvokeAsync(() => regionManager.RequestNavigate(RegionNames.MainRegionOverlay, viewName, parameters));

                autoResetEvent.WaitOne();

                return result;
            });
        }
    }
}
