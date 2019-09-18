using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;

namespace PatientsFomsRepository.ViewModels
{
    class NotificationDialogViewModel : DomainObject, IDialogAware
    {
        #region Поля
        private string title = "Notification";
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
            CloseDialogCommand = new DelegateCommand<string>(CloseDialog);
        }
        #endregion

        #region Методы
        public  void  RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
        public  bool CanCloseDialog()
        {
            return true;
        }
        public  void OnDialogClosed()
        {
        }
        public  void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
        }
        protected  void CloseDialog(string parameter)
        {
            var result = ButtonResult.None;

            if (parameter?.ToLower() == "ОК")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "Отмена")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result));
        }
        #endregion
    }
}
