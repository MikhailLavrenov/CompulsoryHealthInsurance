using PatientsFomsRepository.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
    {
    public class MainWindowViewModel : BindableBase
        {
        //https://rachel53461.wordpress.com/2011/12/18/navigation-with-mvvm-2/

        #region Поля
        private IViewModel currentViewModel;
        #endregion

        #region Свойства 
        public RelayCommand ChangeViewCommand { get; }
        public List<IViewModel> ViewModels { get; }
        public IViewModel CurrentViewModel { get => currentViewModel; set => SetProperty(ref currentViewModel, value); }
        #endregion

        #region Конструкторы
        public MainWindowViewModel()
            {            
            ViewModels = new List<IViewModel>();
            ViewModels.Add(new SRZSettingsViewModel());
            ViewModels.Add(new PatientsFileSettingsViewModel());
            ViewModels.Add(new ImportPatientsViewModel());
            ViewModels.Add(new PatientsFileViewModel());

            CurrentViewModel = ViewModels[0];
            ChangeViewCommand = new RelayCommand(ExecuteChangeView, CanExecuteChangeView);
        }
        #endregion

        #region Методы
        public void ExecuteChangeView(object parameter)
            {
            var viewModel = parameter as IViewModel;
            if (ViewModels.Contains(viewModel)==false)
                ViewModels.Add(viewModel);
            CurrentViewModel = ViewModels.FirstOrDefault(x=> x == viewModel);
            }
        public bool CanExecuteChangeView(object parameter)
        {
            return parameter is IViewModel;
        }
        #endregion
    }
    }
