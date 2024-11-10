using System;
using Rocket.API;

namespace ZaupShop
{
    public class ZaupShopConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor = "green";
        public string MessageIconUrl;
        public string ItemShopTableName;
        public string VehicleShopTableName;
        public bool CanBuyItems;
        public bool CanBuyVehicles;
        public bool CanSellItems;
        public bool QualityCounts;

        public void LoadDefaults()
        {
            MessageColor = "yellow";
            MessageIconUrl = "https://i.imgur.com/aMxb6pb.png";
            ItemShopTableName = "uconomyitemshop";
            VehicleShopTableName = "uconomyvehicleshop";
            CanBuyItems = true;
            CanBuyVehicles = true;
            CanSellItems = true;
            QualityCounts = false;
        }
    }
}
