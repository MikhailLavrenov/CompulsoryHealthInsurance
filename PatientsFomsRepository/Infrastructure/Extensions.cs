using PatientsFomsRepository.Views;
using Prism.Services.Dialogs;
using System;
using System.Windows;

namespace PatientsFomsRepository.Infrastructure
{
    public static class Extensions
    {
        public static ButtonResult ShowDialog(this IDialogService dialogService, string message)
        {
            IDialogResult result = null;
            var dialogName = nameof(NotificationDialogView);
            var dialogParameters = new DialogParameters($"message={message}");
            var dispatcher = Application.Current.Dispatcher;

            var showDialog = (Action)(() => dialogService.ShowDialog(dialogName, dialogParameters, x => result = x));

            dispatcher.BeginInvoke(showDialog).Task.GetAwaiter().GetResult();

            return result.Result;
        }
    }
}
