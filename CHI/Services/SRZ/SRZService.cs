using CHI.Infrastructure;
using CHI.Models;
using CHI.Services.Common;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.SRZ
{
    /// <summary>
    /// Представляет сервис для работы в веб-порталом СРЗ
    /// </summary>
    public class SRZService : WebServiceBase, IDisposable
    {
        /// <summary>
        /// Учетные данные для авторизации
        /// </summary>
        public ICredential Credential { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="useProxy">Использовать прокси-сервер</param>
        /// <param name="proxyAddress">Адрес прокси-сервера</param>
        /// <param name="proxyPort">Порт прокси-сервера</param>
        public SRZService(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
            : base(URL, useProxy, proxyAddress, proxyPort)
        { }


        /// <summary>
        /// Авторизация в веб-портале
        /// </summary>
        /// <param name="credential">Учетные данные</param>
        /// <returns>True-успешно авторизован, False-иначе.</returns>
        public async Task<bool> AuthorizeAsync(ICredential credential)
        {
            Credential = credential;
            var content = new Dictionary<string, string> {
                { "lg", credential.Login },
                { "pw", credential.Password},
            };

            var responseText = await SendPostAsync(@"data/user.ajax.logon.php", content);

            return IsAuthorized = string.IsNullOrEmpty(responseText);
        }

        /// <summary>
        /// Выход с сайта
        /// </summary>
        public async Task LogoutAsync()
        {
            await SendGetTextAsync(@"?show=logoff");
            IsAuthorized = false;
        }

        /// <summary>
        /// Запрашивает данные о пациенте по полису
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса.</param>
        /// <returns>Сведения о пациенте.</returns>
        public async Task<Patient> GetPatientAsync(string insuranceNumber)
        {
            ThrowExceptionIfNotAuthorized();

            var content = new Dictionary<string, string> {
                { "mode", "1" },
                { "person_enp", insuranceNumber },
            };

            var responseText = await SendPostAsync(@"data/reg.person.polis.search.php", content);

            var responseLines = responseText.Split(new string[] { "||" }, 7, StringSplitOptions.None);

            if (responseLines[0] != "0")
                return new Patient(responseLines[2], responseLines[3], responseLines[4], responseLines[5]);
            else
                return null;
        }

        /// <summary>
        /// Скачивает и записывает на диск excel файл прикрепленных пациентов на дату.
        /// </summary>
        /// <param name="excelFile">Путь к файлу для записи.</param>
        /// <param name="onDate">Дата на которую выгружается файл.</param>
        public async Task GetPatientsFileAsync(string excelFile, DateTime onDate)
        {
            ThrowExceptionIfNotAuthorized();

            var urn = await GetPatientsFileUrnAsync(onDate);

            var zipFile = await SendGetStreamAsync(urn);

            using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                ConvertDbfToExcel(archive.Entries[0].Open(), excelFile);
        }

        /// <summary>
        /// Получает ссылку на скачивание файла прикрепленных пациентов.
        /// </summary>
        /// <param name="onDate">Дата на которую сформирован файл.</param>
        /// <returns>Ссылку на скачивание файла прикрепленных пациентов.</returns>
        async Task<string> GetPatientsFileUrnAsync(DateTime onDate)
        {
            var content = new Dictionary<string, string> {
                { "export_date_on", onDate.ToShortDateString() },
                { "exportlist_id", "25" },
            };

            var responseText = await SendPostAsync(@"data/dbase.export.php", content);

            return responseText.SubstringBetween(null, @"<a href='", @"' ");
        }

        /// <summary>
        /// Конвертирует dbf файл в excel
        /// </summary>
        /// <param name="dbfFile">Поток dbf файла.</param>
        /// <param name="excelFilePath">Путь для сохранения excel файла.</param>
        static void ConvertDbfToExcel(Stream dbfFile, string excelFilePath)
        {
            if (dbfFile.CanSeek)
                dbfFile.Seek(0, SeekOrigin.Begin);

            using var table = NDbfReader.Table.Open(dbfFile);
            using var excel = new ExcelPackage();

            var reader = table.OpenReader(Encoding.GetEncoding(866));
            var sheet = excel.Workbook.Worksheets.Add("Лист1");

            //вставляет заголовки и устанавливает формат столбцов
            for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
            {
                sheet.Cells[1, columnIndex + 1].Value = table.Columns[columnIndex].Name.ToString();

                var type = table.Columns[columnIndex].Type;
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (type.Name == "DateTime")
                    sheet.Column(columnIndex + 1).Style.Numberformat.Format = "dd.MM.yyyy";
            }

            //заполняет строки таблицы
            int rowIndex = 2;
            while (reader.Read())
            {
                for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                    sheet.Cells[rowIndex, columnIndex + 1].Value = reader.GetValue(table.Columns[columnIndex]);
                rowIndex++;
            }

            excel.SaveAs(new FileInfo(excelFilePath));
        }
    }
}




























