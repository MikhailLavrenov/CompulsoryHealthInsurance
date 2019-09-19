using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;

namespace PatientsFomsRepository.ViewModels
{
    class NotificationDialogViewModel : DomainObject, IDialogAware
    {
        #region Поля
        private string title;
        private string message;
        #endregion

        #region Свойства
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }
        public event Action<IDialogResult> RequestClose;
        public DelegateCommand<string> CloseDialogCommand { get; }
        #endregion

        #region Конструкторы
        public NotificationDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand<string>(CloseDialogExecute);
        }
        #endregion

        #region Методы
        public void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
        public bool CanCloseDialog()
        {
            return true;
        }
        public void OnDialogClosed()
        {
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("title");
            Message = parameters.GetValue<string>("message");
        }
        protected void CloseDialogExecute(string parameter)
        {
            ButtonResult result;
            var pressedButton = (parameter ?? string.Empty).ToLower();

            if (pressedButton == "ок")
                result = ButtonResult.OK;
            else if (pressedButton == "отмена")
                result = ButtonResult.Cancel;
            else
                result = ButtonResult.None;

            RaiseRequestClose(new DialogResult(result));
        }
        #endregion
    }
}
