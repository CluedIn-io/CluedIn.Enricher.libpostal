using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.libpostal.Vocabularies
{
    public class LocationVocabulary : SimpleVocabulary
    {
        public LocationVocabulary()
        {
            this.VocabularyName = "libpostal Location";
            this.KeyPrefix = "libpostal.locaation";
            this.KeySeparator = ".";
            this.Grouping = EntityType.Location;

            this.AddGroup("Location Details", group =>
            {

                this.House = group.Add(new VocabularyKey("House", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Category = group.Add(new VocabularyKey("Category", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Near = group.Add(new VocabularyKey("Near", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.House_number = group.Add(new VocabularyKey("House_number", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Road = group.Add(new VocabularyKey("Road", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Unit = group.Add(new VocabularyKey("Unit", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Level = group.Add(new VocabularyKey("Level", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Staircase = group.Add(new VocabularyKey("Staircase", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Entrance = group.Add(new VocabularyKey("Entrance", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Po_box = group.Add(new VocabularyKey("Po_box", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Postcode = group.Add(new VocabularyKey("Postcode", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Suburb = group.Add(new VocabularyKey("Suburb", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.City_district = group.Add(new VocabularyKey("City_district", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.City = group.Add(new VocabularyKey("City", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Island = group.Add(new VocabularyKey("Island", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.State_district = group.Add(new VocabularyKey("State_district", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.State = group.Add(new VocabularyKey("State", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Country_region = group.Add(new VocabularyKey("Country_region", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.Country = group.Add(new VocabularyKey("Country", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
                this.World_region = group.Add(new VocabularyKey("World_region", VocabularyKeyDataType.Text, VocabularyKeyVisibility.Visible));
            });

            this.AddMapping(this.Country, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressCountryCode);
            this.AddMapping(this.City, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressCity);
            this.AddMapping(this.State, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressState);
            this.AddMapping(this.Road, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressNameStreet);
            this.AddMapping(this.Po_box, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressPostOfficeBox);
            this.AddMapping(this.Level, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressFloorCode);
            this.AddMapping(this.House_number, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressStreetNumber);
            this.AddMapping(this.Postcode, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInLocation.AddressZipCode);

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