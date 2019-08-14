using PatientsFomsRepository.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
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
            ViewModels.Add(new PatientsFileViewModel());
            ViewModels.Add(new ImportPatientsViewModel());
            ViewModels.Add(new SRZSettingsViewModel());
            ViewModels.Add(new PatientsFileSettingsViewModel());
            ViewModels.Add(new AboutApplicationViewModel());

            CurrentViewModel = ViewModels[0];
            ChangeViewCommand = new RelayCommand(ExecuteChangeView, CanExecuteChangeView);
        }
        #endregion

        #region Методы
        public void ExecuteChangeView(object parameter)
        {
            var viewModel = parameter as IViewModel;

            if (ViewModels.Contains(viewModel))
                ViewModels.Remove(viewModel);

            var type = parameter.GetType();
            var newInstance = (IViewModel)Activator.CreateInstance(type);

            ViewModels.Add(newInstance);
            CurrentViewModel = newInstance;
        }
        public bool CanExecuteChangeView(object parameter)
        {
            return parameter is IViewModel;
        }
        #endregion
    }
}
