using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Libpostal.Vocabularies
{
    public class LocationVocabulary : SimpleVocabulary
    {
        public LocationVocabulary()
        {
            VocabularyName = "libpostal Location";
            KeyPrefix = "libpostal.location";
            KeySeparator = ".";
            Grouping = EntityType.Location;

            AddGroup("Location Details", group =>
            {

                House = group.Add(new VocabularyKey("House", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Category = group.Add(new VocabularyKey("Category", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Near = group.Add(new VocabularyKey("Near", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                House_number = group.Add(new VocabularyKey("House_number", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Road = group.Add(new VocabularyKey("Road", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Unit = group.Add(new VocabularyKey("Unit", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Level = group.Add(new VocabularyKey("Level", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Staircase = group.Add(new VocabularyKey("Staircase", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Entrance = group.Add(new VocabularyKey("Entrance", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Po_box = group.Add(new VocabularyKey("Po_box", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Postcode = group.Add(new VocabularyKey("Postcode", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Suburb = group.Add(new VocabularyKey("Suburb", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                City_district = group.Add(new VocabularyKey("City_district", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                City = group.Add(new VocabularyKey("City", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Island = group.Add(new VocabularyKey("Island", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                State_district = group.Add(new VocabularyKey("State_district", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                State = group.Add(new VocabularyKey("State", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Country_region = group.Add(new VocabularyKey("Country_region", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                Country = group.Add(new VocabularyKey("Country", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                World_region = group.Add(new VocabularyKey("World_region", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
            });

            AddMapping(Country, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressCountryCode);
            AddMapping(City, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressCity);
            AddMapping(State, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressState);
            AddMapping(Road, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressNameStreet);
            AddMapping(Po_box, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressPostOfficeBox);
            AddMapping(Level, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressFloorCode);
            AddMapping(House_number, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressStreetNumber);
            AddMapping(Postcode, Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressZipCode);

        }


        public VocabularyKey House { get; set; }
        public VocabularyKey Category { get; set; }
        public VocabularyKey Near { get; set; }
        public VocabularyKey House_number { get; set; }
        public VocabularyKey Road { get; set; }
        public VocabularyKey Unit { get; set; }
        public VocabularyKey Level { get; set; }
        public VocabularyKey Staircase { get; set; }
        public VocabularyKey Entrance { get; set; }
        public VocabularyKey Po_box { get; set; }
        public VocabularyKey Postcode { get; set; }
        public VocabularyKey Suburb { get; set; }
        public VocabularyKey City_district { get; set; }
        public VocabularyKey City { get; set; }
        public VocabularyKey Island { get; set; }
        public VocabularyKey State_district { get; set; }
        public VocabularyKey State { get; set; }
        public VocabularyKey Country_region { get; set; }
        public VocabularyKey Country { get; set; }
        public VocabularyKey World_region { get; set; }

    }
}
