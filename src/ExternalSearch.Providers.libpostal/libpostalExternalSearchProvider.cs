using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Configuration;
using CluedIn.ExternalSearch.Providers.libpostal.Models;
using CluedIn.ExternalSearch.Providers.libpostal.Vocabularies;
using Newtonsoft.Json;
using RestSharp;
using Castle.Core.Internal;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using EntityType = CluedIn.Core.Data.EntityType;
using Constants = CluedIn.ExternalSearch.Providers.Libpostal.Constants;

namespace CluedIn.ExternalSearch.Providers.libpostal
{
    /// <summary>The libpostal graph external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class libpostalExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider
    {
        private static readonly EntityType[] AcceptedEntityTypes = new EntityType[] { EntityType.Person, EntityType.Organization, EntityType.Infrastructure.User, EntityType.Location, EntityType.Location.Address, "/LegalEntity" };

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        public libpostalExternalSearchProvider()
            : base(Constants.ProviderId, entityTypes: AcceptedEntityTypes)
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
            if (!Accepts(request.EntityMetaData.EntityType))
                yield break;

            //var existingResults = request.GetQueryResults<libpostalResponse>(this).ToList();

            //bool filter(string value)
            //{
            //	return existingResults.Any(r => string.Equals(r.Data.response., value, StringComparison.InvariantCultureIgnoreCase));
            //}

            var entityType = request.EntityMetaData.EntityType;

            var personAddress = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInPerson.HomeAddress, new HashSet<string>());
            var organizationAddress = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Address, new HashSet<string>());
            var userAddress = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInUser.HomeAddress, new HashSet<string>());
            var locationAddress = request.QueryParameters.GetValue(Core.Data.Vocabularies.Vocabularies.CluedInLocation.Address, new HashSet<string>());

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

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var url = ConfigurationManager.AppSettings.GetValue<string>("ExternalSearch.libpostal.url", "");
            if (url.IsNullOrEmpty())
            {
                throw new Exception("Bad configuration");
            }

            var client = new RestClient(url);
            //var client = new RestClient("http://localhost:8080/");
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

            var response = client.ExecuteTaskAsync<libpostalResponse>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Content != null)
                {
                    var data = new libpostalResponse();
                    foreach (var item in JsonConvert.DeserializeObject<List<Items>>(response.Content))
                    {
                        data.Items.Add(item);
                    }
                    yield return new ExternalSearchQueryResult<libpostalResponse>(query, data);
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
            if (result is IExternalSearchQueryResult<libpostalResponse> libpostalResult)
            {
                var code = GetOriginEntityCode(libpostalResult, request);
                var clue = new Clue(code, context.Organization);
                clue.Data.EntityData.Codes.Add(request.EntityMetaData.Codes.First());
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
            if (result is IExternalSearchQueryResult<libpostalResponse> libpostalResult)
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
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<libpostalResponse> resultItem, IExternalSearchRequest request)
        {
            var metadata = new EntityMetadataPart();

            PopulateMetadata(metadata, resultItem, request);

            return metadata;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<libpostalResponse> resultItem, IExternalSearchRequest request)
        {
            return new EntityCode(request.EntityMetaData.EntityType, GetCodeOrigin(), resultItem.Id.ToString());
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
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<libpostalResponse> resultItem, IExternalSearchRequest request)
        {
            var code = GetOriginEntityCode(resultItem, request);

            metadata.EntityType = request.EntityMetaData.EntityType;

            //Name is required, without it the changes are ignored and not added to the entity.
            metadata.Name = request.EntityMetaData.Name;
            //metadata.Description = resultItem.Data.description;
            metadata.OriginEntityCode = code;
            foreach (var item in resultItem.Data.Items)
            {
                switch (item.label)
                {
                    case "house":
                        metadata.Properties[libpostalVocabulary.Location.House] = item.value.ToTitleCase();
                        break;
                    case "category":
                        metadata.Properties[libpostalVocabulary.Location.Category] = item.value.ToTitleCase();
                        break;
                    case "near":
                        metadata.Properties[libpostalVocabulary.Location.Near] = item.value.ToTitleCase();
                        break;
                    case "house_number":
                        metadata.Properties[libpostalVocabulary.Location.House_number] = item.value.ToTitleCase();
                        break;
                    case "road":
                        metadata.Properties[libpostalVocabulary.Location.Road] = item.value.ToTitleCase();
                        break;
                    case "unit":
                        metadata.Properties[libpostalVocabulary.Location.Unit] = item.value.ToTitleCase();
                        break;
                    case "level":
                        metadata.Properties[libpostalVocabulary.Location.Level] = item.value.ToTitleCase();
                        break;
                    case "staircase":
                        metadata.Properties[libpostalVocabulary.Location.Staircase] = item.value.ToTitleCase();
                        break;
                    case "entrance":
                        metadata.Properties[libpostalVocabulary.Location.Entrance] = item.value.ToTitleCase();
                        break;
                    case "po_box":
                        metadata.Properties[libpostalVocabulary.Location.Po_box] = item.value.ToTitleCase();
                        break;
                    case "postcode":
                        metadata.Properties[libpostalVocabulary.Location.Postcode] = item.value.ToTitleCase();
                        break;
                    case "suburb":
                        metadata.Properties[libpostalVocabulary.Location.Suburb] = item.value.ToTitleCase();
                        break;
                    case "city_district":
                        metadata.Properties[libpostalVocabulary.Location.City_district] = item.value.ToTitleCase();
                        break;
                    case "city":
                        metadata.Properties[libpostalVocabulary.Location.City] = item.value.ToTitleCase();
                        break;
                    case "island":
                        metadata.Properties[libpostalVocabulary.Location.Island] = item.value.ToTitleCase();
                        break;
                    case "state_district":
                        metadata.Properties[libpostalVocabulary.Location.State_district] = item.value.ToTitleCase();
                        break;
                    case "state":
                        metadata.Properties[libpostalVocabulary.Location.State] = item.value.ToTitleCase();
                        break;
                    case "country_region":
                        metadata.Properties[libpostalVocabulary.Location.Country_region] = item.value.ToTitleCase();
                        break;
                    case "country":
                        metadata.Properties[libpostalVocabulary.Location.Country] = item.value.ToTitleCase();
                        break;
                    case "world_region":
                        metadata.Properties[libpostalVocabulary.Location.World_region] = item.value.ToTitleCase();
                        break;
                }
            }

            metadata.Codes.Add(code);
        }

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider)
        {
            return AcceptedEntityTypes;
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return BuildQueries(context, request);
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
