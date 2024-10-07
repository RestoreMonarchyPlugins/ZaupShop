using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using System;
using Logger = Rocket.Core.Logging.Logger;

namespace ZaupShop
{
    public class ZaupShop : RocketPlugin<ZaupShopConfiguration>
    {
        public DatabaseMgr ShopDB;
        public static ZaupShop Instance;

        protected override void Load()
        {
            Instance = this;

            ShopDB = new DatabaseMgr();
            ShopDB.CheckSchema();

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "buy_command_usage", "Usage: /buy [v.]<name or id> [amount] (amount optional, default: 1)" },
            { "cost_command_usage", "Usage: /cost [v.]<name or id>" },
            { "sell_command_usage", "Usage: /sell <name or id> [amount] (amount optional)" },
            { "shop_command_usage", "Usage: /shop <add/rem/chng/buy> [v.]<itemid> [cost] [buyback]" },
            { "error_giving_item", "Error: Unable to give you {0}. You have not been charged" },
            { "error_getting_cost", "Error: {0} is not for sale" },
            { "item_cost_msg", "Item: {0} | Buy: {1} {2} | Sell: {3} {4}" },
            { "vehicle_cost_msg", "Vehicle: {0} | Buy: {1} {2}" },
            { "item_buy_msg", "Purchase successful: {5} {0} for {1} {2}. Your balance: {3} {4}" },
            { "vehicle_buy_msg", "Purchase successful: 1 {0} for {1} {2}. Your balance: {3} {4}" },
            { "not_enough_currency_msg", "Insufficient funds: You need {0} to buy {1} {2}" },
            { "buy_items_off", "Error: Item purchasing is currently disabled" },
            { "buy_vehicles_off", "Error: Vehicle purchasing is currently disabled" },
            { "item_not_available", "Error: {0} is not available in the shop" },
            { "vehicle_not_available", "Error: {0} is not available in the shop" },
            { "could_not_find", "Error: Unable to find an ID for {0}" },
            { "sell_items_off", "Error: Item selling is currently disabled" },
            { "not_have_item_sell", "Error: You don't have any {0} to sell" },
            { "not_enough_items_sell", "Error: You don't have {0} {1} to sell" },
            { "sold_items", "Sale successful: {0} {1} sold for {2} {3}. Your balance: {4} {5}" },
            { "no_sell_price_set", "Error: {0} cannot be sold to the shop at this time" },
            { "no_itemid_given", "Error: Item ID is required" },
            { "no_cost_given", "Error: Cost is required" },
            { "invalid_amt", "Error: Invalid amount entered" },
            { "invalid_cost", "Error: Invalid cost value entered" },
            { "invalid_buyback", "Error: Invalid buyback value entered" },
            { "v_not_provided", "Error: Specify 'v' for vehicle or use item ID. Example: /shop rem/101" },
            { "invalid_id_given", "Error: Please provide a valid item or vehicle ID" },
            { "no_permission_shop_chng", "Error: You don't have permission to use the shop change command" },
            { "no_permission_shop_add", "Error: You don't have permission to use the shop add command" },
            { "no_permission_shop_rem", "Error: You don't have permission to use the shop remove command" },
            { "no_permission_shop_buy", "Error: You don't have permission to use the shop buy command" },
            { "changed", "changed" },
            { "added", "added" },
            { "changed_or_added_to_shop", "Success: {1} {0} to the shop with cost {2}" },
            { "changed_or_added_to_shop_with_buyback", "Success: {1} {0} to the shop with cost {2} and buyback {3}" },
            { "error_adding_or_changing", "Error: Failed to add/change {0}" },
            { "removed_from_shop", "Success: {0} removed from the shop" },
            { "not_in_shop_to_remove", "Error: {0} is not in the shop and cannot be removed" },
            { "not_in_shop_to_set_buyback", "Error: {0} is not in the shop and cannot have a buyback price set" },
            { "set_buyback_price", "Success: Buyback price for {0} set to {1}" },
            { "invalid_shop_command", "Error: Invalid shop command entered" }
        };

        public delegate void PlayerShopBuy(UnturnedPlayer player, decimal amt, byte items, ushort item, string type="item");
        public event PlayerShopBuy OnShopBuy;
        public delegate void PlayerShopSell(UnturnedPlayer player, decimal amt, byte items, ushort item);
        public event PlayerShopSell OnShopSell;

        internal void TriggerOnShopBuy(UnturnedPlayer player, decimal amt, byte items, ushort item, string type = "item")
        {
            if (OnShopBuy != null)
                OnShopBuy(player, amt, items, item, type);
        }

        internal void TriggerOnShopSell(UnturnedPlayer player, decimal amt, byte items, ushort item)
        {
            if (OnShopSell != null)
                OnShopSell(player, amt, items, item);
        }


    }
}
