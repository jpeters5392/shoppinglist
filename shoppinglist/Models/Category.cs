using System;
namespace shoppinglist.Models
{
    public class Category
    {
		[Newtonsoft.Json.JsonProperty("id")]
		public string Id { get; set; }

        public string Name { get; set; }
    }
}
