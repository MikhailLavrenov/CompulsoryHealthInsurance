using CHI.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Windows.Media;

namespace CHI.ViewModels
{
    class ColorDialogViewModel : DomainObject, INavigationAware, IRegionMemberLifetime
    {
        Color color;
        Color defaultColor;
        Action<Color> onClose;

        public bool KeepAlive { get => false; }
        public Color Color { get => color; set => SetProperty(ref color, value); }

        public DelegateCommand<ButtonResult?> CloseDialogCommand { get; }


        public ColorDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand<ButtonResult?>(CloseDialogExecute);
        }


        protected void CloseDialogExecute(ButtonResult? buttonResult)
        {
            onClose(buttonResult == ButtonResult.OK ? Color : defaultColor);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            defaultColor = navigationContext.Parameters.GetValue<Color>("content");
            Color = defaultColor;
            onClose = navigationContext.Parameters.GetValue<Action<Color>>("onClose");

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
