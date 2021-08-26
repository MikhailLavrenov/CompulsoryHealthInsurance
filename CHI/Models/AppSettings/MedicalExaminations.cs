using CHI.Infrastructure;
using CHI.Services.MedicalExaminations;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CHI.Models.AppSettings
{
    public class MedicalExaminations : DomainObject
    {
        string address;
        byte maxDegreeOfParallelism;
        string casesFileNames;
        string patientsFileNames;
        Credential credential;
        string fileDirectory;
        bool connectionIsValid;


        public string Address { get => address; set => SetProperty(ref address, AppSettings.FixUrl(value)); }
        public byte MaxDegreeOfParallelism { get => maxDegreeOfParallelism; set => SetProperty(ref maxDegreeOfParallelism, value); }
        public string CasesFileNames { get => casesFileNames; set => SetProperty(ref casesFileNames, value); }
        public string PatientFileNames { get => patientsFileNames; set => SetProperty(ref patientsFileNames, value); }
        public Credential Credential { get => credential; set => SetProperty(ref credential, value); }
        public string FileDirectory { get => fileDirectory; set => SetProperty(ref fileDirectory, value); }
        [XmlIgnore] public string FomsCodeMO { get; internal set; }
        [XmlIgnore] public bool ConnectionIsValid { get => connectionIsValid; set => SetProperty(ref connectionIsValid, value); }


        public override void Validate(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Address):
                    if (string.IsNullOrEmpty(Address) || Uri.TryCreate(Address, UriKind.Absolute, out _) == false)
                        AddError(ErrorMessages.UriFormat, propertyName);
                    else
                        RemoveError(ErrorMessages.UriFormat, propertyName);
                    break;

                case nameof(MaxDegreeOfParallelism):
                    if (MaxDegreeOfParallelism < 1)
                        AddError(ErrorMessages.LessOne, propertyName);
                    else
                        RemoveError(ErrorMessages.LessOne, propertyName);
                    break;
            }
        }

        //Устанавливает значения по умолчанию для портала диспансеризации
        public void SetDefault()
        {
            Address = @"http://disp.foms.local/";
            MaxDegreeOfParallelism = 5;
            PatientFileNames = @"LPM, LVM, LOM, LAM";
            CasesFileNames = @"DPM, DVM, DOM, DAM";
            Credential = new Credential { Login = "МойЛогин", Password = "МойПароль" };
        }

    }
}
