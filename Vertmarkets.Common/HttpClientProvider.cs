using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Vertmarkets.Common.MSHttpClient
{
    public class HttpClientProvider : IHttpClientProvider
    {
        public HttpClient GetHttpClient(DelegatingHandler[] delegatingHandlers, HttpMessageHandler httpMessageHandler)
        {
            if (httpMessageHandler == null)
            {
                return HttpClientFactory.Create(delegatingHandlers);
            }

            return HttpClientFactory.Create(httpMessageHandler, delegatingHandlers);
        }
    }
}
