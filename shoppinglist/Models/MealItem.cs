using System;
namespace shoppinglist.Models
{
    public class MealItem
    {
		[Newtonsoft.Json.JsonProperty("id")]
		public string Id { get; set; }

		public string Name { get; set; }

        public MealType Type { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
