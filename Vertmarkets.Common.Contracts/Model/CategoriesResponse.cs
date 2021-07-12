using System;
using System.Collections.Generic;
using System.Text;

namespace Vertmarkets.Common.Contracts.Model
{
    public class CategoriesResponse
    {
        public List<string> Data { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }

    }
}
