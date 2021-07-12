using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vertmarkets.MagazineStore
{
    public class MagazineSubscriberDetails
    {
        public string Category { get; set; }       
        public string SubscriberId {get; set; }

        public MagazineSubscriberDetails(string category, string subsciberId)
        {
            Category = category;
            SubscriberId = subsciberId;
        }
    }

    public class ObjectComparer : IEqualityComparer<MagazineSubscriberDetails>
    {
        public bool Equals(MagazineSubscriberDetails x, MagazineSubscriberDetails y)
        {
            return x.Category == y.Category && x.SubscriberId == y.SubscriberId;
        }

        public int GetHashCode(MagazineSubscriberDetails obj)
        {
            if (obj is null) return 0;
            int hashCat = obj.Category == null ? 0 : obj.Category.GetHashCode();
            int hashSubId = obj.SubscriberId == null ? 0 : obj.SubscriberId.GetHashCode();
            return hashCat ^ hashSubId;
        }
    }
}
