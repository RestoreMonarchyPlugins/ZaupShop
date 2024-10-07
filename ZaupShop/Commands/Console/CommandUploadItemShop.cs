using Rocket.API;
using Rocket.Core.Logging;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using ZaupShop.Helpers;

namespace ZaupShop.Commands.Console
{
    public class CommandUploadItemShop : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "uploaditemshop";
        public string Help => "";
        public string Syntax => "";
        public List<string> Aliases => [];
        public List<string> Permissions => [];

        public void Execute(IRocketPlayer caller, string[] command)
        {
            IEnumerable<ItemAsset> items = UnturnedHelper.GetAllItems();
            int count = items.Count();

            Logger.Log($"Detected {count} item assets on the server...");
            Logger.Log($"Uploading {items.Count()} items to the {ZaupShop.Instance.Configuration.Instance.ItemShopTableName} table in database now...");

            ThreadHelper.RunAsynchronously(() =>
            {
                foreach (ItemAsset asset in items)
                {
                    // get name of item max length 32 characters
                    string itemName = asset.itemName.Length > 32 ? asset.itemName.Substring(0, 32) : asset.itemName;
                    pluginInstance.ShopDB.AddItem(asset.id, itemName, 0, false, 0, true);
                }
                Logger.Log($"Done! Finished syncing {count} items with the {ZaupShop.Instance.Configuration.Instance.ItemShopTableName} table in database!");
            });
        }
    }
}
