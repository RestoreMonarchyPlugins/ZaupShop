﻿using Newtonsoft.Json;
using Rocket.API;
using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
                    path = ShopImportExportHelper.DownloadAndSaveFile<ItemShop>(fileName, pluginInstance.Directory);
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

                Logger.Log($"Loading items from: {fileName}...");

                string json = File.ReadAllText(path);
                List<ItemShop> itemShops = JsonConvert.DeserializeObject<List<ItemShop>>(json);

                Logger.Log($"Loaded {itemShops.Count} items into memory from: {fileName}");

                string tableName = pluginInstance.Configuration.Instance.ItemShopTableName;
                Logger.Log($"Exporting current {tableName} table contents to file...");
                CommandExportItemShop.Export();

                int count = pluginInstance.ShopDB.DeleteItemShop();

                Logger.Log($"Deleted {count} items from the {tableName} table in database.");

                ShopImportExportHelper.ImportItems(itemShops, tableName, item =>
                {
                    pluginInstance.ShopDB.AddItem(item.Id, item.ItemName, item.BuyPrice, false, item.SellPrice, false);
                });
            });
        }
    }
}