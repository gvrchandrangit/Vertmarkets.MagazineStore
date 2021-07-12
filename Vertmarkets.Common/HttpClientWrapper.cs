using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vertmarkets.Common.Contracts;

namespace Vertmarkets.Common.MSHttpClient
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly ILogger<HttpClientWrapper> _logger;        
        private readonly IHttpClientFactory _httpClientFactory;
        public HttpClientWrapper(ILogger<HttpClientWrapper> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> PostContent(IConnectionInfo connectionInfo, string bodyContent, string contentType = "application/json")
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("ConnectionInfo cannot be null");
            }

            HttpClient httpClient = PrepareHttpClient();            

            _logger.LogDebug($"Started building Http Request mesage for url {connectionInfo.HostUrl}");
            HttpRequestMessage httpRequestMessage = await GetHttpRequestMessage(bodyContent, contentType, connectionInfo);

            TimeSpan timeOut = connectionInfo.TimeOutInSeconds > 1 ? TimeSpan.FromSeconds(connectionInfo.TimeOutInSeconds) : httpClient.Timeout; //httpClient.Timeout
            _logger.LogDebug($"Timeout Value Inseconds: [{timeOut}]");
            System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource(timeOut);

            _logger.LogDebug($"Started sending request to url {connectionInfo.HostUrl}");

            var httpResponse = await httpClient.SendAsync(httpRequestMessage, cancellationTokenSource.Token);

            return httpResponse;
        }

        private async Task<HttpRequestMessage> GetHttpRequestMessage(string requestContent, string contentType, IConnectionInfo connectionInfo)
        {
            HttpMethod httpMethod = new HttpMethod(string.IsNullOrEmpty(connectionInfo.HttpVerb?.Trim()) ? "POST" : connectionInfo.HttpVerb);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpMethod, connectionInfo.HostUrl);

            httpRequestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

            // below condition added to avoid adding http body content e.g. HttpGet, Option
            if (httpMethod != HttpMethod.Post && httpMethod != HttpMethod.Put)
            {
                return httpRequestMessage;
            }

            httpRequestMessage.Content = new StringContent(requestContent, Encoding.UTF8);
            httpRequestMessage.Content.Headers.Remove("Content-Type");
            httpRequestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            return httpRequestMessage;
        }

        private HttpClient PrepareHttpClient()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("HttpClient");
            return httpClient;
        }
    }
}
