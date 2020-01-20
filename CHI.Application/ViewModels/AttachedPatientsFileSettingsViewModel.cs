﻿using CHI.Application.Infrastructure;
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
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand<ColumnProperty> MoveUpCommand { get; }
        public DelegateCommand<ColumnProperty> MoveDownCommand { get; }
        public DelegateCommand ShowFileDialogCommand { get; }

        #endregion

        #region Конструкторы
        public AttachedPatientsFileSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            //MainRegionService.Header = "Настройки столбцов файла прикрепленных пациентов";

            SaveCommand = new DelegateCommand(SaveExecute);
            LoadCommand = new DelegateCommand(LoadExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            MoveUpCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveUpColumnProperty(x as ColumnProperty));
            MoveDownCommand = new DelegateCommand<ColumnProperty>(x => Settings.MoveDownColumnProperty(x as ColumnProperty));
            ShowFileDialogCommand = new DelegateCommand(ShowFileDialogExecute);
        }
        #endregion

        #region Методы
        private void ShowFileDialogExecute()
        {
            fileDialogService.DialogType = settings.DownloadNewPatientsFile ? FileDialogType.Save : FileDialogType.Open;
            fileDialogService.FileName = settings.PatientsFilePath;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() == true)
                settings.PatientsFilePath = fileDialogService.FileName;
        }
        private void SaveExecute()
        {
            Settings.Save();
            MainRegionService.SetCompleteStatus("Настройки сохранены.");
        }
        private void LoadExecute()
        {
            Settings = Settings.Load();
            MainRegionService.SetCompleteStatus("Изменения настроек отменены.");
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultAttachedPatients();
            MainRegionService.SetCompleteStatus("Настройки установлены по умолчанию.");
        }
        #endregion
    }
}