using CHI.Models;
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
        /// <summary>
        /// Используемые учетные данные для авторизации
        /// </summary>
        public ICredential Credential { get; private set; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="useProxy">Использовать прокси-сервер</param>
        /// <param name="proxyAddress">Адрес прокси-сервера</param>
        /// <param name="proxyPort">Порт прокси-сервера</param>
        public SRZService(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
            : base(URL, useProxy, proxyAddress, proxyPort)
        { }
        #endregion

        #region Методы
        /// <summary>
        /// Авторизация в веб-портале
        /// </summary>
        /// <param name="credential">Учетные данные</param>
        /// <returns>True-успешно авторизован, False-иначе.</returns>
        public bool Authorize(ICredential credential)
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
        /// <summary>
        /// Выход с сайта
        /// </summary>
        public void Logout()
        {
            SendRequest(HttpMethod.Get, @"?show=logoff", null);
            IsAuthorized = false;
        }
        /// <summary>
        /// Запрашивает данные о пациенте по полису
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса.</param>
        /// <returns>Сведения о пациенте.</returns>
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
                return new Patient(responseLines[2], responseLines[3], responseLines[4], responseLines[5]);
            else
                return null;
        }
        /// <summary>
        /// Скачивает и записывает на диск excel файл прикрепленных пациентов на дату.
        /// </summary>
        /// <param name="excelFile">Путь к файлу для записи.</param>
        /// <param name="onDate">Дата на которую выгружается файл.</param>
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
        /// <summary>
        /// Получает ссылку на скачивание файла прикрепленных пациентов.
        /// </summary>
        /// <param name="onDate">Дата на которую сформирован файл.</param>
        /// <returns>Ссылку на скачивание файла прикрепленных пациентов.</returns>
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
        /// <summary>
        /// Конвертирует dbf файл в excel
        /// </summary>
        /// <param name="dbfFile">Поток dbf файла.</param>
        /// <param name="excelFilePath">Путь для сохранения excel файла.</param>
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




























