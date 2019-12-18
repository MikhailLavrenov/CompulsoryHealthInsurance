using CHI.Application.Views;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Содержит все расширяющие методы
    /// </summary>
    public static class ExtensionMethods
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
            var dispatcher = System.Windows.Application.Current.Dispatcher;

            var showDialog = (Action)(() => dialogService.ShowDialog(dialogName, dialogParameters, x => result = x));

            dispatcher.BeginInvoke(showDialog).Task.GetAwaiter().GetResult();

            return result.Result;
        }
        /// <summary>
        /// Возвращает массив байтов потока
        /// </summary>
        /// <param name="stream">Поток</param>
        /// <returns>Массив байтов</returns>
        public static byte[] GetBytes(this Stream stream)
        {
            stream.Position = 0;

            byte[] result;

            using (var mStream = new MemoryStream())
            {
                stream.CopyTo(mStream);
                result = mStream.ToArray();
            }

            return result;
        }
    }
}
