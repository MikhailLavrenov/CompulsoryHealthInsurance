using FomsPatientsDB.Models;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
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
    /// Работа с веб-порталом СРЗ
    /// </summary>
    public class WebSiteSRZ : IDisposable
        {
        private HttpClient client;
        private Credential credential;
        bool authorized = false;

        public WebSiteSRZ(string URL, string proxyAddress = null, int proxyPort = 0)
            {
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

        //авторизация на сайте
        public async Task Authorize(Credential credential)
            {
            this.credential = credential;
            var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("lg", credential.Login),
                new KeyValuePair<string, string>("pw", credential.Password),
                });
            var response = await client.PostAsync("data/user.ajax.logon.php", content);
            response.EnsureSuccessStatusCode();
            authorized = true;
            }

        //выход с сайта
        public async void Logout()
            {
            var response = await client.GetAsync("?show=logoff");
            response.EnsureSuccessStatusCode();
            authorized = false;
            }

        //запрашивает данные пациента
        public bool TryGetPatient(string insuranceNumber, out Patient patient)
            {
            var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("mode", "1"),
                new KeyValuePair<string, string>("person_enp", insuranceNumber),
                });
            var response = client.PostAsync("data/reg.person.polis.search.php", content).Result;
            response.EnsureSuccessStatusCode();
            credential.RequestsLeft--;
            var responseText = response.Content.ReadAsStringAsync().Result;

            var responseLines = responseText.Split(new string[] { "||" }, 7, StringSplitOptions.None);

            if (responseLines[0] != "0")
                {
                patient = new Patient(responseLines[2], responseLines[3], responseLines[4], responseLines[5]);
                return true;
                }
            else
                {
                patient = null;
                return false;
                }
            }

        //получает excel файл прикрепленных пациентов на дату
        public async Task GetPatientsFile(string excelFile, DateTime onDate)
            {
            var fileReference = await GetFileReference(onDate);
            var dbfFile = await GetDbfFile(fileReference);
            DbfToExcel(dbfFile, excelFile);
            }

        public void Dispose()
            {
            client.Dispose();
            }

        //получает ссылку на файл заданной даты
        private async Task<string> GetFileReference(DateTime fileDate)
            {
            string shortFileDate = fileDate.ToShortDateString();
            var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("export_date_on", shortFileDate),
                new KeyValuePair<string, string>("exportlist_id", "25"),
                });
            var response = await client.PostAsync("data/dbase.export.php", content);
            response.EnsureSuccessStatusCode();
            string responseText = response.Content.ReadAsStringAsync().Result;

            int begin = responseText.IndexOf(@"<a href='") + 9;
            int end = responseText.IndexOf(@"' ", begin) - begin;

            return responseText.Substring(begin, end);
            }

        //получает dbf файл прикрепленных пацентов
        private async Task<Stream> GetDbfFile(string downloadReference)
            {
            //скачиваем zip архив
            var response = await client.GetAsync(downloadReference);
            response.EnsureSuccessStatusCode();
            var zipFile = await response.Content.ReadAsStreamAsync();

            //извлекаем dbf файл
            Stream dbfFile = new MemoryStream();
            var archive = new ZipArchive(zipFile, ZipArchiveMode.Read);
            archive.Entries[0].Open().CopyTo(dbfFile);
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



        /*

        private class Job    //вспомогательный класс, описывает набор данных для 1 потока запросов к сайту: IDisposable
            {
            public int Nthread;
            public int Nrequests;
            public List<Credential> Accounts;
            public List<Patient> Patients;

            public Job()
                {
                Patients = new List<Patient>();
                Accounts = new List<Credential>();
                }
            }

        //вспомогательный класс, равномерно делит входные данные на N потоков
        private static Job[] PrepareJobs(ConcurrentStack<string> patients, Credential[] Accounts, int nThreads)  
            {
            if (patients.Count < nThreads)
                {
                nThreads = patients.Count / 5;
                if (nThreads < 1)
                    nThreads = 1;
                }

            Job[] jobs = new Job[nThreads];

            //Распределяем учетные записи и запросы равномерно
            int reqInThread = patients.Count / nThreads;
            int ostatok = patients.Count % nThreads;

            //Формируются данные для каждого потока
            for (int i = 0, k = 0, left; i < nThreads; i++)
                {
                jobs[i] = new Job();
                //Распределяем равномерно кол-во запросов для каждого потока
                jobs[i].Nthread = i;
                jobs[i].Nrequests = reqInThread;
                if (ostatok > 0)
                    {
                    jobs[i].Nrequests++;
                    ostatok--;
                    }
                //Формируем полисы
                string str;
                for (int j = 0; j < jobs[i].Nrequests; j++)
                    {
                    patients.TryPop(out str);
                    jobs[i].Patients.Add(new Patient { polis = str });
                    }

                //Формируем логины
                left = jobs[i].Nrequests;
                while (left != 0)
                    {
                    //индекс меняется по кругу
                    if (Accounts.Count() == k)
                        k = 0;
                    if (Accounts[k] == null)
                        {
                        k++;
                        continue;
                        }

                    jobs[i].Accounts.Add(Accounts[k].Copy());
                    if (left >= Accounts[k].Requests)
                        {

                        left -= Accounts[k].Requests;
                        Accounts[k] = null;
                        k++;
                        }
                    else
                        {
                        jobs[i].Accounts[jobs[i].Accounts.Count() - 1].Requests = left;
                        Accounts[k].Requests -= left;
                        break;
                        }
                    }
                }
            return jobs;
            }
        

        //запускает многопоточно запросы к сайту для поиска пациентов
        public async static Task<List<Patient>> GetPatients(ConcurrentStack<string> patients, Credential[] Accounts, int nThreads, Settings settings)   
            {
            return await Task.Run(() =>
            {
                Job[] jobs = WebSiteSRZ.PrepareJobs(patients, Accounts, nThreads);
                var result = new ConcurrentBag<Patient>();
                Task[] tasks = new Task[jobs.Count()];

                foreach (Job job in jobs)
                    {
                    tasks[job.Nthread] = Task.Run(() =>
                    {
                        WebSiteSRZ site;
                        Patient patient;
                        int j = 0;

                        foreach (Credential cred in job.Accounts)
                            {
                            using (site = new WebSiteSRZ(settings.Site, settings.ProxyAddress, settings.ProxyPort))
                                {
                                for (int i = 0; i < 3; i++) //3 попытки на авторизацию с интервалом 10секунд
                                    {
                                    if (site.Authorize(cred).Result)
                                        {
                                        while (cred.Requests > 0)
                                            {
                                            patient = site.GetPatient(job.Patients[j]);
                                            if (patient.polis != null)
                                                {
                                                result.Add(patient);
                                                cred.Requests--;
                                                j++;
                                                }
                                            else break;
                                            }
                                        site.Logout();
                                        break;
                                        }
                                    else
                                        Thread.Sleep(10000);
                                    }
                                }
                            }
                    });
                    }

                Task.WaitAll(tasks);

                return result.ToList<Patient>();
            });
            }
         */

        //запускает многопоточно запросы к сайту для поиска пациентов
        public static List<Patient> GetPatients(string URL, string proxyAddress, int proxyPort, ConcurrentStack<string> patients, List<Credential> credentials, int threadsLimit)
            {
            if (patients.Count < threadsLimit)
                threadsLimit = patients.Count;

            var sites = new WebSiteSRZ[threadsLimit];
            var credentialsLoop = new RoundRobinCredentials(credentials);
            var verifiedPatients = new ConcurrentBag<Patient>();
            var tasks = new Task<WebSiteSRZ>[threadsLimit];

            for (int i = 0; i < threadsLimit; i++)
                {
                int index = i;
                tasks[index] = Task.Run(async () =>
                {
                    var site = new WebSiteSRZ(URL, proxyAddress, proxyPort);
                    credentialsLoop.TryMoveNext();
                    await site.Authorize(credentialsLoop.Current);
                    return site;
                });
                }

            while (patients.TryPop(out string insuranceNumber))
                {
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith(task =>
                {
                    WebSiteSRZ site = task.Result;
                    if (site.TryGetPatient(insuranceNumber, out Patient patient))
                        verifiedPatients.Add(patient);
                    return site;
                });

                }


            Task.WaitAll(tasks);
            var list = new List<Patient>();
            list.AddRange(verifiedPatients);
            return list;
            }


        }


    }




























