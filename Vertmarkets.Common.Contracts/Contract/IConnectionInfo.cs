using System;

namespace Vertmarkets.Common.Contracts
{
    public interface IConnectionInfo
    {
        /// <summary>
        /// Gets or sets the Host Url
        /// </summary>
        string HostUrl { get; set; }

        /// <summary>
        /// Gets or sets the Http Verb
        /// </summary>
        string HttpVerb { get; set; }        

        /// <summary>
        /// Gets or sets the Time Out In Seconds
        /// </summary>
        int TimeOutInSeconds { get; set; }
    }
}
