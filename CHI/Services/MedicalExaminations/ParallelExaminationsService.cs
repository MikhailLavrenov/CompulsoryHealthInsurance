using CHI.Models;
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
    public class ParallelExaminationsService : ParallelWebServiceBase
    {
        static readonly int increaseWaiting = 5000;
        static readonly int decreaseWaiting = 1000;

        public ParallelExaminationsService(string address, ICredential credential, int maxDegreeOfParallelism)
            : base(address, credential, maxDegreeOfParallelism)
        {
        }

        /// <summary>
        /// Загружает осмотры на портал диспансеризации. В случае возникновения исключений при загрузке осмотра - предпринимает несколько попыток.
        /// </summary>
        /// <param name="patientsExaminations">Список профилактических осмотров пациентов.</param>
        /// <returns>Список кортежей состоящий из PatientExaminations, флага успешной загрузки (true-успешно, false-иначе), строки с сообщением об ошибке.</returns>
        public async Task<List<(PatientExaminations, bool, string)>> AddExaminationsAsync(IEnumerable<PatientExaminations> patientsExaminations)
        {
            var loadedExaminations = new ConcurrentBag<(PatientExaminations PatientExaminations, bool IsLoaded, string Error)>();
            var tasks = new Task[Math.Min(maxDegreeOfParallelism, patientsExaminations.Count())];
            var patientsExaminationsStack = new ConcurrentStack<PatientExaminations>(patientsExaminations);

            //задержка потока перед обращением к веб-серверу, увеличивается при росте неудачных попыток, и уменьшается при росте удачных
            var waitTime = 0;

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    ExaminationService service = null;

                    while (patientsExaminationsStack.TryPop(out var patientExaminations))
                    {
                        string error = string.Empty;
                        bool isSuccessful = false;

                        //3 попытки на загрузку осмотра, т.к. иногда веб-сервер обрывает сессии
                        for (int j = 0; j < 3; j++)
                        {
                            await Task.Delay(waitTime);

                            try
                            {
                                if (service == null || j == 2)
                                {
                                    service = new ExaminationService(address, useProxy, proxyAddress, proxyPort);
                                    await service.AuthorizeAsync(credential);
                                }

                                await service.AddPatientExaminationsAsync(patientExaminations);
                                error = string.Empty;
                                isSuccessful = true;

                                DecreaseWaitTimeThreadSafety(ref waitTime);

                                break;
                            }
                            catch (HttpRequestException ex)
                            {
                                error = ex.Message;
                                Interlocked.Add(ref waitTime, increaseWaiting);
                            }
                            catch (WebServiceOperationException ex)
                            {
                                error = ex.Message;
                                break;
                            }
                        }

                        loadedExaminations.Add((patientExaminations, isSuccessful, error));

                        OnProgressChanged(loadedExaminations.Count);
                    }
                });
            }

            await Task.WhenAll(tasks);

            return loadedExaminations.ToList();
        }

        void DecreaseWaitTimeThreadSafety(ref int value)
        {
            if (value == 0)
                return;

            //универсальный InterLocked-паттерн, потокобезопасно уменьшает waitTime
            int initial, desired;

            do
            {
                initial = value;
                desired = initial < decreaseWaiting ? 0 : initial - decreaseWaiting;
            }
            while (initial != Interlocked.CompareExchange(ref value, desired, initial));
        }
    }
}
