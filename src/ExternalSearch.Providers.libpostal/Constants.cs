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

        public static string About { get; set; } = "Libpostal allows for parsing/normalizing street addresses around the world using statistical NLP and open data";
        public static string Icon { get; set; } = "Resources.logo.svg";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            token = new List<Control>()
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
