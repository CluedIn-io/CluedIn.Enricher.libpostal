using System.Collections.Generic;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;

namespace CluedIn.ExternalSearch.Providers.Libpostal
{
    public class LibpostalExternalSearchJobData : CrawlJobData
    {
        public LibpostalExternalSearchJobData(IDictionary<string, object> configuration)
        {
            AcceptedEntityType = GetValue<string>(configuration, Constants.KeyName.AcceptedEntityType);
            PersonAddress = GetValue<string>(configuration, Constants.KeyName.PersonAddress);
            OrganizationAddress = GetValue<string>(configuration, Constants.KeyName.OrganizationAddress);
            UserAddress = GetValue<string>(configuration, Constants.KeyName.UserAddress);
            LocationAddress = GetValue<string>(configuration, Constants.KeyName.LocationAddress);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { Constants.KeyName.AcceptedEntityType, AcceptedEntityType },
                { Constants.KeyName.PersonAddress, PersonAddress },
                { Constants.KeyName.OrganizationAddress, OrganizationAddress },
                { Constants.KeyName.UserAddress, UserAddress },
                { Constants.KeyName.LocationAddress, LocationAddress },
            };
        }
        public string AcceptedEntityType { get; set; }
        public string PersonAddress { get; set; }
        public string OrganizationAddress { get; set; }
        public string UserAddress { get; set; }
        public string LocationAddress { get; set; }
    }
}
