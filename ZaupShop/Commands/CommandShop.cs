using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using ZaupShop.Helpers;

namespace ZaupShop.Commands
{
    public class CommandShop : IRocketCommand
    {
        private ZaupShop pluginInstance => ZaupShop.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "shop";
        public string Help => "Allows admins to change, add, or remove items/vehicles from the shop.";
        public string Syntax => "<add | rem | chng | buy> [v.]<itemid> <cost>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "shop.*", "shop.add", "shop.rem", "shop.remove" };

        public void Execute(IRocketPlayer caller, string[] msg)
        {
            bool isConsole = caller is ConsolePlayer;
            string subCommand = msg.ElementAtOrDefault(0)?.ToLower() ?? null;

            if (subCommand == null 
                || (subCommand == "add" && msg.Length < 3)
                || ((subCommand == "remove" || subCommand == "rem") && msg.Length < 2)
                || (subCommand != "add" && subCommand != "remove" && subCommand != "rem"))
            {
                pluginInstance.SendMessageToPlayer(caller, "shop_command_usage");
                return;
            }

            string[] type = Parser.getComponentsFromSerial(msg[1], '.');
            if (type.Length > 1 && type[0] != "v")
            {
                pluginInstance.SendMessageToPlayer(caller, "v_not_provided");
                return;
            }

            if (!ushort.TryParse(type.Length > 1 ? type[1] : type[0], out ushort id))
            {
                pluginInstance.SendMessageToPlayer(caller, "invalid_id_given");
                return;
            }

            switch (subCommand)
            {
                case "add":
                    HandleAddOrChange(caller, msg, type, id, isConsole);
                    break;
                case "rem":
                case "remove":
                    HandleRemove(caller, type, id, isConsole);
                    break;
                default:
                    pluginInstance.SendMessageToPlayer(caller, "invalid_shop_command");
                    break;
            }
        }

        private void HandleAddOrChange(IRocketPlayer caller, string[] msg, string[] type, ushort id, bool isConsole)
        {
            if (!HasRequiredPermission(caller, msg[0], isConsole)) return;

            // add basically adds or change, this is kept for backward compatibility
            bool isChange = true;
            bool isVehicle = type[0] == "v";

            if (!decimal.TryParse(msg[2], out decimal cost))
            {
                pluginInstance.SendMessageToPlayer(caller, "invalid_cost");
                return;
            }

            decimal? buyback = null;
            if (!isVehicle && msg.Length > 3)
            {
                if (!decimal.TryParse(msg[3], out decimal buyBackDecimal))
                {
                    pluginInstance.SendMessageToPlayer(caller, "invalid_buyback");
                    return;
                }
                else
                {
                    buyback = buyBackDecimal;
                }
            }

            string name;
            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(id.ToString(), out _, out name))
                {
                    pluginInstance.SendMessageToPlayer(caller, "invalid_id_given");
                    return;
                }
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(id.ToString(), out _, out name))
                {
                    pluginInstance.SendMessageToPlayer(caller, "invalid_id_given");
                    return;
                }
            }

            ThreadHelper.RunAsynchronously(() =>
            {
                bool success = isVehicle
                    ? pluginInstance.ShopDB.AddVehicle(id, name, cost, isChange)
                    : pluginInstance.ShopDB.AddItem(id, name, cost, isChange, buyback);

                ThreadHelper.RunSynchronously(() =>
                {
                    if (success)
                    {
                        if (buyback.HasValue)
                        {
                            pluginInstance.SendMessageToPlayer(caller, "changed_or_added_to_shop_with_buyback", name, cost.ToString("N"), buyback.Value.ToString("N"));
                        }
                        else
                        {
                            pluginInstance.SendMessageToPlayer(caller, "changed_or_added_to_shop", name, cost.ToString("N"));
                        }
                    }
                    else
                    {
                        pluginInstance.SendMessageToPlayer(caller, "error_adding_or_changing", name);
                    }
                });
            });
        }

        private void HandleRemove(IRocketPlayer caller, string[] type, ushort id, bool isConsole)
        {
            if (!HasRequiredPermission(caller, "rem", isConsole)) return;

            bool isVehicle = type[0] == "v";
            string name;

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(id.ToString(), out _, out name))
                {
                    pluginInstance.SendMessageToPlayer(caller, "invalid_id_given");
                    return;
                }
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(id.ToString(), out _, out name))
                {
                    pluginInstance.SendMessageToPlayer(caller, "invalid_id_given");
                    return;
                }
            }

            ThreadHelper.RunAsynchronously(() =>
            {
                bool success = isVehicle
                    ? pluginInstance.ShopDB.DeleteVehicle(id)
                    : pluginInstance.ShopDB.DeleteItem(id);

                ThreadHelper.RunSynchronously(() =>
                {
                    if (success)
                    {
                        pluginInstance.SendMessageToPlayer(caller, "removed_from_shop", name);
                    } else
                    {
                        pluginInstance.SendMessageToPlayer(caller, "not_in_shop_to_remove", name);
                    }
                });
            });
        }

        private bool HasRequiredPermission(IRocketPlayer caller, string action, bool isConsole)
        {
            if (isConsole || caller.HasPermission("shop.*")) return true;
            string permission = $"shop.{action}";
            if (!caller.HasPermission(permission))
            {
                pluginInstance.SendMessageToPlayer(caller, $"no_permission_shop_{action}");
                return false;
            }
            return true;
        }
    }
}