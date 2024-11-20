using System;
using System.Collections.Generic;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;

namespace CluedIn.ExternalSearch.Providers.Libpostal
{
    public static class Constants
    {
        public const string ComponentName = "Libpostal";
        public const string ProviderName = "Libpostal";
        public static readonly Guid ProviderId = Guid.Parse("aba4e4cf-3c48-4828-9fdf-990b22e1a29c");
        public const string Instruction = """
            [
              {
                "type": "bulleted-list",
                "children": [
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the entity type to specify the golden records you want to enrich. Only golden records belonging to that entity type will be enriched."
                      }
                    ]
                  },
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the vocabulary keys to provide the input for the enricher to search for additional information. For example, if you provide the website vocabulary key for the Web enricher, it will use specific websites to look for information about companies. In some cases, vocabulary keys are not required. If you don't add them, the enricher will use default vocabulary keys."
                      }
                    ]
                  }
                ]
              }
            ]
            """;
        public struct KeyName
        {
            public const string AcceptedEntityType = "acceptedEntityType";
            public const string PersonAddress = "personAddress";
            public const string OrganizationAddress = "organizationAddress";
            public const string UserAddress = "userAddress";
            public const string LocationAddress = "locationAddress";

        }
        public static string About { get; set; } = "Libpostal allows for parsing/normalizing street addresses around the world using statistical NLP and open data";
        public static string Icon { get; set; } = "Resources.cluedin.png";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            Token = new List<Control>()
            {
                new Control()
                {
                    DisplayName = "Accepted Entity Type",
                    Type = "input",
                    IsRequired = true,
                    Name = KeyName.AcceptedEntityType,
                    Help = "The entity type that defines the golden records you want to enrich (e.g., /Organization)."
                },
                new Control()
                {
                    DisplayName = "Person Address Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.PersonAddress,
                    Help = "The vocabulary key that contains the addresses of persons you want to enrich (e.g., person.home.address.city)."
                },
                new Control()
                {
                    DisplayName = "Organization Address Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.OrganizationAddress,
                    Help = "The vocabulary key that contains the addresses of companies you want to enrich (e.g., organization.address)."
                },
                new Control()
                {
                    DisplayName = "User Address Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.UserAddress,
                    Help = "The vocabulary key that contains the addresses of users you want to enrich (e.g., user.home.address)."
                },
                new Control()
                {
                    DisplayName = "Location Address Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.LocationAddress,
                    Help = "The vocabulary key that contains the addresses of locations you want to enrich (e.g., location.fullAddress)."
                },
            }
        };

        public static IEnumerable<Control> Properties { get; set; } = new List<Control>()
        {
            // NOTE: Leaving this commented as an example - BF
            //new()
            //{
            //    displayName = "Some Data",
            //    type = "input",
            //    isRequired = true,
            //    name = "someData"
            //}
        };

        public static Guide Guide { get; set; } = null;
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
