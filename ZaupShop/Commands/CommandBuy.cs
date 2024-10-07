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
        public string Syntax => "[v.]<name or id> [amount]";
        public List<string> Aliases => [ "bal" ];
        public List<string> Permissions => [];

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
            string itemToFind = isVehicle ? components[1] : components[0];

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(itemToFind, out id, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("could_not_find", itemToFind));
                    return;
                }
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(itemToFind, out id, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("could_not_find", itemToFind));
                    return;
                }
            }

            ThreadHelper.RunAsynchronously(() =>
            {
                decimal cost = isVehicle
                    ? pluginInstance.ShopDB.GetVehicleCost(id)
                    : decimal.Round(pluginInstance.ShopDB.GetItemCost(id) * amountToBuy, 2);

                decimal balance = Uconomy.Instance.Database.GetBalance(player.CSteamID.ToString());

                ThreadHelper.RunSynchronously(() =>
                {
                    if (cost <= 0m)
                    {
                        UnturnedChat.Say(caller, pluginInstance.Translate(isVehicle ? "vehicle_not_available" : "item_not_available", name));
                        return;
                    }

                    if (balance < cost)
                    {
                        string amountString = isVehicle ? "1" : amountToBuy.ToString();
                        UnturnedChat.Say(caller, pluginInstance.Translate("not_enough_currency_msg",
                            Uconomy.Instance.Configuration.Instance.MoneyName, amountString, name));
                        return;
                    }

                    bool success = isVehicle ? player.GiveVehicle(id) : player.GiveItem(id, amountToBuy);

                    if (!success)
                    {
                        UnturnedChat.Say(caller, pluginInstance.Translate("error_giving_item", name));
                        return;
                    }

                    ThreadHelper.RunAsynchronously(() =>
                    {
                        decimal newBalance = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), -cost);

                        ThreadHelper.RunSynchronously(() =>
                        {
                            string messageKey = isVehicle ? "vehicle_buy_msg" : "item_buy_msg";
                            UnturnedChat.Say(caller, pluginInstance.Translate(messageKey,
                                name, cost, Uconomy.Instance.Configuration.Instance.MoneyName, newBalance,
                                Uconomy.Instance.Configuration.Instance.MoneyName, amountToBuy));

                            string itemType = isVehicle ? "vehicle" : "item";
                            pluginInstance.TriggerOnShopBuy(player, cost, amountToBuy, id, itemType);
                            player.Player.gameObject.SendMessage("ZaupShopOnBuy",
                                new object[] { caller, cost, amountToBuy, id, itemType },
                                SendMessageOptions.DontRequireReceiver);
                        });
                    });
                });
            });
        }
    }
}