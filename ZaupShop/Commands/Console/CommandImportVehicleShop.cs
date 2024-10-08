using Newtonsoft.Json;
using Rocket.API;
using Rocket.Core.Logging;
using System.Collections.Generic;
using System.IO;
using ZaupShop.Helpers;
using ZaupShop.Models;

namespace ZaupShop.Commands.Console
{
    public class CommandImportVehicleShop : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "importvehicleshop";
        public string Help => "";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                Logger.Log("Please provide a file name to import.");
                return;
            }

            string fileName = command[0];

            ThreadHelper.RunAsynchronously(() =>
            {
                string path;
                if (fileName.StartsWith("https://") || fileName.StartsWith("http://"))
                {
                    path = ShopImportExportHelper.DownloadAndSaveFile<VehicleShop>(fileName, pluginInstance.Directory);
                    if (path == null) return;
                    fileName = Path.GetFileName(path);
                }
                else
                {
                    path = Path.Combine(pluginInstance.Directory, fileName);
                }

                if (!File.Exists(path))
                {
                    Logger.Log($"File {path} does not exist.");
                    return;
                }

                Logger.Log($"Loading vehicles from: {fileName}...");

                string json = File.ReadAllText(path);
                List<VehicleShop> vehicleShops = JsonConvert.DeserializeObject<List<VehicleShop>>(json);

                Logger.Log($"Loaded {vehicleShops.Count} vehicles into memory from: {fileName}");

                string tableName = pluginInstance.Configuration.Instance.VehicleShopTableName;
                Logger.Log($"Exporting current {tableName} table contents to file...");
                CommandExportVehicleShop.Export();

                int count = pluginInstance.ShopDB.DeleteVehicleShop();

                Logger.Log($"Deleted {count} vehicles from the {tableName} table in database.");

                ShopImportExportHelper.ImportItems(vehicleShops, tableName, vehicle =>
                {
                    pluginInstance.ShopDB.AddVehicle(vehicle.Id, vehicle.VehicleName, vehicle.BuyPrice, false, false);
                });
            });
        }
    }
}