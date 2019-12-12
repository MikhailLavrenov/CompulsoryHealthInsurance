using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.Common;
using CHI.Services.MedicalExaminations;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Application.ViewModels
{
    class LicenseManagerViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public DelegateCommandAsync ExportExaminationsCommand { get; }
        #endregion

        #region Конструкторы
        public LicenseManagerViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Менеджер лицензий";

            ExportExaminationsCommand = new DelegateCommandAsync(ExportExaminationsExecute);
        }
        #endregion

        #region Методы
        private void ExportExaminationsExecute()
        {
        }
        #endregion



    }
}
