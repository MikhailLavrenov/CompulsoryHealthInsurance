using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
    {

    /// <summary>
    /// Работает с веб-порталом СРЗ
    /// </summary>
    public class WebSiteSRZ : IDisposable
        {
        private HttpClient client;

        public WebSiteSRZ(string URL, string proxyAddress = null, int proxyPort = 0)
            {
            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();
            if (proxyAddress!=null && proxyPort!=0)
                {
                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy($"{proxyAddress}:{proxyPort}");
                }
            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(URL);
            }

        //авторизация на сайте
        public async Task Authorize(string login, string password)
            {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("lg", login),
                new KeyValuePair<string, string>("pw", password),
            });

            var response = await client.PostAsync("data/user.ajax.logon.php", content);
            response.EnsureSuccessStatusCode();
            }

        //выход с сайта
        public async void Logout()
            {
            var response = await client.GetAsync("?show=logoff");
            response.EnsureSuccessStatusCode();
            }

        //получает excel файл прикрепленных пациентов на дату
        public async Task GetPatientsExcelFile(DateTime date, string excelFile)
            {
            var fileReference = await GetFileReference(date);
            var dbfFile = $"{Path.GetDirectoryName(excelFile)}\\ATT_MO_temp_ERW3sdcxf1XCa.DBF";
            await DownloadPatientsFile(fileReference, dbfFile);
            DbfToExcel(dbfFile, excelFile);
            File.Delete(dbfFile);
            }

        //загружает файл прикрепленных пациентов на дату
        private async Task DownloadPatientsFile(string fileReference, string dbfFile)
            {
            //скачиваем zip архив в оперативную память
            var response = await client.GetAsync(fileReference);
            response.EnsureSuccessStatusCode();
            byte[] zipArchive = await response.Content.ReadAsByteArrayAsync();

            //извлекаем dbf файл из zip архива            
            using (var memoryStream = new MemoryStream(zipArchive))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                archive.Entries[0].ExtractToFile(dbfFile, true);
            }

        //получает ссылку на файл на дату
        private async Task<string> GetFileReference(DateTime date)
            {
            string shortDate = date.ToShortDateString().ToString();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("export_date_on", shortDate),
                new KeyValuePair<string, string>("exportlist_id", "25"),
            });

            var response = await client.PostAsync("data/dbase.export.php", content);
            response.EnsureSuccessStatusCode();
            string responseText = response.Content.ReadAsStringAsync().Result;

            int begin = responseText.IndexOf(@"<a href='") + 9;
            int end = responseText.IndexOf(@"' ", begin) - begin;

            return responseText.Substring(begin, end);
            }

        //запрашивает данные пациента
        public void GetPatient(string insuranceNumber)
            {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mode", "1"),
                new KeyValuePair<string, string>("person_enp", insuranceNumber),
            });

            var response = client.PostAsync("data/reg.person.polis.search.php", content).Result;
            response.EnsureSuccessStatusCode();

            string result = response.Content.ReadAsStringAsync().Result;
            WebResponseToFullName(result);

            }

        //преобразует ответ web сервера в ФИО
        private static string WebResponseToFullName(string response)
            {

            var lines = response.Split(new string[] { "||" }, 7, StringSplitOptions.None);

            return "";
            }

        //преобразует dbf в excel
        public void DbfToExcel(string dbfFile, string excelFile)
            {
            using (Stream stream = new FileStream(dbfFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var table = NDbfReader.Table.Open(stream))
            using (ExcelPackage excel = new ExcelPackage())
                {
                var reader = table.OpenReader(Encoding.GetEncoding(866));
                var sheet = excel.Workbook.Worksheets.Add("Лист1");

                for (int column = 0; column < table.Columns.Count; column++)
                    {
                    sheet.Cells[1, column + 1].Value = table.Columns[column].Name.ToString();

                    var type = table.Columns[column].Type;
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    if (type.Name == "DateTime")
                        sheet.Column(column + 1).Style.Numberformat.Format = "dd.MM.yyyy";
                    }

                int row = 2;
                while (reader.Read())
                    {
                    for (int column = 0; column < table.Columns.Count; column++)
                        sheet.Cells[row, column + 1].Value = reader.GetValue(table.Columns[column]);                        

                    row++;
                    }

                excel.SaveAs(new FileInfo(excelFile));
                }
            }

        public void Dispose()
            {
            client.Dispose();
            }

        }
    }




























