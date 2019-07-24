using PatientsFomsRepository.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
    {
    class MainWindowViewModel : BindableBase
        {
        #region Fields
        private IViewModel currentPageViewModel;
        #endregion

        #region Properties 
        public RelayCommand ChangeViewCommand { get; }
        public List<IViewModel> PageViewModels { get; }
        public IViewModel CurrentPageViewModel { get => currentPageViewModel; set => SetProperty(ref currentPageViewModel, value); }
        #endregion

        #region Creator
        public MainWindowViewModel()
            {
            ChangeViewCommand = new RelayCommand(ExecuteChangeViewCommand);
            PageViewModels = new List<IViewModel>();
            PageViewModels.Add(new WebSiteSRZSettingsViewModel());
            CurrentPageViewModel = PageViewModels[0];
            }
        #endregion

        #region Methods
        public void ExecuteChangeViewCommand(object parameter)
            {
            var viewModel = parameter as IViewModel;
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);
            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
            }
        //public bool CanExecuteChangeViewCommand(object parameter)
        //    {
        //    return parameter is IViewModel;
        //    }
        #endregion
        }
    }
