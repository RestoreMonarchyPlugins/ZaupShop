using Rocket.API;
using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ZaupShop.Helpers;
using ZaupShop.Models;

namespace ZaupShop.Commands.Console
{
    public class CommandImportItemShop : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "importitemshop";
        public string Help => "";
        public string Syntax => "";
        public List<string> Aliases => [];
        public List<string> Permissions => [];

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
                string path = Path.Combine(pluginInstance.Directory, fileName);

                if (!File.Exists(path))
                {
                    Logger.Log($"File {path} does not exist.");
                    return;
                }

                Logger.Log($"Loading items from: {fileName}...");

                string json = File.ReadAllText(path);
                List<ItemShop> itemShops = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemShop>>(json);

                Logger.Log($"Loaded {itemShops.Count} items into memory from: {fileName}");

                fileName = CommandExportItemShop.Export(false);

                Logger.Log($"Exported current item shop to: {fileName}");

                int count = pluginInstance.ShopDB.DeleteItemShop();

                Logger.Log($"Deleted {count} items from the {pluginInstance.Configuration.Instance.ItemShopTableName} table in database.");

                Logger.Log($"Uploading {itemShops.Count} items to the {pluginInstance.Configuration.Instance.ItemShopTableName} table in database now...");
                foreach (ItemShop itemShop in itemShops)
                {
                    pluginInstance.ShopDB.AddItem(itemShop.Id, itemShop.ItemName, itemShop.BuyPrice, false, itemShop.SellPrice, false);
                }

                Logger.Log($"Done! Finished uploading {itemShops.Count} items to the {pluginInstance.Configuration.Instance.ItemShopTableName} table in database!");
            });
        }
    }
}
