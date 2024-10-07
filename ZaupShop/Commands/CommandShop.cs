using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using ZaupShop.Helpers;
using Logger = Rocket.Core.Logging.Logger;

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
        public List<string> Permissions => new List<string> { "shop.*", "shop.add", "shop.rem", "shop.chng", "shop.buy" };

        public void Execute(IRocketPlayer caller, string[] msg)
        {
            bool isConsole = caller is ConsolePlayer;
            if (!HasPermission(caller, isConsole))
            {
                UnturnedChat.Say(caller, "You don't have permission to use the /shop command.");
                return;
            }

            if (msg.Length == 0 || msg.Length < 2 || (msg.Length == 2 && msg[0] != "rem"))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("shop_command_usage"));
                return;
            }

            string[] type = Parser.getComponentsFromSerial(msg[1], '.');
            if (type.Length > 1 && type[0] != "v")
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("v_not_provided"));
                return;
            }

            if (!ushort.TryParse(type.Length > 1 ? type[1] : type[0], out ushort id))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_id_given"));
                return;
            }

            switch (msg[0].ToLower())
            {
                case "add":
                case "chng":
                    HandleAddOrChange(caller, msg, type, id, isConsole);
                    break;
                case "rem":
                    HandleRemove(caller, type, id, isConsole);
                    break;
                case "buy":
                    HandleBuy(caller, msg, id, isConsole);
                    break;
                default:
                    UnturnedChat.Say(caller, pluginInstance.Translate("invalid_shop_command"));
                    break;
            }
        }

        private bool HasPermission(IRocketPlayer caller, bool isConsole)
        {
            if (isConsole) return true;
            if (caller is UnturnedPlayer player && player.IsAdmin) return true;
            return caller.HasPermission("shop.*") || Permissions.Any(p => caller.HasPermission(p));
        }

        private void HandleAddOrChange(IRocketPlayer caller, string[] msg, string[] type, ushort id, bool isConsole)
        {
            if (!HasRequiredPermission(caller, msg[0], isConsole)) return;

            bool isChange = msg[0].ToLower() == "chng";
            string action = pluginInstance.Translate(isChange ? "changed" : "added");
            bool isVehicle = type[0] == "v";

            if (!decimal.TryParse(msg[2], out decimal cost))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_cost"));
                return;
            }

            string name;
            bool success;

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(id.ToString(), out _, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("invalid_id_given"));
                    return;
                }
                success = pluginInstance.ShopDB.AddVehicle(id, name, cost, isChange);
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(id.ToString(), out _, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("invalid_id_given"));
                    return;
                }
                success = pluginInstance.ShopDB.AddItem(id, name, cost, isChange);
            }

            string message = success
                ? pluginInstance.Translate("changed_or_added_to_shop", action, name, cost)
                : pluginInstance.Translate("error_adding_or_changing", name);

            UnturnedChat.Say(caller, message);
        }

        private void HandleRemove(IRocketPlayer caller, string[] type, ushort id, bool isConsole)
        {
            if (!HasRequiredPermission(caller, "rem", isConsole)) return;

            bool isVehicle = type[0] == "v";
            string name;
            bool success;

            if (isVehicle)
            {
                if (!UnturnedHelper.TryGetVehicleByIdOrName(id.ToString(), out _, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("invalid_id_given"));
                    return;
                }
                success = pluginInstance.ShopDB.DeleteVehicle(id);
            }
            else
            {
                if (!UnturnedHelper.TryGetItemByIdOrName(id.ToString(), out _, out name))
                {
                    UnturnedChat.Say(caller, pluginInstance.Translate("invalid_id_given"));
                    return;
                }
                success = pluginInstance.ShopDB.DeleteItem(id);
            }

            string message = success
                ? pluginInstance.Translate("removed_from_shop", name)
                : pluginInstance.Translate("not_in_shop_to_remove", name);

            UnturnedChat.Say(caller, message);
        }

        private void HandleBuy(IRocketPlayer caller, string[] msg, ushort id, bool isConsole)
        {
            if (!HasRequiredPermission(caller, "buy", isConsole)) return;

            if (!UnturnedHelper.TryGetItemByIdOrName(id.ToString(), out _, out string name))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_id_given"));
                return;
            }

            if (!decimal.TryParse(msg[2], out decimal buyPrice))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("invalid_cost"));
                return;
            }

            bool success = pluginInstance.ShopDB.SetBuyPrice(id, buyPrice);
            string message = success
                ? pluginInstance.Translate("set_buyback_price", name, buyPrice.ToString())
                : pluginInstance.Translate("not_in_shop_to_set_buyback", name);

            UnturnedChat.Say(caller, message);
        }

        private bool HasRequiredPermission(IRocketPlayer caller, string action, bool isConsole)
        {
            if (isConsole || caller.HasPermission("shop.*")) return true;
            string permission = $"shop.{action}";
            if (!caller.HasPermission(permission))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate($"no_permission_shop_{action}"));
                return false;
            }
            return true;
        }
    }
}