using Newtonsoft.Json;

namespace ZaupShop.Models
{
    public class ItemShop
    {
        [JsonProperty("ID")]
        public ushort Id { get; set; }
        [JsonProperty("ItemName")]
        public string ItemName { get; set; }
        [JsonProperty("BuyPrice")]
        public decimal BuyPrice { get; set; }
        [JsonProperty("SellPrice")]
        public decimal SellPrice { get; set; }
    }
}
