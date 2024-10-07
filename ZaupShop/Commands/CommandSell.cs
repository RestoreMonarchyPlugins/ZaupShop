using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;
using ZaupShop.Helpers;

namespace ZaupShop.Commands
{
    public class CommandSell : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sell";
        public string Help => "Allows you to sell items to the shop from your inventory.";
        public string Syntax => "<name or id> [amount]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 0 || (command.Length > 0 && string.IsNullOrWhiteSpace(command[0])))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("sell_command_usage"));
                return;
            }

            byte amount = 1;
            if (command.Length > 1 && !byte.TryParse(command[1], out amount))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_amt"));
                return;
            }

            if (!pluginInstance.Configuration.Instance.CanSellItems)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("sell_items_off"));
                return;
            }

            ItemAsset asset = UnturnedHelper.GetItemByIdOrName(command[0]);
            if (asset == null)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("could_not_find", command[0]));
                return;
            }

            List<InventorySearch> items = player.Inventory.search(asset.id, true, true);
            if (items.Count == 0)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("not_have_item_sell", asset.itemName));
                return;
            }

            if (items.Count < amount)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("not_enough_items_sell", amount.ToString(), asset.itemName));
                return;
            }

            ThreadHelper.RunAsynchronously(() =>
            {
                decimal price = pluginInstance.ShopDB.GetItemBuyPrice(asset.id);

                ThreadHelper.RunSynchronously(() =>
                {
                    if (price <= 0.00m)
                    {
                        UnturnedChat.Say(caller, pluginInstance.Translate("no_sell_price_set", asset.itemName));
                        return;
                    }

                    decimal addMoney = 0;
                    for (int i = 0; i < amount; i++)
                    {
                        if (player.Player.equipment.checkSelection(items[i].page, items[i].jar.x, items[i].jar.y))
                        {
                            player.Player.equipment.dequip();
                        }
                        byte quality = pluginInstance.Configuration.Instance.QualityCounts ? items[i].jar.item.durability : (byte)100;
                        decimal perItemPrice = decimal.Round(price * (quality / 100.0m), 2);
                        addMoney += perItemPrice;
                        player.Inventory.removeItem(items[i].page, player.Inventory.getIndex(items[i].page, items[i].jar.x, items[i].jar.y));
                    }

                    ThreadHelper.RunAsynchronously(() =>
                    {
                        decimal balance = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), addMoney);

                        ThreadHelper.RunSynchronously(() =>
                        {
                            UnturnedChat.Say(player, pluginInstance.Translate("sold_items", amount, asset.itemName, addMoney, Uconomy.Instance.Configuration.Instance.MoneyName, balance, Uconomy.Instance.Configuration.Instance.MoneyName));
                            pluginInstance.TriggerOnShopSell(player, addMoney, amount, asset.id);
                            player.Player.gameObject.SendMessage("ZaupShopOnSell", new object[] { player, addMoney, amount, asset.id }, SendMessageOptions.DontRequireReceiver);
                        });
                    });
                });
            });
        }
    }
}