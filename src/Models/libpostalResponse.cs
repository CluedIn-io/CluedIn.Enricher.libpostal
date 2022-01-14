using System.Collections.Generic;
using CluedIn.Core.Data;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.libpostal.Models
{
	public class libpostalResponse
	{
		public libpostalResponse() 
		{
			this.Items = new List<Items>();
		}
		public List<Items> Items { get; set; }
	}

	public class Items
	{
		public string label { get; set; }
		public string value { get; set; }
	}
}