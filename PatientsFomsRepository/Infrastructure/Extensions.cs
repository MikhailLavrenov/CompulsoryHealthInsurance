using PatientsFomsRepository.Views;
using Prism.Services.Dialogs;
using System;
using System.Windows;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Содержит все расширяющие методы
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Вызывает диалоговое окно модально
        /// </summary>
        public static ButtonResult ShowDialog(this IDialogService dialogService, string title, string message)
        {
            IDialogResult result = null;
            var dialogName = nameof(NotificationDialogView);
            var dialogParameters = new DialogParameters();
            dialogParameters.Add("title", title);
            dialogParameters.Add("message", message);
            var dispatcher = Application.Current.Dispatcher;

            var showDialog = (Action)(() => dialogService.ShowDialog(dialogName, dialogParameters, x => result = x));

            dispatcher.BeginInvoke(showDialog).Task.GetAwaiter().GetResult();

            return result.Result;
        }
    }
}
