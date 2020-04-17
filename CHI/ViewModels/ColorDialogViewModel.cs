using CHI.Infrastructure;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Windows.Media;

namespace CHI.ViewModels
{
    class ColorDialogViewModel : DomainObject, IDialogAware
    {
        private string title;
        private Color color;

        public string Title { get => title; set => SetProperty(ref title, value); }
        public Color Color { get => color; set => SetProperty(ref color, value); }

        public event Action<IDialogResult> RequestClose;
        public DelegateCommand<ButtonResult?> CloseDialogCommand { get; }


        public ColorDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand<ButtonResult?>(CloseDialogExecute);
        }


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
            Color = parameters.GetValue<Color>("color");
        }

        protected void CloseDialogExecute(ButtonResult? button)
        {
            var parameters = new DialogParameters();
            parameters.Add("color", Color);

            RaiseRequestClose(new DialogResult(button.Value, parameters));
        }
    }
}
