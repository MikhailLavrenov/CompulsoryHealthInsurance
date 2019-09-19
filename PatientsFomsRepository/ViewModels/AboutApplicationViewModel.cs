using PatientsFomsRepository.Infrastructure;
using Prism.Commands;
using Prism.Regions;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PatientsFomsRepository.ViewModels
{
    class AboutApplicationViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private readonly string manualPath;
        private readonly string repositoryPath;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public string Name { get; }
        public string Version { get; }
        public string Copyright { get; }
        public string Author { get; }
        public string Email { get; }
        public string Phone { get; }
        public DelegateCommand OpenManualCommand { get; }
        public DelegateCommand OpenRepositoryCommand { get; }
        #endregion

        #region Конструкторы
        public AboutApplicationViewModel(IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;

            mainRegionService.Header = "О программе";
            manualPath = "Инструкция.docx";
            repositoryPath = @"https://github.com/MikhailLavrenov/PatientsFomsRepository";
            Name = "Хранилище пациентов из СРЗ";
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Copyright = @"©  2019";
            Author = "Лавренов Михаил Владимирович";
            Email = "mvlavrenov@mail.ru";
            Phone = "8-924-213-79-11";

            OpenManualCommand = new DelegateCommand( ()=>Process.Start(manualPath), ()=> File.Exists(manualPath));
            OpenRepositoryCommand = new DelegateCommand(() => Process.Start(repositoryPath));
        }
        #endregion

        #region Методы
        #endregion

    }
}
