using Rocket.API;
using System.Collections.Generic;
using System.Linq;
using ZaupShop.Helpers;
using ZaupShop.Models;

namespace ZaupShop.Commands.Console
{
    public class CommandExportVehicleShop : IRocketCommand
    {
        private static ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "exportvehicleshop";
        public string Help => "";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ThreadHelper.RunAsynchronously(() =>
            {
                Export(command.ElementAtOrDefault(0));
            });            
        }

        public static string Export(string fileName = null)
        {
            string tableName = pluginInstance.Configuration.Instance.VehicleShopTableName;
            IEnumerable<VehicleShop> vehicleShops = pluginInstance.ShopDB.GetAllVehicleShop();

            return ShopImportExportHelper.ExportItems(vehicleShops, tableName, pluginInstance.Directory, fileName);
        }
    }
}