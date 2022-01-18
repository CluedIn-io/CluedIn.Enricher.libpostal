namespace CluedIn.ExternalSearch.Providers.Libpostal.Vocabularies
{
    public static class LibpostalVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="LibpostalVocabulary" /> class.
        /// </summary>
        static LibpostalVocabulary()
        {
            Location = new LocationVocabulary();
        }

        public static LocationVocabulary Location { get; private set; }
    }
}
