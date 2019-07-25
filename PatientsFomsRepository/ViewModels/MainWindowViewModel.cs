using PatientsFomsRepository.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
    {
    public class MainWindowViewModel : BindableBase
        {
        #region Fields
        private IViewModel currentPageViewModel;
        #endregion

        #region Properties 
        public RelayCommand ChangeViewCommand { get; }
        public List<IViewModel> ViewModels { get; }
        public IViewModel CurrentViewModel { get => currentPageViewModel; set => SetProperty(ref currentPageViewModel, value); }
        #endregion

        #region Creator
        public MainWindowViewModel()
            {
            ChangeViewCommand = new RelayCommand(ExecuteChangeView);
            ViewModels = new List<IViewModel>();
            ViewModels.Add(new SRZSettingsViewModel());
            CurrentViewModel = ViewModels[0];
            }
        #endregion

        #region Methods
        public void ExecuteChangeView(object parameter)
            {
            var viewModel = parameter as IViewModel;
            if (!ViewModels.Contains(viewModel))
                ViewModels.Add(viewModel);
            CurrentViewModel = ViewModels.FirstOrDefault(vm => vm == viewModel);
            }
        //public bool CanExecuteChangeView(object parameter)
        //    {
        //    return parameter is IViewModel;
        //    }
        #endregion
        }
    }
