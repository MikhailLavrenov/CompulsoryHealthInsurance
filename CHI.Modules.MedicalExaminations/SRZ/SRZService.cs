using CHI.Services.AttachedPatients;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CHI.Services.SRZ
{
    /// <summary>
    /// Работа с веб-порталом СРЗ
    /// </summary>
    public class SRZService : IDisposable
    {
        #region Поля
        private HttpClient client;
        #endregion

        #region Свойства
        public Credential Credential { get; private set; }
        public bool Authorized { get; private set; }
        #endregion

        #region Конструкторы
        public SRZService(string URL) 
            : this(URL, null, 0)
        { }
        public SRZService(string URL, string proxyAddress, int proxyPort)
        {
            Authorized = false;
            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();

            if (proxyAddress != null && proxyPort != 0)
            {
                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy($"{proxyAddress}:{proxyPort}");
            }

            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(URL);
        }
        #endregion

        #region Методы
        //авторизация на сайте
        public bool TryAuthorize(Credential credential)
        {
            Credential = credential;
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("lg", credential.Login),
                new KeyValuePair<string, string>("pw", credential.Password),
            });

            try
            {
                var response = client.PostAsync("data/user.ajax.logon.php", content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (responseText == "")
                    return Authorized = true;
                else
                    return Authorized = false;
            }
            catch (Exception)
            {
                return Authorized = false;
            }
        }
        //выход с сайта
        public void Logout()
        {
            Authorized = false;
            var response = client.GetAsync("?show=logoff").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
        //запрашивает данные пациента
        public bool TryGetPatient(string insuranceNumber, out Patient patient)
        {
            patient = null;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mode", "1"),
                new KeyValuePair<string, string>("person_enp", insuranceNumber),
            });

            try
            {
                var response = client.PostAsync("data/reg.person.polis.search.php", content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var responseLines = responseText.Split(new string[] { "||" }, 7, StringSplitOptions.None);

                if (responseLines[0] != "0")
                {
                    patient = new Patient(responseLines[2], responseLines[3], responseLines[4], responseLines[5]);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //получает excel файл прикрепленных пациентов на дату
        public void GetPatientsFile(string excelFile, DateTime onDate)
        {
            var fileReference = GetFileReference(onDate);
            var dbfFile = GetDbfFile(fileReference);
            DbfToExcel(dbfFile, excelFile);
        }
        //Освобождает ресурсы
        public void Dispose()
        {
            client.Dispose();
        }
        //получает ссылку на файл заданной даты
        private string GetFileReference(DateTime fileDate)
        {
            string shortFileDate = fileDate.ToShortDateString();
            var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("export_date_on", shortFileDate),
                new KeyValuePair<string, string>("exportlist_id", "25"),
                });

            var response = client.PostAsync("data/dbase.export.php", content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            string responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            int begin = responseText.IndexOf(@"<a href='") + 9;
            int end = responseText.IndexOf(@"' ", begin) - begin;

            return responseText.Substring(begin, end);
        }
        //получает dbf файл прикрепленных пацентов
        private Stream GetDbfFile(string downloadReference)
        {
            //скачивает zip архив
            var response = client.GetAsync(downloadReference).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var zipFile = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            //извлекает dbf файл
            Stream dbfFile = new MemoryStream();
            using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
            {
                archive.Entries[0].Open().CopyTo(dbfFile);
            }

            dbfFile.Position = 0;

            return dbfFile;
        }
        //преобразует dbf в excel
        private void DbfToExcel(Stream dbfFile, string excelFilePath)
        {
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




























