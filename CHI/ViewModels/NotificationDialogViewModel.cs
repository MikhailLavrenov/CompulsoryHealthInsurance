using CHI.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;

namespace CHI.ViewModels
{
    class NotificationDialogViewModel : DomainObject, INavigationAware, IRegionMemberLifetime
    {
        private string message;
        Action<bool> onClose;

        public bool KeepAlive { get => false; }
        public string Message { get => message; set => SetProperty(ref message, value); }

        public DelegateCommand<ButtonResult?> CloseDialogCommand { get; }
        

        public NotificationDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand<ButtonResult?>(CloseDialogExecute);
        }

        protected void CloseDialogExecute(ButtonResult? buttonResult)
        {
            onClose(buttonResult.Value== ButtonResult.OK? true:false);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Message = navigationContext.Parameters.GetValue<string>("content");
            onClose = navigationContext.Parameters.GetValue<Action<bool>>("onClose");

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
