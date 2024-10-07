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
            IEnumerable<Asset> assets = GetAllVehicles();

            if (!ushort.TryParse(idOrName, out ushort id))
            {
                idOrName = idOrName.ToLower();
            }            

            return assets.FirstOrDefault(a => (id != 0 && a.id == id) || (a?.FriendlyName != null && a.FriendlyName.ToLower().Contains(idOrName)));    
        }

        internal static ItemAsset GetItemByIdOrName(string idOrName)
        {
            IEnumerable<ItemAsset> itemAssets = GetAllItems();

            if (!ushort.TryParse(idOrName, out ushort id))
            {
                idOrName = idOrName.ToLower();
            }

            return itemAssets.FirstOrDefault(a => (id != 0 && a.id == id) || (a?.FriendlyName != null && a.FriendlyName.ToLower().Contains(idOrName)));
        }

        internal static IEnumerable<Asset> GetAllVehicles()
        {
            List<VehicleAsset> vehicleAssets = new();
            List<VehicleRedirectorAsset> vehicleRedirectorAssets = new();

            Assets.find(vehicleAssets);
            Assets.find(vehicleRedirectorAssets);

            Asset[] assets = [.. vehicleAssets, .. vehicleRedirectorAssets];

            return assets.Where(x => !string.IsNullOrEmpty(x.FriendlyName) && x.FriendlyName != "Name").ToArray();

        }

        internal static IEnumerable<ItemAsset> GetAllItems()
        {
            List<ItemAsset> itemAssets = new();

            Assets.find(itemAssets);

            return itemAssets.Where(x => !string.IsNullOrEmpty(x.FriendlyName) && x.FriendlyName != "Name").ToArray();
        }
    }
}
