using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Configuration;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.libpostal.Models;
using CluedIn.ExternalSearch.Providers.libpostal.Vocabularies;
using Newtonsoft.Json;
using RestSharp;
using Castle.Core.Internal;
//using RestSharp.Contrib;

namespace CluedIn.ExternalSearch.Providers.libpostal
{
	public class queryBody
	{
		public string query;
	}
	/// <summary>The libpostal graph external search provider.</summary>
	/// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
	public class libpostalExternalSearchProvider : ExternalSearchProviderBase
	{
		public static readonly Guid ProviderId = Guid.Parse("aba4e4cf-3c48-4828-9fdf-990b22e1a29c");

		/**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

		public libpostalExternalSearchProvider()
			: base(ProviderId, entityTypes: new EntityType[] { EntityType.Person, EntityType.Organization, EntityType.Infrastructure.User })
		{
		}


		/**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/
		public override bool Accepts(EntityType entityType)
		{
			return true;
		}
		/// <summary>Builds the queries.</summary>
		/// <param name="context">The context.</param>
		/// <param name="request">The request.</param>
		/// <returns>The search queries.</returns>
		public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
		{
			if (!this.Accepts(request.EntityMetaData.EntityType))
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
		}

		/// <summary>Executes the search.</summary>
		/// <param name="context">The context.</param>
		/// <param name="query">The query.</param>
		/// <returns>The results.</returns>
		public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
		{
			var url = ConfigurationManager.AppSettings.GetValue<string>("ExternalSearch.libpostal.url", "");
			if (url.IsNullOrEmpty()) { throw new Exception("Bad configuration"); }
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

			var response = client.ExecuteTaskAsync(request).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Content != null)
					foreach (var item in JsonConvert.DeserializeObject<List<libpostalResponse>>(response.Content))
					{
						yield return new ExternalSearchQueryResult<libpostalResponse>(query, item);
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
				var code = this.GetOriginEntityCode(libpostalResult, request);
				var clue = new Clue(code, context.Organization);
				clue.Data.EntityData.Codes.Add(request.EntityMetaData.Codes.First());
				this.PopulateMetadata(clue.Data.EntityData, libpostalResult, request);
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
				return this.CreateMetadata(libpostalResult, request);
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

			this.PopulateMetadata(metadata, resultItem, request);

			return metadata;
		}

		/// <summary>Gets the origin entity code.</summary>
		/// <param name="resultItem">The result item.</param>
		/// <returns>The origin entity code.</returns>
		private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<libpostalResponse> resultItem, IExternalSearchRequest request)
		{
			return new EntityCode(request.EntityMetaData.EntityType, this.GetCodeOrigin(), resultItem.Id.ToString());
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
			var code = this.GetOriginEntityCode(resultItem, request);

			metadata.EntityType = request.EntityMetaData.EntityType;

			//Name is required, without it the changes are ignored and not added to the entity.
			metadata.Name = request.EntityMetaData.Name;
			//metadata.Description = resultItem.Data.description;
			metadata.OriginEntityCode = code;

			switch (resultItem.Data.label)
			{
				case "house":
					metadata.Properties[libpostalVocabulary.Location.House] = resultItem.Data.value;
					break;
				case "category":
					metadata.Properties[libpostalVocabulary.Location.Category] = resultItem.Data.value;
					break;
				case "near":
					metadata.Properties[libpostalVocabulary.Location.Near] = resultItem.Data.value;
					break;
				case "house_number":
					metadata.Properties[libpostalVocabulary.Location.House_number] = resultItem.Data.value;
					break;
				case "road":
					metadata.Properties[libpostalVocabulary.Location.Road] = resultItem.Data.value;
					break;
				case "unit":
					metadata.Properties[libpostalVocabulary.Location.Unit] = resultItem.Data.value;
					break;
				case "level":
					metadata.Properties[libpostalVocabulary.Location.Level] = resultItem.Data.value;
					break;
				case "staircase":
					metadata.Properties[libpostalVocabulary.Location.Staircase] = resultItem.Data.value;
					break;
				case "entrance":
					metadata.Properties[libpostalVocabulary.Location.Entrance] = resultItem.Data.value;
					break;
				case "po_box":
					metadata.Properties[libpostalVocabulary.Location.Po_box] = resultItem.Data.value;
					break;
				case "postcode":
					metadata.Properties[libpostalVocabulary.Location.Postcode] = resultItem.Data.value;
					break;
				case "suburb":
					metadata.Properties[libpostalVocabulary.Location.Suburb] = resultItem.Data.value;
					break;
				case "city_district":
					metadata.Properties[libpostalVocabulary.Location.City_district] = resultItem.Data.value;
					break;
				case "city":
					metadata.Properties[libpostalVocabulary.Location.City] = resultItem.Data.value;
					break;
				case "island":
					metadata.Properties[libpostalVocabulary.Location.Island] = resultItem.Data.value;
					break;
				case "state_district":
					metadata.Properties[libpostalVocabulary.Location.State_district] = resultItem.Data.value;
					break;
				case "state":
					metadata.Properties[libpostalVocabulary.Location.State] = resultItem.Data.value;
					break;
				case "country_region":
					metadata.Properties[libpostalVocabulary.Location.Country_region] = resultItem.Data.value;
					break;
				case "country":
					metadata.Properties[libpostalVocabulary.Location.Country] = resultItem.Data.value;
					break;
				case "world_region":
					metadata.Properties[libpostalVocabulary.Location.World_region] = resultItem.Data.value;
					break;
			}

			metadata.Codes.Add(code);
		}
	}
}