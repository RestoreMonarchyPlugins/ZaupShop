using Newtonsoft.Json;
using Rocket.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ZaupShop.Helpers
{
    internal static class ShopImportExportHelper
    {
        internal static string DownloadAndSaveFile<T>(string url, string pluginDirectory)
        {
            string fileName = Path.GetFileName(url);
            if (!url.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Log($"{fileName} must be a json file.");
                return null;
            }

            using (WebClient wc = new WebClient())
            {
                Logger.Log($"Downloading {fileName} from {url}...");
                string content = wc.DownloadString(url);

                if (string.IsNullOrEmpty(content))
                {
                    Logger.Log($"Content of {fileName} is empty.");
                    return null;
                }

                try
                {
                    content = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<IEnumerable<T>>(content), Formatting.Indented);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error parsing {fileName}: {ex.Message}");
                    return null;
                }

                string path = Path.Combine(pluginDirectory, fileName);
                File.WriteAllText(path, content);
                Logger.Log($"Downloaded {fileName} to {path}");

                return path;
            }
        }

        internal static void ImportItems<T>(List<T> items, string tableName, Action<T> addItemAction)
        {
            string itemType = typeof(T).Name.Contains("Vehicle") ? "vehicles" : "items";
            Logger.Log($"Uploading {items.Count} {itemType} to the {tableName} table in database now...");
            foreach (T item in items)
            {
                addItemAction(item);
            }
            Logger.Log($"Done! Finished uploading {items.Count} items to the {tableName} table in database!");
        }

        internal static string ExportItems<T>(IEnumerable<T> items, string tableName, string pluginDirectory, string fileName)
        {
            if (!items.Any())
            {
                Logger.Log($"{tableName} table is empty");
                return null;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{tableName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
            } else if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }
            
            string json = JsonConvert.SerializeObject(items, Formatting.Indented);
            string path = Path.Combine(pluginDirectory, fileName);
            File.WriteAllText(path, json);

            string itemType = typeof(T).Name.Contains("Vehicle") ? "vehicles" : "items";
            Logger.Log($"Exported {items.Count()} {itemType} to: {fileName}");

            return path;
        }
    }
}