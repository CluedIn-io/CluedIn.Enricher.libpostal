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
            Location = new LocationVocabulary();
        }

        public static LocationVocabulary Location { get; private set; }
    }
}
