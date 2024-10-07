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
    public class CommandBuy : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "buy";
        public string Help => "Allows you to buy items from the shop.";
        public string Syntax => "[v.]<name or id> [amount] [25 | 50 | 75 | 100]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;

            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("buy_command_usage"));
                return;
            }

            byte amountToBuy = 1;
            if (command.Length > 1 && !byte.TryParse(command[1], out amountToBuy))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_amt"));
                return;
            }

            var components = Parser.getComponentsFromSerial(command[0], '.');
            if ((components.Length == 2 && components[0].Trim() != "v") ||
                (components.Length == 1 && components[0].Trim() == "v") ||
                components.Length > 2 || command[0].Trim() == string.Empty)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("buy_command_usage"));
                return;
            }

            bool isVehicle = components[0] == "v";
            if (isVehicle && !pluginInstance.Configuration.Instance.CanBuyVehicles)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("buy_vehicles_off"));
                return;
            }
            else if (!isVehicle && !pluginInstance.Configuration.Instance.CanBuyItems)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("buy_items_off"));
                return;
            }

            ushort id;
            string name;
            decimal cost;

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(components[1], out id, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("could_not_find", components[1]));
                    return;
                }
                cost = pluginInstance.ShopDB.GetVehicleCost(id);
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(components[0], out id, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("could_not_find", components[0]));
                    return;
                }
                cost = decimal.Round(pluginInstance.ShopDB.GetItemCost(id) * amountToBuy, 2);
            }

            if (cost <= 0m)
            {
                if (isVehicle)
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("vehicle_not_available", name));
                }
                else
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("item_not_available", name));
                }
                return;
            }

            decimal balance = Uconomy.Instance.Database.GetBalance(player.CSteamID.ToString());
            if (balance < cost)
            {
                string amountString;
                if (isVehicle)
                {
                    amountString = "1";
                }
                else
                {
                    amountString = amountToBuy.ToString();
                }
                UnturnedChat.Say(caller, pluginInstance.Translate("not_enough_currency_msg",
                    Uconomy.Instance.Configuration.Instance.MoneyName, amountString, name));
                return;
            }

            bool success;
            if (isVehicle)
            {
                success = player.GiveVehicle(id);
            }
            else
            {
                success = player.GiveItem(id, amountToBuy);
            }

            if (!success)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("error_giving_item", name));
                return;
            }

            decimal newBalance = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), -cost);

            if (isVehicle)
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("vehicle_buy_msg",
                    name, cost, Uconomy.Instance.Configuration.Instance.MoneyName, newBalance,
                    Uconomy.Instance.Configuration.Instance.MoneyName, amountToBuy));
            }
            else
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("item_buy_msg",
                    name, cost, Uconomy.Instance.Configuration.Instance.MoneyName, newBalance,
                    Uconomy.Instance.Configuration.Instance.MoneyName, amountToBuy));
            }

            string itemType;
            if (isVehicle)
            {
                itemType = "vehicle";
            }
            else
            {
                itemType = "item";
            }
            pluginInstance.TriggerOnShopBuy(player, cost, amountToBuy, id, itemType);
            player.Player.gameObject.SendMessage("ZaupShopOnBuy",
                new object[] { caller, cost, amountToBuy, id, itemType },
                SendMessageOptions.DontRequireReceiver);
        }
    }
}