using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vertmarkets.Common.Contracts.Model;

namespace Vertmarkets.MagazineStore
{
    public class Processor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Processor> _logger;
        readonly AsyncTimeoutPolicy _timeoutPolicy;
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;        

        public Processor(IHttpClientFactory httpClientFactory, ILogger<Processor> logger)
        {
            this._logger = logger;
            this._httpClientFactory = httpClientFactory;

            _timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1200)); // throws TimeoutRejectedException if timeout of 1 second is exceeded

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(3, onRetry: OnRetry);
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, int retryCount)
        {
            if (delegateResult.Exception is HttpRequestException)
            {
                if (delegateResult.Exception.GetBaseException().Message == "The operation timed out")
                {
                    // log something about the timeout
                    _logger.LogInformation($" RequestException: {delegateResult.Exception.GetBaseException().Message}");
                }
            }
        }

        public async Task<string> GetAnswersResult()
        {
            string result = string.Empty;
            try
            {
                TokenResponse tokenResponse = await GetToken();
                List<MagazineSubscriberDetails> magSubscribed = new List<MagazineSubscriberDetails>();
                Answer answer = new Answer();
                if (tokenResponse.Success)
                {
                    CategoriesResponse categoriesResp = await GetCategories(tokenResponse.Token);
                    SubscriberResponse subscriberResp = await GetMagazineSubscribers(tokenResponse.Token);

                    if (categoriesResp.Success)
                    {
                        MagazinesResponse magResp = null;

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

                    //_logger.LogInformation($"Answer Body Content: {JsonConvert.SerializeObject(answer)}");

                    AnswerResponse answerResponse = await PostAnswers(tokenResponse.Token, JsonConvert.SerializeObject(answer));
                    result = JsonConvert.SerializeObject(answerResponse);
                }
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        public async Task<TokenResponse> GetToken()
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Start();

            TokenResponse apiResponse = null;            
            var httpClient = GetHttpClient();
            string requestEndpoint = $"token";

            HttpResponseMessage response =
                await                
                    _httpRetryPolicy.ExecuteAsync(() =>
                        _timeoutPolicy.ExecuteAsync(
                            async token => await httpClient.GetAsync(requestEndpoint, token), CancellationToken.None));

            if (response.IsSuccessStatusCode)
            {
                apiResponse = JsonConvert.DeserializeObject<TokenResponse>(await response?.Content.ReadAsStringAsync());
            }

            //watch.Stop();
            //_logger.LogInformation($"GetToken Execution Time: {watch.ElapsedMilliseconds} ms");

            return apiResponse;
        }

        public async Task<CategoriesResponse> GetCategories(string token)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Start();

            CategoriesResponse categoriesResponse = null;
            var httpClient = GetHttpClient();
            string requestEndpoint = $"categories/{token}";

            HttpResponseMessage response =
                await
                    _httpRetryPolicy.ExecuteAsync(() =>
                        _timeoutPolicy.ExecuteAsync(
                            async token => await httpClient.GetAsync(requestEndpoint, token), CancellationToken.None));

            if (response.IsSuccessStatusCode)
            {
                categoriesResponse = JsonConvert.DeserializeObject<CategoriesResponse>(await response?.Content.ReadAsStringAsync());
            }

            //watch.Stop();
            //_logger.LogInformation($"GetCategories Execution Time: {watch.ElapsedMilliseconds} ms");

            return categoriesResponse;
        }

        public async Task<MagazinesResponse> GetMagazines(string token, string magazine)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Start();

            MagazinesResponse magazinesResponse = null;
            var httpClient = GetHttpClient();
            string requestEndpoint = $"magazines/{token}/{magazine}";

            //HttpResponseMessage response =
            //    await
            //        _httpRetryPolicy.ExecuteAsync(() =>
            //            _timeoutPolicy.ExecuteAsync(
            //                async token => await httpClient.GetAsync(requestEndpoint, token), CancellationToken.None));

            TimeSpan timeOut = httpClient.Timeout;
            System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource(timeOut);
            HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint, cancellationTokenSource.Token);

            if (response.IsSuccessStatusCode)
            {
                magazinesResponse = JsonConvert.DeserializeObject<MagazinesResponse>(await response?.Content.ReadAsStringAsync());
            }

            //watch.Stop();
            //_logger.LogInformation($"GetMagazines Execution Time: {watch.ElapsedMilliseconds} ms");

            return magazinesResponse;
        }

        public async Task<SubscriberResponse> GetMagazineSubscribers(string token)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Start();

            SubscriberResponse subscriberResponse = null;
            var httpClient = GetHttpClient();
            string requestEndpoint = $"subscribers/{token}";

            //HttpResponseMessage response =
            //    await
            //        _httpRetryPolicy.ExecuteAsync(() =>
            //            _timeoutPolicy.ExecuteAsync(
            //                async token => await httpClient.GetAsync(requestEndpoint, token), CancellationToken.None));
            TimeSpan timeOut = httpClient.Timeout;
            System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource(timeOut);
            HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint, CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                subscriberResponse = JsonConvert.DeserializeObject<SubscriberResponse>(await response?.Content.ReadAsStringAsync());
            }

            //watch.Stop();
            //_logger.LogInformation($"GetMagazineSubscribers Execution Time: {watch.ElapsedMilliseconds} ms");

            return subscriberResponse;
        }

        public async Task<AnswerResponse> PostAnswers(string token, string answer)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //watch.Start();

            AnswerResponse asnwerResponse = null;
            var httpClient = GetHttpClient();
            string requestEndpoint = $"answer/{token}";
            HttpContent httpContent = new StringContent(answer, Encoding.UTF8, "application/json");

            HttpResponseMessage response =
                await
                    _httpRetryPolicy.ExecuteAsync(() =>
                        _timeoutPolicy.ExecuteAsync(
                            async token => await httpClient.PostAsync(requestEndpoint, httpContent), CancellationToken.None));

            if (response.IsSuccessStatusCode)
            {
                asnwerResponse = JsonConvert.DeserializeObject<AnswerResponse>(await response?.Content.ReadAsStringAsync());
            }

            //watch.Stop();
            //_logger.LogInformation($"PostAnswers Execution Time: {watch.ElapsedMilliseconds} ms");

            return asnwerResponse;
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(@"http://magazinestore.azurewebsites.net/api/")
            };
            //var httpClient = _httpClientFactory.CreateClient("HttpClient");
            httpClient.BaseAddress = new Uri(@"http://magazinestore.azurewebsites.net/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
