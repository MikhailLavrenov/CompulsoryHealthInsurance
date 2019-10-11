using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace CHI.Modules.MedicalExaminations.Services
{
    public class BillsRegister
    {
        private StringComparison comparer = StringComparison.OrdinalIgnoreCase;
        private List<string> filePaths;

        public BillsRegister(ICollection<string> filePaths)
        {
            this.filePaths = filePaths.ToList();
        }
        public BillsRegister(string filePath)
        {
            filePaths = new List<string>() { filePath };
        }

        public List<ZL_LIST> GetCoupons(List<Stream> billFiles)
        {
            var result = new List<ZL_LIST>();

            foreach (var billFile in billFiles)
            {
                billFile.Seek(0, SeekOrigin.Begin);
                var formatter = new XmlSerializer(typeof(ZL_LIST));
                var t = (ZL_LIST)formatter.Deserialize(billFile);

                result.Add(t);
            }

            return result;

        }
        public List<Stream> GetFiles(IEnumerable<string> requiredFileNamesStartsWith)
        {
            var files = new List<Stream>();

            foreach (var filePath in filePaths)
                files.AddRange(GetFilesRecursive(filePath, requiredFileNamesStartsWith));

            return files;
        }
        private List<Stream> GetFilesRecursive(string path, IEnumerable<string> requiredFileNamesStartsWith)
        {
            var result = new List<Stream>();
            var isDirectory = new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory);

            if (isDirectory)
            {
                var entries = Directory.GetFileSystemEntries(path);

                foreach (var entry in entries)
                    result.AddRange(GetFilesRecursive(entry, requiredFileNamesStartsWith));
            }
            else
            {
                var extension = Path.GetExtension(path);

                if (extension.Equals(".xml", comparer)
                    && requiredFileNamesStartsWith.Any(x => Path.GetFileName(path).StartsWith(x, comparer)))
                    result.Add(new FileStream(path, FileMode.Open));

                else if (extension.Equals(".zip", comparer))
                    using (var archive = ZipFile.OpenRead(path))
                    {
                        foreach (var entry in archive.Entries)
                            result.AddRange(GetFilesRecursive(entry, requiredFileNamesStartsWith));
                    }
            }

            return result;
        }
        private List<Stream> GetFilesRecursive(ZipArchiveEntry archiveEntry, IEnumerable<string> requiredFileNamesStartsWith)
        {
            var result = new List<Stream>();

            if (string.IsNullOrEmpty(archiveEntry.Name))
                return result;

            var extension = Path.GetExtension(archiveEntry.Name);

            if (extension.Equals(".xml", comparer)
                && requiredFileNamesStartsWith.Any(x => archiveEntry.Name.StartsWith(x, comparer)))
            {
                var extractedEntry = new MemoryStream();
                archiveEntry.Open().CopyTo(extractedEntry);
                result.Add(extractedEntry);
            }
            else if (extension.Equals(".zip", comparer))
            {
                var extractedEntry = new MemoryStream();
                archiveEntry.Open().CopyTo(extractedEntry);

                using (var archive = new ZipArchive(extractedEntry))
                {
                    foreach (var entry in archive.Entries)
                        result.AddRange(GetFilesRecursive(entry, requiredFileNamesStartsWith));
                }
            }

            return result;
        }

        [XmlRoot(ElementName = "ZL_LIST")]
        public class ZL_LIST
        {
            //Счёт
            [XmlElement(ElementName = "SCHET")]
            public SCHET SCHET { get; set; }
            //Записи
            [XmlElement(ElementName = "ZAP")]
            public List<ZAP> ZAP { get; set; }
        }

        [XmlRoot(ElementName = "SCHET")]
        public class SCHET
        {
            //Реестровый номер медицинской организации
            [XmlElement(ElementName = "CODE_MO")]
            public string CODE_MO { get; set; }
            //Тип диспансеризации
            [XmlElement(ElementName = "DISP")]
            public string DISP { get; set; }
        }

        [XmlRoot(ElementName = "ZAP")]
        public class ZAP
        {
            //Сведения о пациенте
            [XmlElement(ElementName = "PACIENT")]
            public PACIENT PACIENT { get; set; }
            //Сведения о законченном случае
            [XmlElement(ElementName = "Z_SL")]
            public Z_SL Z_SL { get; set; }
        }

        [XmlRoot(ElementName = "PACIENT")]
        public class PACIENT
        {
            //Тип документа, подтверждающего факт страхования по ОМС
            [XmlElement(ElementName = "VPOLIS")]
            public string VPOLIS { get; set; }
            //Серия документа, подтверждающего факт страхования по ОМС
            [XmlElement(ElementName = "SPOLIS")]
            public string SPOLIS { get; set; }
            //Номер документа, подтверждающего факт страхования по ОМС
            [XmlElement(ElementName = "NPOLIS")]
            public string NPOLIS { get; set; }
        }

        [XmlRoot(ElementName = "Z_SL")]
        public class Z_SL
        {
            //Дата начала лечения
            [XmlElement(ElementName = "DATE_Z_1")]
            public string DATE_Z_1 { get; set; }
            //Дата окончания лечения
            [XmlElement(ElementName = "DATE_Z_2")]
            public string DATE_Z_2 { get; set; }
            //результат диспансеризации
            [XmlElement(ElementName = "RSLT_D")]
            public string RSLT_D { get; set; }
            //Исход заболевания
            [XmlElement(ElementName = "ISHOD")]
            public string ISHOD { get; set; }
            [XmlElement(ElementName = "SL")]
            public SL SL { get; set; }
        }

        [XmlRoot(ElementName = "SL")]
        public class SL
        {
            //Цель обраще-ния
            public string CEL { get; set; }
            [XmlElement(ElementName = "DATE_1")]
            //Дата начала лечения
            public string DATE_1 { get; set; }
            //Дата оконча-ния лечения
            [XmlElement(ElementName = "DATE_2")]
            public string DATE_2 { get; set; }
            //Диспансерное наблюдение
            [XmlElement(ElementName = "PR_D_N")]
            public string PR_D_N { get; set; }
            //Назначения
            [XmlElement(ElementName = "NAZ")]
            public NAZ NAZ { get; set; }
        }

        [XmlRoot(ElementName = "NAZ")]
        public class NAZ
        {
            //Вид назначения
            [XmlElement(ElementName = "NAZ_R")]
            public string NAZ_R { get; set; }
            //Дата направления
            [XmlElement(ElementName = "NAPR_DATE")]
            public string NAPR_DATE { get; set; }
        }

        [XmlRoot(ElementName = "USL")]
        public class USL
        {
            //Дата начала оказания услуги
            [XmlElement(ElementName = "DATE_IN")]
            public string DATE_IN { get; set; }
            //Код услуги
            [XmlElement(ElementName = "CODE_USL")]
            public string CODE_USL { get; set; }
        }
    }
}
