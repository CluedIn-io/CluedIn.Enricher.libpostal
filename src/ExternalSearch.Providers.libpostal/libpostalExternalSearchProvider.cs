using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Castle.Core.Internal;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using EntityType = CluedIn.Core.Data.EntityType;
using CluedIn.ExternalSearch.Providers.Libpostal.Vocabularies;
using CluedIn.ExternalSearch.Providers.Libpostal.Models;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.Connectors;
using System.Text.RegularExpressions;

namespace CluedIn.ExternalSearch.Providers.Libpostal
{
    /// <summary>The Libpostal graph external search provider.</summary>
    /// <seealso cref="ExternalSearchProviderBase" />
    public class LibpostalExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider, IExternalSearchProviderWithVerifyConnection
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        private static readonly EntityType[] DefaultAcceptedEntityTypes = { };

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        public LibpostalExternalSearchProvider()
            : base(Constants.ProviderId, entityTypes: DefaultAcceptedEntityTypes)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider) => this.Accepts(config);

        private IEnumerable<EntityType> Accepts(IDictionary<string, object> config)
            => Accepts(new LibpostalExternalSearchJobData(config));

        private IEnumerable<EntityType> Accepts(LibpostalExternalSearchJobData config)
        {
            if (!string.IsNullOrWhiteSpace(config.AcceptedEntityType))
            {
                // If configured, only accept the configured entity types
                return new EntityType[] { config.AcceptedEntityType };
            }

            // Fallback to default accepted entity types
            return DefaultAcceptedEntityTypes;
        }

        private bool Accepts(LibpostalExternalSearchJobData config, EntityType entityTypeToEvaluate)
        {
            var configurableAcceptedEntityTypes = this.Accepts(config).ToArray();

            return configurableAcceptedEntityTypes.Any(entityTypeToEvaluate.Is);
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
            => InternalBuildQueries(context, request, new LibpostalExternalSearchJobData(config));

        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, LibpostalExternalSearchJobData config)
        {
            if (!this.Accepts(config, request.EntityMetaData.EntityType))
                yield break;

            //var existingResults = request.GetQueryResults<LibpostalResponse>(this).ToList();

            //bool filter(string value)
            //{
            //	return existingResults.Any(r => string.Equals(r.Data.response., value, StringComparison.InvariantCultureIgnoreCase));
            //}

            var entityType = request.EntityMetaData.EntityType;

            var configMap           = config.ToDictionary();
            var personAddress       = GetValue(request, configMap, Constants.KeyName.PersonAddress, Core.Data.Vocabularies.Vocabularies.CluedInPerson.HomeAddress);
            var organizationAddress = GetValue(request, configMap, Constants.KeyName.OrganizationAddress, Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Address);
            var userAddress         = GetValue(request, configMap, Constants.KeyName.UserAddress, Core.Data.Vocabularies.Vocabularies.CluedInUser.HomeAddress);
            var locationAddress     = GetValue(request, configMap, Constants.KeyName.LocationAddress, Core.Data.Vocabularies.Vocabularies.CluedInLocation.Address);


            if (personAddress != null && personAddress.Count > 0)
            {
                foreach (var item in personAddress)
                {
                    var queryBody = new Dictionary<string, string>
                    {
                        {"body", item }
                    };
                    yield return new ExternalSearchQuery(this, entityType, queryBody);
                }
            }
            if (organizationAddress != null && organizationAddress.Count > 0)
            {
                foreach (var item in organizationAddress)
                {
                    var queryBody = new Dictionary<string, string>
                    {
                        {"body", item }
                    };
                    yield return new ExternalSearchQuery(this, entityType, queryBody);
                }
            }
            if (userAddress != null && userAddress.Count > 0)
            {
                foreach (var item in userAddress)
                {
                    var queryBody = new Dictionary<string, string>
                    {
                        {"body", item }
                    };
                    yield return new ExternalSearchQuery(this, entityType, queryBody);
                }
            }
            if (locationAddress != null && locationAddress.Count > 0)
            {
                foreach (var item in locationAddress)
                {
                    var queryBody = new Dictionary<string, string>
                    {
                        {"body", item }
                    };
                    yield return new ExternalSearchQuery(this, entityType, queryBody);
                }
            }
        }

        private static HashSet<string> GetValue(IExternalSearchRequest request, IDictionary<string, object> config, string keyName, VocabularyKey defaultKey)
        {
            HashSet<string> value;
            if (config.TryGetValue(keyName, out var customVocabKey) && !string.IsNullOrWhiteSpace(customVocabKey?.ToString()))
            {
                value = request.QueryParameters.GetValue<string, HashSet<string>>(customVocabKey.ToString(), new HashSet<string>());
            }
            else
            {
                value = request.QueryParameters.GetValue(defaultKey, new HashSet<string>());
            }

            return value;
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            var url = ConfigurationManagerEx.AppSettings.GetValue("ExternalSearch.Libpostal.url", "");
            if (url.IsNullOrEmpty())
            {
                throw new Exception("Bad configuration");
            }

            var client = new RestClient(url);
            var request = new RestRequest("parser", Method.POST);
            string address = null;
            request.AddHeader("Content-type", "application/json");
            if (query.QueryParameters.ContainsKey("body"))
            {
                address = query.QueryParameters["body"].First();
            }
            else
            {
                throw new Exception("Wrong query");
            }


            request.AddJsonBody(new queryBody() { query = address });

            var response = client.ExecuteTaskAsync<LibpostalResponse>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Content != null)
                {
                    var data = new LibpostalResponse();
                    foreach (var item in JsonConvert.DeserializeObject<List<Items>>(response.Content))
                    {
                        data.Items.Add(item);
                    }
                    yield return new ExternalSearchQueryResult<LibpostalResponse>(query, data);
                }
            }
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                yield break;
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
        }


        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            if (result is IExternalSearchQueryResult<LibpostalResponse> libpostalResult)
            {
                var code = new EntityCode(request.EntityMetaData.OriginEntityCode.Type, "libpostal", $"{query.QueryKey}{request.EntityMetaData.OriginEntityCode}".ToDeterministicGuid());
                var clue = new Clue(code, context.Organization);
                PopulateMetadata(clue.Data.EntityData, libpostalResult, request);
                return new[] { clue };
            }
            return null;
        }

        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            if (result is IExternalSearchQueryResult<LibpostalResponse> libpostalResult)
            {
                return CreateMetadata(libpostalResult, request);
            }

            return null;
        }

        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            return null;
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return null;
        }

        public ConnectionVerificationResult VerifyConnection(ExecutionContext context, IReadOnlyDictionary<string, object> config)
        {
            var url = ConfigurationManagerEx.AppSettings.GetValue("ExternalSearch.Libpostal.url", "");
            if (url.IsNullOrEmpty())
            {
                return new ConnectionVerificationResult(false, "Bad configuration: Invalid url");
            }

            var client = new RestClient(url);
            var request = new RestRequest("parser", Method.POST);
            var address = "Belgrave House, 76 Buckingham Palace Road";
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new queryBody() { query = address });

            var response = client.ExecuteAsync<LibpostalResponse>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
            {
                return new ConnectionVerificationResult(true, string.Empty);
            }

            return ConstructVerifyConnectionResponse(response);
        }

        private ConnectionVerificationResult ConstructVerifyConnectionResponse(IRestResponse response)
        {
            var errorMessageBase = $"{Constants.ProviderName} returned \"{(int)response.StatusCode} {response.StatusDescription}\".";
            if (response.ErrorException != null)
            {
                return new ConnectionVerificationResult(false, $"{errorMessageBase} {(!string.IsNullOrWhiteSpace(response.ErrorException.Message) ? response.ErrorException.Message : "This could be due to breaking changes in the external system")}.");
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized)
            {
                return new ConnectionVerificationResult(false, $"{errorMessageBase} This could be due to invalid API key.");
            }

            var regex = new Regex(@"\<(html|head|body|div|span|img|p\>|a href)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            var isHtml = regex.IsMatch(response.Content);

            var errorMessage = response.IsSuccessful ? string.Empty
                : string.IsNullOrWhiteSpace(response.Content) || isHtml
                    ? $"{errorMessageBase} This could be due to breaking changes in the external system."
                    : $"{errorMessageBase} {response.Content}.";

            return new ConnectionVerificationResult(response.IsSuccessful, errorMessage);
        }

        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<LibpostalResponse> resultItem, IExternalSearchRequest request)
        {
            var metadata = new EntityMetadataPart();

            PopulateMetadata(metadata, resultItem, request);

            return metadata;
        }

        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<LibpostalResponse> resultItem, IExternalSearchRequest request)
        {
            var code = new EntityCode(request.EntityMetaData.OriginEntityCode.Type, "libpostal", $"{request.Queries.FirstOrDefault()?.QueryKey}{request.EntityMetaData.OriginEntityCode}".ToDeterministicGuid());

            metadata.EntityType = request.EntityMetaData.EntityType;

            //Name is required, without it the changes are ignored and not added to the entity.
            metadata.Name = request.EntityMetaData.Name;
            //metadata.Description = resultItem.Data.description;
            metadata.OriginEntityCode = code;
            metadata.Codes.Add(request.EntityMetaData.OriginEntityCode);

            foreach (var item in resultItem.Data.Items)
            {
                switch (item.label)
                {
                    case "house":
                        metadata.Properties[LibpostalVocabulary.Location.House] = item.value.ToTitleCase();
                        break;
                    case "category":
                        metadata.Properties[LibpostalVocabulary.Location.Category] = item.value.ToTitleCase();
                        break;
                    case "near":
                        metadata.Properties[LibpostalVocabulary.Location.Near] = item.value.ToTitleCase();
                        break;
                    case "house_number":
                        metadata.Properties[LibpostalVocabulary.Location.House_number] = item.value.ToTitleCase();
                        break;
                    case "road":
                        metadata.Properties[LibpostalVocabulary.Location.Road] = item.value.ToTitleCase();
                        break;
                    case "unit":
                        metadata.Properties[LibpostalVocabulary.Location.Unit] = item.value.ToTitleCase();
                        break;
                    case "level":
                        metadata.Properties[LibpostalVocabulary.Location.Level] = item.value.ToTitleCase();
                        break;
                    case "staircase":
                        metadata.Properties[LibpostalVocabulary.Location.Staircase] = item.value.ToTitleCase();
                        break;
                    case "entrance":
                        metadata.Properties[LibpostalVocabulary.Location.Entrance] = item.value.ToTitleCase();
                        break;
                    case "po_box":
                        metadata.Properties[LibpostalVocabulary.Location.Po_box] = item.value.ToTitleCase();
                        break;
                    case "postcode":
                        metadata.Properties[LibpostalVocabulary.Location.Postcode] = item.value.ToTitleCase();
                        break;
                    case "suburb":
                        metadata.Properties[LibpostalVocabulary.Location.Suburb] = item.value.ToTitleCase();
                        break;
                    case "city_district":
                        metadata.Properties[LibpostalVocabulary.Location.City_district] = item.value.ToTitleCase();
                        break;
                    case "city":
                        metadata.Properties[LibpostalVocabulary.Location.City] = item.value.ToTitleCase();
                        break;
                    case "island":
                        metadata.Properties[LibpostalVocabulary.Location.Island] = item.value.ToTitleCase();
                        break;
                    case "state_district":
                        metadata.Properties[LibpostalVocabulary.Location.State_district] = item.value.ToTitleCase();
                        break;
                    case "state":
                        metadata.Properties[LibpostalVocabulary.Location.State] = item.value.ToTitleCase();
                        break;
                    case "country_region":
                        metadata.Properties[LibpostalVocabulary.Location.Country_region] = item.value.ToTitleCase();
                        break;
                    case "country":
                        metadata.Properties[LibpostalVocabulary.Location.Country] = item.value.ToTitleCase();
                        break;
                    case "world_region":
                        metadata.Properties[LibpostalVocabulary.Location.World_region] = item.value.ToTitleCase();
                        break;
                }
            }
        }

        // Since this is a configurable external search provider, theses methods should never be called
        public override bool Accepts(EntityType entityType) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query) => throw new NotSupportedException();
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request) => throw new NotSupportedException();

        /**********************************************************************************************************
         * PROPERTIES
         **********************************************************************************************************/

        public string Icon { get; } = Constants.Icon;
        public string Domain { get; } = Constants.Domain;
        public string About { get; } = Constants.About;

        public AuthMethods AuthMethods { get; } = Constants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = Constants.Properties;
        public Guide Guide { get; } = Constants.Guide;
        public IntegrationType Type { get; } = Constants.IntegrationType;
    }
}
