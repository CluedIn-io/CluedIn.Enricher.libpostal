using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CluedIn.Core.Data.Parts;
using javax.sound.midi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.ExternalSearch;
using Moq;

namespace libpostal.Unit.Tests
{
	[TestClass]
	public class libpostalTests : libpostalTestsBase
	{
		[TestMethod]
		public void CanBeCreated()
		{
			Assert.IsNotNull(_sut);
		}

		[TestMethod]
		public void ProviderIdHasBeenSet()
		{
			Assert.AreNotEqual(
				Guid.Empty,
				CluedIn.ExternalSearch.Providers.libpostal.libpostalExternalSearchProvider.ProviderId);
		}

		//[TestMethod]
		//public void BuildQueries()
		//{
		//	Assert.ThrowsException<ArgumentNullException>(() => _sut.BuildQueries(_executionContext, null));
		//}

		[TestMethod]
		public void BuildQueries2()
		{
			IEntityMetadata entitymetadata = new EntityMetadataPart()
			{
				EntityType = EntityType.Location.Address,
			};

			Mock<IExternalSearchRequest> externalSearchQuery = new Mock<IExternalSearchRequest>();
			externalSearchQuery.SetupAllProperties();
			//externalSearchQuery.Object.EntityMetaData.
			externalSearchQuery.Object.EntityMetaData.EntityType = EntityType.Location.Address;

			//var queries = _sut.BuildQueries(_executionContext, _externalSearchCommand.Object);
			//foreach (var query in queries)
			//{
			//	Console.WriteLine(query.QueryParameters);
			//}
			Assert.ThrowsException<ArgumentNullException>(()=> _sut.BuildQueries(_executionContext, externalSearchQuery.Object));
		}
	}
}
