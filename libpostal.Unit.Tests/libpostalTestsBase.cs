using System;
using CluedIn.ExternalSearch.Providers.libpostal;
using CluedIn.ExternalSearch.Providers.libpostal.Vocabularies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CluedIn.Testing.Base.Context;
using CluedIn.Testing.Base.ExternalSearch;
using CluedIn.Testing.Base.Processing.Actors;
using Moq;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.Core.Processing;
using CluedIn.Core.Serialization;
using CluedIn.Core.Workflows;
using CluedIn.ExternalSearch;
using CluedIn.Core;

namespace libpostal.Unit.Tests
{
	[TestClass]
	public abstract class libpostalTestsBase
	{
		protected readonly CluedIn.ExternalSearch.Providers.libpostal.libpostalExternalSearchProvider _sut;

		protected readonly Mock<IExternalSearchRequest> _externalSearchCommand;
		protected readonly ExecutionContext _executionContext = null;

		protected libpostalTestsBase()
		{
			_sut = new CluedIn.ExternalSearch.Providers.libpostal.libpostalExternalSearchProvider();

			_externalSearchCommand = new Mock<IExternalSearchRequest>();
		}
	}
}
