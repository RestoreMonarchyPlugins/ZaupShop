using Newtonsoft.Json;
using Rocket.API;
using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
        public List<string> Aliases => [];
        public List<string> Permissions => [];

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Export();
        }

        public static string Export(bool shouldLog = true)
        {
            string tableName = pluginInstance.Configuration.Instance.ItemShopTableName;
            string fileName = $"{tableName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";

            IEnumerable<ItemShop> itemShops = pluginInstance.ShopDB.GetAllItemShop();
            string json = JsonConvert.SerializeObject(itemShops, Formatting.Indented);
            string path = Path.Combine(pluginInstance.Directory, fileName);

            File.WriteAllText(path, json);
            Logger.Log($"Exported {itemShops.Count()} items to: {fileName}");

            return path;
        }
    }
}
