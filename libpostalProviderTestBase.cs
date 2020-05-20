using CluedIn.Core;
using CluedIn.ExternalSearch;
using Moq;

namespace CluedIn.Enricher.libpostal.Unit.Test._Name_Provider
{
    public abstract class libpostalProviderTestBase
    {
        protected readonly CluedIn.Enricher.libpostal.libpostalProvider _sut;

        protected readonly Mock<IExternalSearchRequest> _externalSearchCommand;
        protected readonly ExecutionContext _executionContext = null;

        protected _Name_ProviderTestBase()
        {
            _sut = new CluedIn.Enricher._Name_._Name_Provider();

            _externalSearchCommand = new Mock<IExternalSearchRequest>();
        }
    }
}