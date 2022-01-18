using CluedIn.Core.Data.Vocabularies.CluedIn;

namespace CluedIn.ExternalSearch.Providers.libpostal.Vocabularies
{
    public static class libpostalVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="libpostalVocabulary" /> class.
        /// </summary>
        static libpostalVocabulary()
        {
            Person = new CluedInPersonVocabulary();
            Organization = new CluedInOrganizationVocabulary();
            User = new CluedInUserVocabulary();
            Location = new LocationVocabulary();
        }

        public static CluedInPersonVocabulary Person { get; private set; }
        public static CluedInOrganizationVocabulary Organization { get; private set; }
        public static CluedInUserVocabulary User { get; private set; }
        public static LocationVocabulary Location { get; private set; }
    }
}
