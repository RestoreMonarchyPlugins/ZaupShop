﻿using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using Logger = Rocket.Core.Logging.Logger;

namespace ZaupShop
{
    public class ZaupShop : RocketPlugin<ZaupShopConfiguration>
    {
        public DatabaseMgr ShopDB;
        public static ZaupShop Instance;

        public UnityEngine.Color MessageColor { get; set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, UnityEngine.Color.green);

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
            { "buy_command_usage", "Usage: /buy [[b]][v.]<name or id>[[/b]] [amount] (amount optional, default: 1)" },
            { "cost_command_usage", "Usage: /cost [[b]][v.]<name or id>[[/b]]" },
            { "sell_command_usage", "Usage: /sell [[b]]<name or id>[[/b]] [amount] (amount optional)" },
            { "shop_command_usage", "Usage: /shop [[b]]<add/remove>[[/b]] [v.]<itemid> [cost] [buyback]" },
            { "error_giving_item", "Error: Unable to give you [[b]]{0}[[/b]]. You have not been charged" },
            { "error_getting_cost", "Error: [[b]]{0}[[/b]] is not for sale" },
            { "item_cost_msg", "Item: [[b]]{0}[[/b]] | Buy: [[b]]{1} {2}[[/b]] | Sell: [[b]]{3} {4}[[/b]]" },
            { "vehicle_cost_msg", "Vehicle: [[b]]{0}[[/b]] | Buy: [[b]]{1} {2}[[/b]]" },
            { "item_buy_msg", "Purchase successful: [[b]]{5}x {0}[[/b]] for [[b]]{1} {2}[[/b]]. Your balance: [[b]]{3} {4}[[/b]]" },
            { "vehicle_buy_msg", "Purchase successful: [[b]]1x {0}[[/b]] for [[b]]{1} {2}[[/b]]. Your balance: [[b]]{3} {4}[[/b]]" },
            { "not_enough_currency_msg", "Insufficient funds: You need [[b]]{0} {1}[[/b]] to buy [[b]]x{2} {3}[[/b]]" },
            { "buy_items_off", "Error: Item purchasing is currently disabled" },
            { "buy_vehicles_off", "Error: Vehicle purchasing is currently disabled" },
            { "item_not_available", "Error: [[b]]{0}[[/b]] is not available in the shop" },
            { "vehicle_not_available", "Error: [[b]]{0}[[/b]] is not available in the shop" },
            { "could_not_find", "Error: Unable to find an ID for [[b]]{0}[[/b]]" },
            { "sell_items_off", "Error: Item selling is currently disabled" },
            { "not_have_item_sell", "Error: You don't have any [[b]]{0}[[/b]] to sell" },
            { "not_enough_items_sell", "Error: You don't have [[b]]{0} {1}[[/b]] to sell" },
            { "sold_items", "Sale successful: [[b]]{0} {1}[[/b]] sold for [[b]]{2} {3}[[/b]]. Your balance: [[b]]{4} {5}[[/b]]" },
            { "no_sell_price_set", "Error: [[b]]{0}[[/b]] cannot be sold to the shop at this time" },
            { "no_itemid_given", "Error: Item ID is required" },
            { "no_cost_given", "Error: Cost is required" },
            { "invalid_amt", "Error: Invalid amount entered" },
            { "invalid_cost", "Error: Invalid cost value entered" },
            { "invalid_buyback", "Error: Invalid buyback value entered" },
            { "v_not_provided", "Error: Specify [[b]]'v'[[/b]] for vehicle or use item ID. Example: /shop add 363 1000 50" },
            { "invalid_id_given", "Error: Please provide a valid item or vehicle ID" },
            { "no_permission_shop_add", "Error: You don't have permission to use the shop add command" },
            { "no_permission_shop_rem", "Error: You don't have permission to use the shop remove command" },
            { "changed_or_added_to_shop", "Success: [[b]]{0}[[/b]] added to the shop with cost [[b]]{1}[[/b]]" },
            { "changed_or_added_to_shop_with_buyback", "Success: [[b]]{0}[[/b]] added to the shop with cost [[b]]{1}[[/b]] and buyback [[b]]{2}[[/b]]" },
            { "error_adding_or_changing", "Error: Failed to add or update [[b]]{0}[[/b]]" },
            { "removed_from_shop", "Success: [[b]]{0}[[/b]] removed from the shop" },
            { "not_in_shop_to_remove", "Error: [[b]]{0}[[/b]] is not in the shop and cannot be removed" },
            { "not_in_shop_to_set_buyback", "Error: [[b]]{0}[[/b]] is not in the shop and cannot have a buyback price set" },
            { "set_buyback_price", "Success: Buyback price for [[b]]{0}[[/b]] set to [[b]]{1}[[/b]]" },
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

        internal void SendMessageToPlayer(IRocketPlayer player, string translationKey, params object[] placeholder)
        {
            string msg = Translate(translationKey, placeholder);
            msg = msg.Replace("[[", "<").Replace("]]", ">");
            if (player is ConsolePlayer)
            {
                Logger.Log(msg);
                return;
            }

            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
            if (unturnedPlayer != null)
            {
                ChatManager.serverSendMessage(msg, MessageColor, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, Configuration.Instance.MessageIconUrl, true);
            }
        }
    }
}
