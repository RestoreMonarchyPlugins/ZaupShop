using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
namespace ZaupShop.Helpers
{
    internal static class UnturnedHelper
    {
        internal static bool TryGetVehicleByIdOrName(string idOrName, out ushort vehicleId, out string name)
        {
            Asset asset = GetVehicleByIdOrName(idOrName);

            if (asset == null)
            {
                vehicleId = 0;
                name = null;
                return false;
            }
            vehicleId = asset.id;
            name = asset.FriendlyName;
            return true;
        }

        internal static bool TryGetItemByIdOrName(string idOrName, out ushort itemId, out string name)
        {
            Asset asset = GetItemByIdOrName(idOrName);
            if (asset == null)
            {
                itemId = 0;
                name = null;
                return false;
            }
            itemId = asset.id;
            name = asset.FriendlyName;
            return true;
        }

        internal static Asset GetVehicleByIdOrName(string idOrName)
        {
            // Check if the input is an ID first
            if (ushort.TryParse(idOrName, out ushort id))
            {
                IEnumerable<Asset> assets = GetAllVehicles();
                return assets.FirstOrDefault(a => a.id == id);
            }

            // Otherwise, search by name with improved prioritization
            return FindAssetByNameWithPriority(GetAllVehicles(), idOrName);
        }

        internal static ItemAsset GetItemByIdOrName(string idOrName)
        {
            // Check if the input is an ID first
            if (ushort.TryParse(idOrName, out ushort id))
            {
                IEnumerable<ItemAsset> itemAssets = GetAllItems();
                return itemAssets.FirstOrDefault(a => a.id == id);
            }

            // Otherwise, search by name with improved prioritization
            return FindAssetByNameWithPriority(GetAllItems(), idOrName);
        }

        private static T FindAssetByNameWithPriority<T>(IEnumerable<T> assets, string searchTerm) where T : Asset
        {
            if (string.IsNullOrEmpty(searchTerm))
                return null;

            string searchLower = searchTerm.ToLower();

            // First priority: Exact match
            var exactMatch = assets.FirstOrDefault(a =>
                a?.FriendlyName != null &&
                a.FriendlyName.ToLower() == searchLower);

            if (exactMatch != null)
                return exactMatch;

            // Second priority: Starts with the search term (ordered by ID ascending)
            var startsWithMatches = assets.Where(a =>
                a?.FriendlyName != null &&
                a.FriendlyName.ToLower().StartsWith(searchLower))
                .OrderBy(a => a.id);

            var firstStartsWith = startsWithMatches.FirstOrDefault();
            if (firstStartsWith != null)
                return firstStartsWith;

            // Third priority: Contains the search term (ordered by ID ascending)
            return assets.Where(a =>
                a?.FriendlyName != null &&
                a.FriendlyName.ToLower().Contains(searchLower))
                .OrderBy(a => a.id)
                .FirstOrDefault();
        }

        internal static IEnumerable<Asset> GetAllVehicles()
        {
            List<VehicleAsset> vehicleAssets = new();
            List<VehicleRedirectorAsset> vehicleRedirectorAssets = new();
            Assets.find(vehicleAssets);
            Assets.find(vehicleRedirectorAssets);
            Asset[] assets = [.. vehicleAssets, .. vehicleRedirectorAssets];

            return assets.Where(x => x.id != 0 && !string.IsNullOrEmpty(x.FriendlyName) && x.FriendlyName != "Name").ToArray();
        }

        internal static IEnumerable<ItemAsset> GetAllItems()
        {
            List<ItemAsset> itemAssets = new();
            Assets.find(itemAssets);

            return itemAssets.Where(x => !string.IsNullOrEmpty(x.itemName) && x.itemName != "Name" && !x.isPro).ToArray();
        }
    }
}