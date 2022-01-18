using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.Libpostal.Models
{
    public class LibpostalResponse
    {
        public LibpostalResponse()
        {
            Items = new List<Items>();
        }
        public List<Items> Items { get; set; }
    }

    public class Items
    {
        public string label { get; set; }
        public string value { get; set; }
    }
}
