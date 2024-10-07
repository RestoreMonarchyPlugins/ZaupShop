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

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "buy_command_usage", "Usage: /buy [v.]<name or id> [amount] [quality of 25, 50, 75, or 100] (last 2 optional and only for items, default 1 amount, 100 quality)." },
            { "cost_command_usage", "Usage: /cost [v.]<name or id>." },
            { "sell_command_usage", "Usage: /sell <name or id> [amount] (optional)." },
            { "shop_command_usage", "Usage: /shop <add/rem/chng/buy> [v.]<itemid> <cost>  <cost> is not required for rem, buy is only for items." },
            { "error_giving_item", "There was an error giving you {0}.  You have not been charged." },
            { "error_getting_cost", "There was an error getting the cost of {0}!" },
            { "item_cost_msg", "The item {0} costs {1} {2} to buy and gives {3} {4} when you sell it." },
            { "vehicle_cost_msg", "The vehicle {0} costs {1} {2} to buy." },
            { "item_buy_msg", "You have bought {5} {0} for {1} {2}.  You now have {3} {4}." },
            { "vehicle_buy_msg", "You have bought 1 {0} for {1} {2}.  You now have {3} {4}." },
            { "not_enough_currency_msg", "You do not have enough {0} to buy {1} {2}." },
            { "buy_items_off", "I'm sorry, but the ability to buy items is turned off." },
            { "buy_vehicles_off", "I'm sorry, but the ability to buy vehicles is turned off." },
            { "item_not_available", "I'm sorry, but {0} is not available in the shop." },
            { "vehicle_not_available", "I'm sorry, but {0} is not available in the shop." },
            { "could_not_find", "I'm sorry, I couldn't find an id for {0}." },
            { "sell_items_off", "I'm sorry, but the ability to sell items is turned off." },
            { "not_have_item_sell", "I'm sorry, but you don't have any {0} to sell." },
            { "not_enough_items_sell", "I'm sorry, but you don't have {0} {1} to sell." },
            //{ "not_enough_ammo_sell", "I'm sorry, but you don't have enough ammo in {0} to sell." },
            { "sold_items", "You have sold {0} {1} to the shop and receive {2} {3} in return.  Your balance is now {4} {5}." },
            { "no_sell_price_set", "The shop is not buying {0} right now" },
            { "no_itemid_given", "An itemid is required." },
            { "no_cost_given", "A cost is required." },
            { "invalid_amt", "You have entered in an invalid amount." },
            { "v_not_provided", "You must specify v for vehicle or just an item id.  Ex. /shop rem/101" },
            { "invalid_id_given", "You need to provide a valid item or vehicle id." },
            { "no_permission_shop_chng", "You don't have permission to use the shop chng msg." },
            { "no_permission_shop_add", "You don't have permission to use the shop add msg." },
            { "no_permission_shop_rem", "You don't have permission to use the shop rem msg." },
            { "no_permission_shop_buy", "You don't have permission to use the shop buy msg." },
            { "changed", "changed" },
            { "added", "added" },
            { "changed_or_added_to_shop", "You have {0} the {1} with cost {2} to the shop." },
            { "error_adding_or_changing", "There was an error adding/changing {0}!" },
            { "removed_from_shop", "You have removed the {0} from the shop." },
            { "not_in_shop_to_remove", "{0} wasn't in the shop, so couldn't be removed." },
            { "not_in_shop_to_set_buyback", "{0} isn't in the shop so can't set a buyback price." },
            { "set_buyback_price", "You set the buyback price for {0} to {1} in the shop." },
            { "invalid_shop_command", "You entered an invalid shop command." }
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

        public void Sell(UnturnedPlayer playerid, string[] components)
        {
            
        }

    }
}
