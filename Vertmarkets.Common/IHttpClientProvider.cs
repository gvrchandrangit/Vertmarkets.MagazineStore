using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Vertmarkets.Common.MSHttpClient
{
    public interface IHttpClientProvider
    {
        HttpClient GetHttpClient(DelegatingHandler[] delegatingHandlers, HttpMessageHandler httpMessageHandler);
    }
}
