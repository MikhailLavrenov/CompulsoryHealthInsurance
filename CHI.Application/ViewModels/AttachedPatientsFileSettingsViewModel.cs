using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.AttachedPatients;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CHI.Application.ViewModels
{
    public class AttachedPatientsFileSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand<ColumnProperty> MoveUpCommand { get; }
        public DelegateCommand<ColumnProperty> MoveDownCommand { get; }
        #endregion

        #region Конструкторы
        public AttachedPatientsFileSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            //MainRegionService.Header = "Настройки столбцов файла прикрепленных пациентов";

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MoveUpCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveUpColumnProperty(x as ColumnProperty));
            MoveDownCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveDownColumnProperty(x as ColumnProperty));
        }
        #endregion

        #region Методы
        private void SetDefaultExecute()
        {
            Settings.SetDefaultAttachedPatientsFile();
            MainRegionService.SetCompleteStatus("Настройки установлены по умолчанию.");
        }
        #endregion
    }
}