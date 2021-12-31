using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.libpostal.Vocabularies
{
    public static class libpostalVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="KnowledgeGraphVocabulary" /> class.
        /// </summary>
        static libpostalVocabulary()
        {
            Person = new PersonVocabulary();
            Organization = new OrganizationVocabulary();
            User = new UserVocabulary();
            Location = new LocationVocabulary();
        }

        public static PersonVocabulary Person { get; private set; }
        public static OrganizationVocabulary Organization { get; private set; }
        public static UserVocabulary User { get; private set; }

        public static LocationVocabulary Location { get; private set; }
    }
}
