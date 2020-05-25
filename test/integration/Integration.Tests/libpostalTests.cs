using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.ExternalSearch;
using CluedIn.ExternalSearch.Providers.libpostal;
using CluedIn.ExternalSearch.Providers.libpostal.Models;
using CluedIn.Testing.Base.ExternalSearch;
using Moq;
using Xunit;
using Xunit.Abstractions;
//using TestContext = CluedIn.Tests.Unit.TestContext;

namespace CluedIn.Tests.Integration.ExternalSearch
{
    public class libpostalTests : BaseExternalSearchTest<libpostalExternalSearchProvider>
    {
        private readonly ITestOutputHelper outputHelper;

        public libpostalTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }


        [Theory]

        [InlineData(null)]
        public void TestNoClueProduction(string name)
        {
            IEntityMetadata entityMetadata = new EntityMetadataPart()
            {
                Name = name,
                EntityType = EntityType.Organization
            };

            this.Setup(null, entityMetadata);

            // Assert
            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
            Assert.True(clues.Count == 0);
        }

    }
}