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
            Location = new PersonVocabulary();
            Organization = new OrganizationVocabulary();
            User = new UserVocabulary();

        }

        public static PersonVocabulary Location { get; private set; }
        public static OrganizationVocabulary Organization { get; private set; }
        public static UserVocabulary User { get; private set; }
    }
}