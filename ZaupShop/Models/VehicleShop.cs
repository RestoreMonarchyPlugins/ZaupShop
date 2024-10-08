using Newtonsoft.Json;

namespace ZaupShop.Models
{
    public class VehicleShop
    {
        [JsonProperty("ID")]
        public ushort Id { get; set; }
        [JsonProperty("VehicleName")]
        public string VehicleName { get; set; }
        [JsonProperty("BuyPrice")]
        public decimal BuyPrice { get; set; }
        [JsonProperty("SellPrice")]
        public decimal SellPrice { get; set; }
    }
}
