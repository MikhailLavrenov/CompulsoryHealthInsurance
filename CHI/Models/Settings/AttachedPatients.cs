using CHI.Infrastructure;
using CHI.Services.SRZ;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CHI.Models
{
    public class AttachedPatients : DomainObject
    {

        string filePath;
        bool formatFile;
        ObservableCollection<ColumnProperty> columnProperties;


        public string PatientsFilePath { get => filePath; set => SetProperty(ref filePath, value); }
        public bool FormatFile { get => formatFile; set => SetProperty(ref formatFile, value); }
        public ObservableCollection<ColumnProperty> ColumnProperties { get => columnProperties; set => SetProperty(ref columnProperties, value); }


        public AttachedPatients()
        {
            ColumnProperties = new();
        }


        //сдвигает вверх элемент коллекции ColumnProperties
        public void MoveUpColumnProperty(ColumnProperty item)
        {
            var itemIndex = ColumnProperties.IndexOf(item);
            if (itemIndex > 0)
                ColumnProperties.Move(itemIndex, itemIndex - 1);
        }

        //сдвигает вниз элемент коллекции ColumnProperties
        public void MoveDownColumnProperty(ColumnProperty item)
        {
            var itemIndex = ColumnProperties.IndexOf(item);
            if (itemIndex >= 0 && itemIndex < ColumnProperties.Count - 1)
                ColumnProperties.Move(itemIndex, itemIndex + 1);
        }

        //устанавливает по-умолчанию настройки файла прикрепленных пациентов
        public void SetDefault()
        {
            FormatFile = true;
            PatientsFilePath = "Прикрепленные пациенты выгрузка.xlsx";

            ColumnProperties = new ObservableCollection<ColumnProperty>()
             {
                    new ColumnProperty{Name="ENP",         AltName="Полис",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="FIO",         AltName="ФИО",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="Фамилия",     AltName="Фамилия",               Hide=false,  Delete=false},
                    new ColumnProperty{Name="Имя",         AltName="Имя",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="Отчество",    AltName="Отчество",              Hide=false,  Delete=false},
                    new ColumnProperty{Name="SEX",         AltName="Пол",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="BIRTHDAY",    AltName="Дата рождения",         Hide=false,  Delete=false},
                    new ColumnProperty{Name="SNILS",       AltName="СНИЛС",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="OLR_NUM",     AltName="Участок",               Hide=false,  Delete=false},
                    new ColumnProperty{Name="TERRITORY",   AltName="Регион",                Hide=false,  Delete=false},
                    new ColumnProperty{Name="DISTRICT",    AltName="Район",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="CITY",        AltName="Город",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="TOWN",        AltName="Населенный пункт",      Hide=false,  Delete=false},
                    new ColumnProperty{Name="STREET",      AltName="Улица",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="HOUSE",       AltName="Дом",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="CORPUS",      AltName="Корпус",                Hide=false,  Delete=false},
                    new ColumnProperty{Name="FLAT",        AltName="Квартира",              Hide=false,  Delete=false},
                    new ColumnProperty{Name="PC_COMM",     AltName="Примечание",            Hide=false,  Delete=false},
                    new ColumnProperty{Name="LAR_NAME",    AltName="Причина прикрепления",  Hide=false,  Delete=false},
                    new ColumnProperty{Name="DOC_SNILS",   AltName="СНИЛС врача",           Hide=false,  Delete=false},
                    new ColumnProperty{Name="SMOCODE",     AltName="Код СМО",               Hide=false,  Delete=false},
                    new ColumnProperty{Name="KLSTREET",    AltName="Код КЛАДР",             Hide=false,  Delete=true},
                    new ColumnProperty{Name="DISTRICTMO",  AltName="Участок МИС",           Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_BDATE",    AltName="PC_BDATE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PERSON_ID",   AltName="ID Пациента",           Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_ID",       AltName="PC_ID",                 Hide=false,  Delete=true},
                    new ColumnProperty{Name="MO_CODE",     AltName="MO_CODE",               Hide=false,  Delete=true},
                    new ColumnProperty{Name="MO_EDNUM",    AltName="MO_EDNUM",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_NUM",      AltName="PC_NUM",                Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_EDATE",    AltName="PC_EDATE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LAT_CODE",    AltName="LAT_CODE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LAT_NAME",    AltName="LAT_NAME",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LAR_CODE",    AltName="LAR_CODE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LDR_CODE",    AltName="LDR_CODE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LDR_NAME",    AltName="LDR_NAME",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_IDATE",    AltName="PC_IDATE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PRV_TYPE",    AltName="PRV_TYPE",              Hide=false,  Delete=true},
             };
        }
    }
}
