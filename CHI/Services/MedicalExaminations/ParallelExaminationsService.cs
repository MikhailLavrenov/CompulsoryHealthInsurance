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
            var sleepTime = 0;

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    ExaminationService service = null;

                    while (patientsExaminationsStack.TryPop(out var patientExaminations))
                    {
                        string error = string.Empty;
                        bool isSuccessful = true;

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
                                    service = new ExaminationService(address, useProxy, proxyAddress, proxyPort);
                                    await service.AuthorizeAsync(credential);
                                }

                                await service.AddPatientExaminationsAsync(patientExaminations);
                                error = string.Empty;
                                isSuccessful = true;

                                if (sleepTime != 0)
                                {
                                    //универсальный InterLocked-паттерн, потокобезопасно уменьшает sleepRate если он положительный
                                    int initial, desired;

                                    do
                                    {
                                        initial = sleepTime;
                                        desired = initial;
                                        if (desired >= 1000)
                                            desired -= 1000;
                                        else if (desired > 0)
                                            desired = 0;
                                    }
                                    while (initial != Interlocked.CompareExchange(ref sleepTime, desired, initial));
                                }

                                break;
                            }
                            catch (HttpRequestException ex)
                            {
                                error = ex.Message;
                                isSuccessful = false;
                                service = null;
                            }
                            //??????????????????????????
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

                        loadedExaminations.Add((patientExaminations, isSuccessful, error));

                        OnProgressChanged(loadedExaminations.Count);
                    }
                });
            }

            await Task.WhenAll(tasks);

            return loadedExaminations.ToList();
        }
    }
}
