using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vertmarkets.Common.Contracts;

namespace Vertmarkets.Common.MSHttpClient
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponseMessage> PostContent(IConnectionInfo connectionInfo, string bodyContent, string contentType = "application/json");
    }
}
