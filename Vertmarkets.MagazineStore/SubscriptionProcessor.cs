#if Commented
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Vertmarkets.Common.Contracts.Model;
using Vertmarkets.Common.MSHttpClient;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Polly;

namespace Vertmarkets.MagazineStore
{

    public class SubscriptionProcessor
    {
        private readonly ILogger<SubscriptionProcessor> _logger;
        private readonly IHttpClientWrapper _httpClientWrapper;

        public SubscriptionProcessor(ILogger<SubscriptionProcessor> logger, IHttpClientWrapper httpClientWrapper)
        {
            _logger = logger;
            _httpClientWrapper = httpClientWrapper;
            _logger.LogInformation("In Application::ctor");
        }
            
        public async Task<string> GetAnswersResult()
        {
            string result = string.Empty;

            //TokenResponse tokenResponse1 = null;
            //for (int i=0; i < 20; i++)
            //{
            //    _logger.LogInformation($"GetToken loop: {i}");
            //    tokenResponse1 = await GenerateAuthToken();                
            //}
            //while(true)
            //    return result;

            TokenResponse tokenResponse = await GenerateAuthToken();            
            List<MagazineSubscriberDetails> magSubscribed = new List<MagazineSubscriberDetails>();
            Answer answer = new Answer();
            if (tokenResponse.Success)
            {
                CategoriesResponse categoriesResp = await GetCategories(tokenResponse.Token);
                SubscriberResponse subscriberResp = await GetMagazineSubscribers(tokenResponse.Token);

                if (categoriesResp.Success)
                {
                    MagazinesResponse magResp = null;

                    //foreach (var c in categoriesResp.Data) await AsyncMethod(c);

                    foreach (var c in categoriesResp.Data)
                    {
                        magResp = await GetMagazines(tokenResponse.Token, c);

                        magResp.Data.ForEach(m =>
                        {
                            magSubscribed.AddRange(subscriberResp.Data.Where(s => s.MagazineIds.Contains(m.Id))
                                .Select(s => new MagazineSubscriberDetails(c, s.Id))
                                .ToList());
                        });
                    }
                }
            }

            if (magSubscribed.Count > 0)
            {
                answer.Subscribers = new List<string>();

                var subscriberList = magSubscribed.Distinct(new ObjectComparer()).ToList().GroupBy(g => g.SubscriberId)
                    .Select(g => Tuple.Create(g.Key, g.Count()))
                    .OrderByDescending(g => g.Item2)
                    .ToList();

                var maxCount = subscriberList.Select(r => r.Item2).FirstOrDefault();

                subscriberList.ForEach(t =>
                {
                    if (maxCount == t.Item2)
                        answer.Subscribers.Add(t.Item1);
                });

                _logger.LogInformation($"Answer Body Content: {JsonConvert.SerializeObject(answer)}");

                AnswerResponse answerResponse = await PostAnswers(tokenResponse.Token, JsonConvert.SerializeObject(answer));
                result = JsonConvert.SerializeObject(answerResponse);
            }

            return result;
        }

        //private Task AsyncMethod(string c)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<AnswerResponse> PostAnswers(string token, string answer)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Start();

            AnswerResponse asnwerResponse = null;
            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                HostUrl = $"http://magazinestore.azurewebsites.net/api/answer/{token}",
                HttpVerb = HttpMethod.Post.ToString(),
                TimeOutInSeconds = 120,
            };


            HttpResponseMessage httpResponseMessage = await _httpClientWrapper.PostContent(connectionInfo, answer);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string response = await httpResponseMessage?.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(response) == false)
                {
                    asnwerResponse = JsonConvert.DeserializeObject<AnswerResponse>(response);
                    _logger.LogInformation($"Asnwer Response: {JsonConvert.SerializeObject(asnwerResponse)}");
                }
            }

            watch.Stop();
            _logger.LogInformation($"PostAnswers Execution Time: {watch.ElapsedMilliseconds} ms");

            return asnwerResponse;
        }

        public async Task<SubscriberResponse> GetMagazineSubscribers(string token)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Start();

            SubscriberResponse subscriberResponse = null;
            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                HostUrl = $"http://magazinestore.azurewebsites.net/api/subscribers/{token}",
                HttpVerb = HttpMethod.Get.ToString(),
                TimeOutInSeconds = 120,
            };


            HttpResponseMessage httpResponseMessage = await _httpClientWrapper.PostContent(connectionInfo, string.Empty);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string response = await httpResponseMessage?.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(response) == false)
                {
                    subscriberResponse = JsonConvert.DeserializeObject<SubscriberResponse>(response);
                    _logger.LogInformation($"Subscribers List: {JsonConvert.SerializeObject(subscriberResponse.Data)}");
                }
            }

            watch.Stop();
            _logger.LogInformation($"GetMagazineSubscribers Execution Time: {watch.ElapsedMilliseconds} ms");

            return subscriberResponse;
        }

        public async Task<MagazinesResponse> GetMagazines(string token, string magazine)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Start();

            MagazinesResponse magazinesResponse = null;
            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                HostUrl = $"http://magazinestore.azurewebsites.net/api/magazines/{token}/{magazine}",
                HttpVerb = HttpMethod.Get.ToString(),
                TimeOutInSeconds = 120,
            };


            HttpResponseMessage httpResponseMessage = await _httpClientWrapper.PostContent(connectionInfo, string.Empty);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string response = await httpResponseMessage?.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(response) == false)
                {
                    magazinesResponse = JsonConvert.DeserializeObject<MagazinesResponse>(response);
                    _logger.LogInformation($"Magazines List: {JsonConvert.SerializeObject(magazinesResponse.Data)}");
                }
            }

            watch.Stop();
            _logger.LogInformation($"GetMagazines Execution Time: {watch.ElapsedMilliseconds} ms");

            return magazinesResponse;
        }

        public async Task<CategoriesResponse> GetCategories(string token)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Start();

            CategoriesResponse categoriesResponse = null;
            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                HostUrl = $"http://magazinestore.azurewebsites.net/api/categories/{token}",
                HttpVerb = HttpMethod.Get.ToString(),
                TimeOutInSeconds = 120,
            };

            //Task<HttpResponseMessage> t = Task.Run(() => _httpClientWrapper.PostContent(connectionInfo, string.Empty));
            //t.Wait();

            //string result = t.Result.Content.ReadAsStringAsync().Result;
            //_logger.LogInformation($"Categories: {result}");

            HttpResponseMessage httpResponseMessage = await _httpClientWrapper.PostContent(connectionInfo, string.Empty);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string response = await httpResponseMessage?.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(response) == false)
                {
                    categoriesResponse = JsonConvert.DeserializeObject<CategoriesResponse>(response);
                    _logger.LogInformation($"Categories List: {JsonConvert.SerializeObject(categoriesResponse.Data)}");
                }
            }

            watch.Stop();
            _logger.LogInformation($"GetCategories Execution Time: {watch.ElapsedMilliseconds} ms");

            return categoriesResponse;
        }


        public async Task<TokenResponse> GenerateAuthToken()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Start();

            TokenResponse apiResponse = null;
            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                HostUrl = "http://magazinestore.azurewebsites.net/api/token",
                HttpVerb = HttpMethod.Get.ToString(),
                TimeOutInSeconds = 1,
            };

            //Task<HttpResponseMessage> t = Task.Run(() => _httpClientWrapper.PostContent(connectionInfo, string.Empty, "application/json"));
            //t.Wait();

            //string token = t.Result.Content.ReadAsStringAsync().Result;

            HttpResponseMessage httpResponseMessage = await _httpClientWrapper.PostContent(connectionInfo, string.Empty);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string response = await httpResponseMessage?.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(response) == false)
                {
                    apiResponse = JsonConvert.DeserializeObject<TokenResponse>(response);
                }
            }

            watch.Stop();
            _logger.LogInformation($"GenerateAuthToken Execution Time: {watch.ElapsedMilliseconds} ms");
            return apiResponse;
        }

        public static Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func)
        {
            return Policy.Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(5, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                    //do some logging
                    })
                .ExecuteAsync<TResult>(func); // This is an async-await-eliding contraction of: .ExecuteAsync<TResult>(async () => await func());
        }
    }
}
#endif
