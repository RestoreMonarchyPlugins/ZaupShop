# ZaupShop
Simple shop for items and vehicles using Uconomy currency.

## Features
- The plugin uses **Uconomy** to handle the currency (it is required)
- Requires **MySQL** to store the items and vehicles prices
- This version of the plugin makes all database calls asynchronously (**lag-free**)
- Allows admins to add items and vehicles from in-game using the **/shop** command

The plugin uses the same database as Uconomy to store the items and vehicles prices, that's why you don't have to enter the database information in the configuration file.

To save you time, we prepared the prices for almost all items and vehicles and share them at [UnturnedServerConfigs](https://github.com/RestoreMonarchyPlugins/UnturnedServerConfigs) GitHub repository. 
You can easily import them into your database, by running the following commands in the server console after you have installed the plugin:
```sh
importitemshop https://raw.githubusercontent.com/RestoreMonarchyPlugins/UnturnedServerConfigs/refs/heads/master/shop/itemshop.json

importvehicleshop https://raw.githubusercontent.com/RestoreMonarchyPlugins/UnturnedServerConfigs/refs/heads/master/shop/vehicleshop.json
```
You can always edit the prices to your liking using MySQL Server client like HeidiSQL, phpMyAdmin or MySQL Workbench.

## Credits
This is our 2.0 version of popular ZaupShop plugin originally created by **Zamirathe.** We fixed it, made it lag free and added `uploadvehicleshop` and `uploaditemshop` commands.

## Commands
### Player Commands
* `/buy <item> [amount]` - Buy an item (id or name) from the shop
* `/buy v.<vehicle> [amount]` - Buy a vehicle (id or name) from the shop
* `/cost <item>` - Get the cost of an item (id or name) from the shop
* `/cost v.<vehicle>` - Get the cost of a vehicle (id or name) from the shop
* `/sell <item> [amount]` - Sell an item (id or name) to the shop

### Admin Commands
* `/shop add <itemId> <cost> <buyback>` - Add an item to the shop (buyback is the price the shop will buy the item back for aka sell price)
* `/shop add v.<vehicleId> <cost>` - Add a vehicle to the shop
* `/shop remove <itemId>` - Remove an item from the shop
* `/shop remove v.<vehicleId>` - Remove a vehicle from the shop

### Console Commands
* `uploaditemshop` - Adds all item assets from the server to the `ItemShopTableName` table in database
* `uploadvehicleshop` - Adds all vehicle assets from the server to the `VehicleShopTableName` table in database

## Permissions
```xml
<!-- Player Permissions -->
<Permission Cooldown="0">buy</Permission>
<Permission Cooldown="0">cost</Permission>
<Permission Cooldown="0">sell</Permission>

<!-- Admin Permissions, don't give them to players! -->
<Permission Cooldown="0">shop</Permission>
<Permission Cooldown="0">shop.add</Permission>
<Permission Cooldown="0">shop.remove</Permission>
```

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<ZaupShopConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <ItemShopTableName>uconomyitemshop</ItemShopTableName>
  <VehicleShopTableName>uconomyvehicleshop</VehicleShopTableName>
  <CanBuyItems>true</CanBuyItems>
  <CanBuyVehicles>true</CanBuyVehicles>
  <CanSellItems>true</CanSellItems>
  <QualityCounts>true</QualityCounts>
</ZaupShopConfiguration>
```

## Translations
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="buy_command_usage" Value="Usage: /buy [v.]&lt;name or id&gt; [amount] (amount optional, default: 1)" />
  <Translation Id="cost_command_usage" Value="Usage: /cost [v.]&lt;name or id&gt;" />
  <Translation Id="sell_command_usage" Value="Usage: /sell &lt;name or id&gt; [amount] (amount optional)" />
  <Translation Id="shop_command_usage" Value="Usage: /shop &lt;add/remove&gt; [v.]&lt;itemid&gt; [cost] [buyback]" />
  <Translation Id="error_giving_item" Value="Error: Unable to give you {0}. You have not been charged" />
  <Translation Id="error_getting_cost" Value="Error: {0} is not for sale" />
  <Translation Id="item_cost_msg" Value="Item: {0} | Buy: {1} {2} | Sell: {3} {4}" />
  <Translation Id="vehicle_cost_msg" Value="Vehicle: {0} | Buy: {1} {2}" />
  <Translation Id="item_buy_msg" Value="Purchase successful: {5} {0} for {1} {2}. Your balance: {3} {4}" />
  <Translation Id="vehicle_buy_msg" Value="Purchase successful: 1 {0} for {1} {2}. Your balance: {3} {4}" />
  <Translation Id="not_enough_currency_msg" Value="Insufficient funds: You need {0} {1} to buy x{2} {3}" />
  <Translation Id="buy_items_off" Value="Error: Item purchasing is currently disabled" />
  <Translation Id="buy_vehicles_off" Value="Error: Vehicle purchasing is currently disabled" />
  <Translation Id="item_not_available" Value="Error: {0} is not available in the shop" />
  <Translation Id="vehicle_not_available" Value="Error: {0} is not available in the shop" />
  <Translation Id="could_not_find" Value="Error: Unable to find an ID for {0}" />
  <Translation Id="sell_items_off" Value="Error: Item selling is currently disabled" />
  <Translation Id="not_have_item_sell" Value="Error: You don't have any {0} to sell" />
  <Translation Id="not_enough_items_sell" Value="Error: You don't have {0} {1} to sell" />
  <Translation Id="sold_items" Value="Sale successful: {0} {1} sold for {2} {3}. Your balance: {4} {5}" />
  <Translation Id="no_sell_price_set" Value="Error: {0} cannot be sold to the shop at this time" />
  <Translation Id="no_itemid_given" Value="Error: Item ID is required" />
  <Translation Id="no_cost_given" Value="Error: Cost is required" />
  <Translation Id="invalid_amt" Value="Error: Invalid amount entered" />
  <Translation Id="invalid_cost" Value="Error: Invalid cost value entered" />
  <Translation Id="invalid_buyback" Value="Error: Invalid buyback value entered" />
  <Translation Id="v_not_provided" Value="Error: Specify 'v' for vehicle or use item ID. Example: /shop add 363 1000 50" />
  <Translation Id="invalid_id_given" Value="Error: Please provide a valid item or vehicle ID" />
  <Translation Id="no_permission_shop_add" Value="Error: You don't have permission to use the shop add command" />
  <Translation Id="no_permission_shop_rem" Value="Error: You don't have permission to use the shop remove command" />
  <Translation Id="changed_or_added_to_shop" Value="Success: {0} added to the shop with cost {1}" />
  <Translation Id="changed_or_added_to_shop_with_buyback" Value="Success: {0} added to the shop with cost {1} and buyback {2}" />
  <Translation Id="error_adding_or_changing" Value="Error: Failed to add or update {0}" />
  <Translation Id="removed_from_shop" Value="Success: {0} removed from the shop" />
  <Translation Id="not_in_shop_to_remove" Value="Error: {0} is not in the shop and cannot be removed" />
  <Translation Id="not_in_shop_to_set_buyback" Value="Error: {0} is not in the shop and cannot have a buyback price set" />
  <Translation Id="set_buyback_price" Value="Success: Buyback price for {0} set to {1}" />
  <Translation Id="invalid_shop_command" Value="Error: Invalid shop command entered" />
</Translations>
```

