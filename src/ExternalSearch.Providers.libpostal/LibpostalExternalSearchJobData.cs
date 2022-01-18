using System.Collections.Generic;
using CluedIn.Core.Crawling;

namespace CluedIn.ExternalSearch.Providers.Libpostal
{
    public class LibpostalExternalSearchJobData : CrawlJobData
    {
        public LibpostalExternalSearchJobData(IDictionary<string, object> configuration)
        {
           
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>();
        }
    }
}
