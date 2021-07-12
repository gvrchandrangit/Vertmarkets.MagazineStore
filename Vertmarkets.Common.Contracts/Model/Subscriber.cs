using System;
using System.Collections.Generic;
using System.Text;

namespace Vertmarkets.Common.Contracts.Model
{
    public class Subscriber
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<int> MagazineIds { get; set; }
    }
}
