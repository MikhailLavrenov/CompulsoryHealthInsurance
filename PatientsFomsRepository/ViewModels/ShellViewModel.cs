using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PatientsFomsRepository.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Поля
        private IRegionManager regionManager;
        IRegion mainRegion;
        private string header;
        private string progress;
        private IViewModel viewModel;
        #endregion

        #region Свойства 
        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Progress { get => progress; set => SetProperty(ref progress, value); }
        public IViewModel ViewModel { get => viewModel; set => SetProperty(ref viewModel, value); }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel()
        {
        }
        public ShellViewModel(IRegionManager regionManager)
        {
            ShowViewCommand = new DelegateCommand<Type>(ShowViewExecute);

            this.regionManager = regionManager;            
        }
        #endregion

        #region Методы
        private void ShowViewExecute(Type parameter)
        {
            mainRegion = regionManager.Regions[RegionNames.MainRegion];

            mainRegion.RequestNavigate(parameter.Name);

            var enumeratorViews = mainRegion.ActiveViews.GetEnumerator();
            enumeratorViews.MoveNext();
            var view= enumeratorViews.Current as FrameworkElement;

            ViewModel = view.DataContext as IViewModel;

            //regionManager.RequestNavigate(RegionNames.MainRegion, parameter.Name);
        }
        #endregion
    }
}
