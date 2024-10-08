using Rocket.API;
using Rocket.Core.Logging;
using System.Collections.Generic;
using System.Linq;
using ZaupShop.Helpers;
using ZaupShop.Models;

namespace ZaupShop.Commands.Console
{
    public class CommandExportItemShop : IRocketCommand
    {
        private static ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "exportitemshop";
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
            string tableName = pluginInstance.Configuration.Instance.ItemShopTableName;
            IEnumerable<ItemShop> itemShops = pluginInstance.ShopDB.GetAllItemShop();

            return ShopImportExportHelper.ExportItems(itemShops, tableName, pluginInstance.Directory, fileName);
        }
    }
}