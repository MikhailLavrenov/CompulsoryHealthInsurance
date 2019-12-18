using CHI.Services.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет сервис для многопоточной загрузки профилактических осмотров на портал диспансеризации
    /// </summary>
    public class ExaminationServiceParallel
    {
        /// <summary>
        /// Коллекция учетных записей
        /// </summary>
        public IEnumerable<ICredential> Credentials { get; private set; }
        /// <summary>
        /// URL
        /// </summary>
        public string URL { get; private set; }
        /// <summary>
        /// Исползовать прокси-сервер
        /// </summary>
        public bool UseProxy { get; private set; }
        /// <summary>
        /// Адрес прокси-сервера
        /// </summary>
        public string ProxyAddress { get; private set; }
        /// <summary>
        /// Порт прокси-сервера
        /// </summary>
        public int ProxyPort { get; private set; }
        /// <summary>
        /// Лимит параллельных потоков
        /// </summary>
        public int ThreadsLimit { get; private set; }

        /// <summary>
        /// Событие возникает при изменении кол-ва пациентов с загруженными осмотрами. 
        /// </summary>
        public event EventHandler<CounterEventArgs> AddCounterChangeEvent;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url"></param>
        /// <param name="threadsLimit"></param>
        /// <param name="credentials"></param>
        public ExaminationServiceParallel(string url, int threadsLimit, IEnumerable<ICredential> credentials)
            : this(url, false, null, 0, threadsLimit, credentials)
        {
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="useProxy">Использовать прокси-сервер</param>
        /// <param name="proxyAddress">Адрес прокси-сервера</param>
        /// <param name="proxyPort">Порт прокси-сервера</param>
        /// <param name="threadsLimit">Лимит параллельных потоков</param>
        /// <param name="credentials">Коллекция учетных записей</param>
        public ExaminationServiceParallel(string url, bool useProxy, string proxyAddress, int proxyPort, int threadsLimit, IEnumerable<ICredential> credentials)
        {
            URL = url;
            UseProxy = useProxy;
            ProxyAddress = proxyAddress;
            ProxyPort = proxyPort;
            ThreadsLimit = threadsLimit;
            Credentials = credentials;
        }

        /// <summary>
        /// Загружает осмотры на портал диспансеризации. В случае возникновения исключений при загрузке осмотра - предпринимает несколько попыток.
        /// </summary>
        /// <param name="patientsExaminations">Список профилактических осмотров пациентов.</param>
        /// <returns>Список кортежей состоящий из PatientExaminations, флага успешной загрузки (true-успешно, false-иначе), строки с сообщением об ошибке.</returns>
        public List<Tuple<PatientExaminations, bool, string>> AddPatientsExaminations(List<PatientExaminations> patientsExaminations)
        {
            var threadsLimit = ThreadsLimit;

            if (patientsExaminations.Count < threadsLimit)
                threadsLimit = patientsExaminations.Count;

            var circularList = new CircularList<ICredential>(Credentials);
            var result = new ConcurrentBag<Tuple<PatientExaminations, bool, string>>();
            var tasks = new Task<ExaminationService>[threadsLimit];
            var counter = 0;
            //задержка потока перед обращением к веб-серверу, увеличивается при росте неудачных попыток, и уменьшается при росте удачных
            var sleepTime = 0;

            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => (ExaminationService)null);

            for (int i = 0; i < patientsExaminations.Count; i++)
            {
                var patientExaminations = patientsExaminations[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    var error = string.Empty;
                    var isSuccessful = true;

                    //3 попытки на загрузку осмотра, т.к. иногда веб-сервер обрывает сессии
                    for (int j = 1; j < 4; j++)
                    {
                        if (j != 1)
                            Interlocked.Add(ref sleepTime, 5000);

                        Thread.Sleep(sleepTime);

                        try
                        {
                            if (service == null)
                            {
                                service = new ExaminationService(URL, UseProxy, ProxyAddress, ProxyPort);

                                service.Authorize(circularList.GetNext());
                            }

                            service.AddPatientExaminations(patientExaminations);

                            //универсальный InterLocked-паттерн, потокобезопасно уменьшает sleepRate на 1 если он положительный
                            int initial, desired;

                            do
                            {
                                initial = sleepTime;
                                desired = initial;
                                if (desired >= 1000)
                                    desired -= 1000;
                            }
                            while (initial != Interlocked.CompareExchange(ref sleepTime, desired, initial));

                            break;
                        }
                        catch (HttpRequestException ex)
                        {
                            error = ex.Message;
                            isSuccessful = false;
                            service = null;
                        }
                        catch (InvalidOperationException ex)
                        {
                            error = ex.Message;
                            isSuccessful = false;
                        }
                        catch (WebServiceOperationException ex)
                        {
                            error = ex.Message;
                            isSuccessful = false;
                            break;
                        }
                    }

                    result.Add(new Tuple<PatientExaminations, bool, string>(patientExaminations, isSuccessful, error));
                    Interlocked.Increment(ref counter);
                    AddCounterChangeEvent(null, new CounterEventArgs(counter, patientsExaminations.Count));

                    return service;
                });
            }

            for (int i = 0; i < tasks.Length; i++)
            {
                var index = Task.WaitAny(tasks);

                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    service?.Logout();
                    return service;
                });
            }

            Task.WaitAll(tasks);

            return result.ToList();
        }
    }
}
