using System;
namespace shoppinglist.Models
{
    public class ShoppingItem
    {
		[Newtonsoft.Json.JsonProperty("id")]
		public string Id { get; set; }

		public string Name { get; set; }

        public string Description { get; set; }

        public double Quantity { get; set; }

        public DateTime CompletedOn { get; set; }

        public string CategoryId { get; set; }
    }
}
