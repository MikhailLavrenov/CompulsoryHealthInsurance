using PatientsFomsRepository.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PatientsFomsRepository.ViewModels
{
    class AboutApplicationViewModel : BindableBase, IViewModel
    {
        #region Поля
        private string progress;
        private string manualPath;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public string Progress { get => progress; set => SetProperty(ref progress, value); }
        public string Name { get; }
        public string Version { get; }
        public string Copyright { get; }
        public string Author { get; }
        public string Email { get; }
        public string Phone { get; }
        public RelayCommand OpenManualCommand { get; }
        #endregion

        #region Конструкторы
        public AboutApplicationViewModel()
        {
            ShortCaption = "О программе";
            FullCaption = "О программе";
            Progress = "";
            manualPath = "Инструкция.docx";

            Name = "Хранилище пациентов из СРЗ";
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Copyright = @"©  2019";
            Author = "Лавренов Михаил Владимирович";
            Email = "mvlavrenov@mail.ru";
            Phone = "8-924-213-79-11";

            OpenManualCommand = new RelayCommand(x => Process.Start(manualPath),x=>File.Exists(manualPath)) ;
        }
        #endregion

        #region Методы
        #endregion

    }
}
