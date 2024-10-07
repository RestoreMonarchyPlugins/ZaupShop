using Rocket.API;
using Rocket.Core.Logging;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using ZaupShop.Helpers;

namespace ZaupShop.Commands
{
    public class CommandUploadVehicleShop : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "uploadvehicleshop";
        public string Help => "";
        public string Syntax => "";
        public List<string> Aliases => [];
        public List<string> Permissions => [];

        public void Execute(IRocketPlayer caller, string[] command)
        {
            IEnumerable<Asset> vehicles = UnturnedHelper.GetAllVehicles();
            int count = vehicles.Count();

            Logger.Log($"Detected {count} vehicle assets on the server...");
            Logger.Log($"Uploading {vehicles.Count()} vehicles to the {ZaupShop.Instance.Configuration.Instance.VehicleShopTableName} table in database now...");

            ThreadHelper.RunAsynchronously(() =>
            {
                foreach (Asset asset in vehicles)
                {
                    string vehicleName = asset.FriendlyName.Length > 32 ? asset.FriendlyName.Substring(0, 32) : asset.FriendlyName;
                    pluginInstance.ShopDB.AddVehicle(asset.id, vehicleName, 0, false, true);
                }
                Logger.Log($"Done! Finished syncing {count} vehicles with the {ZaupShop.Instance.Configuration.Instance.VehicleShopTableName} table in database!");
            });
        }
    }
}
