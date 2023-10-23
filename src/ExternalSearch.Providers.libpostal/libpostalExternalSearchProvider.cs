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

namespace CluedIn.ExternalSearch.Providers.Libpostal
{
    /// <summary>The Libpostal graph external search provider.</summary>
    /// <seealso cref="ExternalSearchProviderBase" />
    public class LibpostalExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider
    {
        private static readonly EntityType[] _acceptedEntityTypes = new EntityType[] { };

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        public LibpostalExternalSearchProvider()
            : base(Constants.ProviderId, entityTypes: _acceptedEntityTypes)
        {
        }


        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            foreach (var externalSearchQuery in InternalBuildQueries(context, request))
            {
                yield return externalSearchQuery;
            }
        }
        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config = null)
        {
            if (config.TryGetValue(Constants.KeyName.AcceptedEntityType, out var customType) && !string.IsNullOrWhiteSpace(customType?.ToString()))
            {
                if (!request.EntityMetaData.EntityType.Is(customType.ToString()))
                {
                    yield break;
                }
            }
            else if (!Accepts(request.EntityMetaData.EntityType))
                yield break;

            //var existingResults = request.GetQueryResults<LibpostalResponse>(this).ToList();

            //bool filter(string value)
            //{
            //	return existingResults.Any(r => string.Equals(r.Data.response., value, StringComparison.InvariantCultureIgnoreCase));
            //}

            var entityType = request.EntityMetaData.EntityType;

            var personAddress = GetValue(request, config, Constants.KeyName.PersonAddress, Core.Data.Vocabularies.Vocabularies.CluedInPerson.HomeAddress);
            var organizationAddress = GetValue(request, config, Constants.KeyName.OrganizationAddress, Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Address);
            var userAddress = GetValue(request, config, Constants.KeyName.UserAddress, Core.Data.Vocabularies.Vocabularies.CluedInUser.HomeAddress);
            var locationAddress = GetValue(request, config, Constants.KeyName.LocationAddress, Core.Data.Vocabularies.Vocabularies.CluedInLocation.Address);


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

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
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


        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            if (result is IExternalSearchQueryResult<LibpostalResponse> libpostalResult)
            {
                var code = GetOriginEntityCode(libpostalResult, request);
                var clue = new Clue(code, context.Organization);
                PopulateMetadata(clue.Data.EntityData, libpostalResult, request);
                return new[] { clue };
            }
            return null;
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            if (result is IExternalSearchQueryResult<LibpostalResponse> libpostalResult)
            {
                return CreateMetadata(libpostalResult, request);
            }
            return null;
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<LibpostalResponse> resultItem, IExternalSearchRequest request)
        {
            var metadata = new EntityMetadataPart();

            PopulateMetadata(metadata, resultItem, request);

            return metadata;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<LibpostalResponse> resultItem, IExternalSearchRequest request)
        {
            return new EntityCode(request.EntityMetaData.EntityType, GetCodeOrigin(), request.EntityMetaData.OriginEntityCode.Value);
        }

        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("libpostal");
        }


        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<LibpostalResponse> resultItem, IExternalSearchRequest request)
        {
            var code = GetOriginEntityCode(resultItem, request);

            metadata.EntityType = request.EntityMetaData.EntityType;

            //Name is required, without it the changes are ignored and not added to the entity.
            metadata.Name = request.EntityMetaData.Name;
            //metadata.Description = resultItem.Data.description;
            metadata.OriginEntityCode = code;
            metadata.Codes.Add(code);
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

            metadata.Codes.Add(code);
        }

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider)
        {
            return _acceptedEntityTypes;
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return InternalBuildQueries(context, request, config);
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            return ExecuteSearch(context, query);
        }

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return BuildClues(context, query, result, request);
        }

        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return GetPrimaryEntityMetadata(context, result, request);
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return GetPrimaryEntityPreviewImage(context, result, request);
        }

        public string Icon { get; } = Constants.Icon;
        public string Domain { get; } = Constants.Domain;
        public string About { get; } = Constants.About;

        public AuthMethods AuthMethods { get; } = Constants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = Constants.Properties;
        public Guide Guide { get; } = Constants.Guide;
        public IntegrationType Type { get; } = Constants.IntegrationType;
    }
}
