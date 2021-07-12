using System;
using System.Collections.Generic;
using System.Text;

namespace Vertmarkets.Common.Contracts.Model
{
    public class ConnectionInfo : IConnectionInfo
    {
        public string HostUrl { get; set; }
        public string HttpVerb { get; set; }
        public int TimeOutInSeconds { get; set; }
    }
}
