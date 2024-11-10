using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using ZaupShop.Helpers;

namespace ZaupShop.Commands
{
    public class CommandCost : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "cost";
        public string Help => "Tells you the cost of a selected item.";
        public string Syntax => "[v.]<name or id>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer playerId, string[] command)
        {
            if (command.Length == 0 || command.Length > 2)
            {
                pluginInstance.SendMessageToPlayer(playerId, "cost_command_usage");
                return;
            }

            bool isVehicle = false;
            string itemName;

            if (command.Length == 1)
            {
                if (command[0].StartsWith("v."))
                {
                    isVehicle = true;
                    itemName = command[0].Substring(2);
                }
                else
                {
                    itemName = command[0];
                }
            }
            else // command.Length == 2
            {
                if (command[0] == "v")
                {
                    isVehicle = true;
                    itemName = command[1];
                }
                else
                {
                    pluginInstance.SendMessageToPlayer(playerId, "cost_command_usage");
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                pluginInstance.SendMessageToPlayer(playerId, "cost_command_usage");
                return;
            }

            ushort id;
            string name;
            string moneyName = Uconomy.Instance.Configuration.Instance.MoneyName;

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(itemName, out id, out name))
                {
                    pluginInstance.SendMessageToPlayer(playerId, "could_not_find", itemName);
                    return;
                }
                ThreadHelper.RunAsynchronously(() =>
                {
                    decimal cost = pluginInstance.ShopDB.GetVehicleCost(id);
                    ThreadHelper.RunSynchronously(() =>
                    {
                        if (cost <= 0m)
                        {
                            pluginInstance.SendMessageToPlayer(playerId, "error_getting_cost", name);
                            return;
                        }
                        pluginInstance.SendMessageToPlayer(playerId, "vehicle_cost_msg", name, cost.ToString("N"), moneyName);
                    });
                });

                return;
            }

            if (!UnturnedHelper.TryGetItemByIdOrName(itemName, out id, out name))
            {
                pluginInstance.SendMessageToPlayer(playerId, "could_not_find", itemName);
                return;
            }
            ThreadHelper.RunAsynchronously(() =>
            {
                decimal itemCost = pluginInstance.ShopDB.GetItemCost(id);
                decimal buybackPrice = pluginInstance.ShopDB.GetItemBuyPrice(id);
                ThreadHelper.RunSynchronously(() =>
                {
                    if (itemCost <= 0 && buybackPrice <= 0)
                    {
                        pluginInstance.SendMessageToPlayer(playerId, "error_getting_cost", name);
                        return;
                    }
                    pluginInstance.SendMessageToPlayer(playerId, "item_cost_msg", name, itemCost.ToString("N"), moneyName, buybackPrice.ToString("N"), moneyName);
                });
            });
        }
    }
}