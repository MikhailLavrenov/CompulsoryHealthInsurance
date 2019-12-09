using CHI.Services.AttachedPatients;
using CHI.Services.Common;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;

namespace CHI.Services.SRZ
{
    /// <summary>
    /// Представляет сервис для работы в веб-порталом СРЗ
    /// </summary>
    public class SRZService : WebServiceBase, IDisposable
    {
        #region Поля
        #endregion

        #region Свойства
        public ICredential Credential { get; private set; }
        #endregion

        #region Конструкторы
        public SRZService(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
            : base(URL, useProxy, proxyAddress, proxyPort)
        { }
        #endregion

        #region Методы
        //авторизация на сайте
        public bool TryAuthorize(ICredential credential)
        {
            Credential = credential;
            var content = new Dictionary<string, string> {
                { "lg", credential.Login },
                { "pw", credential.Password},
            };

            try
            {
                var responseText = SendRequest(HttpMethod.Post, @"data/user.ajax.logon.php", content);

                if (responseText == "")
                    return IsAuthorized = true;
                else
                    return IsAuthorized = false;
            }
            catch (Exception)
            {
                return IsAuthorized = false;
            }
        }
        //выход с сайта
        public void Logout()
        {
            SendRequest(HttpMethod.Get, @"?show=logoff", null);
            IsAuthorized = false;
        }
        //запрашивает данные пациента
        public Patient GetPatient(string insuranceNumber)
        {
            CheckAuthorization();

            var content = new Dictionary<string, string> {
                { "mode", "1" },
                { "person_enp", insuranceNumber },
            };

            var responseText = SendRequest(HttpMethod.Post, @"data/reg.person.polis.search.php", content);

            var responseLines = responseText.Split(new string[] { "||" }, 7, StringSplitOptions.None);

            if (responseLines[0] != "0")
                return  new Patient(responseLines[2], responseLines[3], responseLines[4], responseLines[5]);
            else
                return null;
        }
        //получает excel файл прикрепленных пациентов на дату
        public void GetPatientsFile(string excelFile, DateTime onDate)
        {
            CheckAuthorization();

            var reference = GetPatientsFileReference(onDate);

            //скачивает zip архив
            var zipFile = SendGetRequest(reference);

            //извлекает dbf файл
            Stream dbfFile = new MemoryStream();

            using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                archive.Entries[0].Open().CopyTo(dbfFile);

            ConvertDbfToExcel(dbfFile, excelFile);
        }
        //получает ссылку на скачивание файла прикрепленных пациентов
        private string GetPatientsFileReference(DateTime onDate)
        {            
            string shortFileDate = onDate.ToShortDateString();

            var content = new Dictionary<string, string> {
                { "export_date_on", shortFileDate },
                { "exportlist_id", "25" },
            };

            var responseText = SendRequest(HttpMethod.Post, @"data/dbase.export.php", content);

            int begin = responseText.IndexOf(@"<a href='") + 9;
            int length = responseText.IndexOf(@"' ", begin) - begin;

            return responseText.Substring(begin, length);
        }
        //преобразует dbf в excel
        private static void ConvertDbfToExcel(Stream dbfFile, string excelFilePath)
        {
            dbfFile.Position = 0;

            using (var table = NDbfReader.Table.Open(dbfFile))
            using (var excel = new ExcelPackage())
            {
                var reader = table.OpenReader(Encoding.GetEncoding(866));
                var sheet = excel.Workbook.Worksheets.Add("Лист1");

                //вставляет заголовки и устанавливает формат столбцов
                for (int column = 0; column < table.Columns.Count; column++)
                {
                    sheet.Cells[1, column + 1].Value = table.Columns[column].Name.ToString();

                    var type = table.Columns[column].Type;
                    type = Nullable.GetUnderlyingType(type) ?? type;

                    if (type.Name == "DateTime")
                        sheet.Column(column + 1).Style.Numberformat.Format = "dd.MM.yyyy";
                }

                //заполняет строки таблицы
                int row = 2;
                while (reader.Read())
                {
                    for (int column = 0; column < table.Columns.Count; column++)
                        sheet.Cells[row, column + 1].Value = reader.GetValue(table.Columns[column]);
                    row++;
                }

                excel.SaveAs(new FileInfo(excelFilePath));
            }
        }
        #endregion
    }
}




























