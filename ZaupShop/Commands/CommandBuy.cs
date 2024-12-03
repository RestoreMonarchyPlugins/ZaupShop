using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
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
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;

            if (command.Length == 0 || command.Length > 2)
            {
                pluginInstance.SendMessageToPlayer(player, "buy_command_usage");
                return;
            }

            byte amountToBuy = 1;
            if (command.Length == 2 && !byte.TryParse(command[1], out amountToBuy))
            {
                pluginInstance.SendMessageToPlayer(player, "invalid_amt");
                return;
            }

            bool isVehicle = false;
            string itemName;

            if (command[0].StartsWith("v.", StringComparison.OrdinalIgnoreCase))
            {
                isVehicle = true;
                itemName = command[0].Substring(2);
            }
            else if (command[0].Equals("v", StringComparison.OrdinalIgnoreCase) && command.Length == 2)
            {
                isVehicle = true;
                itemName = command[1];
                amountToBuy = 1; // Reset amount for vehicles
            }
            else
            {
                itemName = command[0];
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                pluginInstance.SendMessageToPlayer(player, "buy_command_usage");
                return;
            }

            if (isVehicle && !pluginInstance.Configuration.Instance.CanBuyVehicles)
            {
                pluginInstance.SendMessageToPlayer(player, "buy_vehicles_off");
                return;
            }
            else if (!isVehicle && !pluginInstance.Configuration.Instance.CanBuyItems)
            {
                pluginInstance.SendMessageToPlayer(player, "buy_items_off");
                return;
            }

            ushort id;
            string name;

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(itemName, out id, out name))
                {
                    pluginInstance.SendMessageToPlayer(player, "could_not_find", itemName);
                    return;
                }
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(itemName, out id, out name))
                {
                    pluginInstance.SendMessageToPlayer(player, "could_not_find", itemName);
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
                        if (isVehicle)
                        {
                            pluginInstance.SendMessageToPlayer(player, "vehicle_not_available", name);
                        }                            
                        else
                        {
                            pluginInstance.SendMessageToPlayer(player, "item_not_available", name);
                        }

                        return;
                    }

                    if (balance < cost)
                    {
                        string amountString = isVehicle ? "1" : amountToBuy.ToString();
                        string costString = cost.ToString("N");
                        string moneyName = Uconomy.Instance.Configuration.Instance.MoneyName;

                        pluginInstance.SendMessageToPlayer(player, "not_enough_currency_msg", costString, moneyName, amountString, name);
                        return;
                    }

                    bool success = isVehicle ? player.GiveVehicle(id) : player.GiveItem(id, amountToBuy);

                    if (!success)
                    {
                        pluginInstance.SendMessageToPlayer(player, "error_giving_item", name);
                        return;
                    }

                    ThreadHelper.RunAsynchronously(() =>
                    {
                        decimal newBalance = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), -cost);

                        ThreadHelper.RunSynchronously(() =>
                        {
                            string messageKey = isVehicle ? "vehicle_buy_msg" : "item_buy_msg";
                            string moneyName = Uconomy.Instance.Configuration.Instance.MoneyName;
                            pluginInstance.SendMessageToPlayer(player, messageKey, name, cost, moneyName, newBalance.ToString("N"), moneyName, amountToBuy);

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