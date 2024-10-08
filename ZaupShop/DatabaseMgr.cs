using fr34kyn01535.Uconomy;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZaupShop.Models;

namespace ZaupShop
{
    public class DatabaseMgr
    {
        private string ConnectionString { get; }

        public DatabaseMgr()
        {
            UconomyConfiguration config = Uconomy.Instance.Configuration.Instance;
            int databasePort = config.DatabasePort == 0 ? 3306 : config.DatabasePort;
            ConnectionString = $"SERVER={config.DatabaseAddress};" +
                               $"DATABASE={config.DatabaseName};" +
                               $"UID={config.DatabaseUsername};" +
                               $"PASSWORD={config.DatabasePassword};" +
                               $"PORT={databasePort};";
        }

        public void CheckSchema()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            // Check and create item shop table
            command.CommandText = $"SHOW TABLES LIKE '{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}'";
            if (command.ExecuteScalar() == null)
            {
                command.CommandText = $@"CREATE TABLE `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` (
                    `id` int(6) NOT NULL,
                    `itemname` varchar(32) NOT NULL,
                    `cost` decimal(15,2) NOT NULL DEFAULT '20.00',
                    `buyback` decimal(15,2) NOT NULL DEFAULT '0.00',
                    PRIMARY KEY (`id`)
                )";
                command.ExecuteNonQuery();
            }

            // Check and create vehicle shop table
            command.CommandText = $"SHOW TABLES LIKE '{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}'";
            if (command.ExecuteScalar() == null)
            {
                command.CommandText = $@"CREATE TABLE `{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}` (
                    `id` int(6) NOT NULL,
                    `vehiclename` varchar(32) NOT NULL,
                    `cost` decimal(15,2) NOT NULL DEFAULT '100.00',
                    PRIMARY KEY (`id`)
                )";
                command.ExecuteNonQuery();
            }

            // Check and add buyback column to item shop table
            command.CommandText = $"SHOW COLUMNS FROM `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` LIKE 'buyback'";
            if (command.ExecuteScalar() == null)
            {
                command.CommandText = $"ALTER TABLE `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` ADD `buyback` decimal(15,2) NOT NULL DEFAULT '0.00'";
                command.ExecuteNonQuery();
            }
        }

        private MySqlConnection createConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public bool AddItem(int id, string name, decimal cost, bool isChange, decimal? buyback = null, bool dontReplace = false)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $@"
                INSERT INTO `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` 
                (`id`, `itemname`, `cost`, `buyback`) 
                VALUES (@id, @name, @cost, IFNULL(@buyback, 0))
                ON DUPLICATE KEY UPDATE 
                `itemname` = VALUES(`itemname`),
                `cost` = CASE 
                    WHEN @dontReplace = 1 THEN `cost`
                    ELSE VALUES(`cost`)
                END,
                `buyback` = CASE 
                    WHEN @dontReplace = 1 THEN `buyback`
                    WHEN @buyback IS NULL THEN `buyback`
                    ELSE VALUES(`buyback`)
                END";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@cost", cost);
            command.Parameters.AddWithValue("@buyback", buyback.HasValue ? buyback.Value : DBNull.Value);
            command.Parameters.AddWithValue("@dontReplace", dontReplace);

            connection.Open();
            int affected = command.ExecuteNonQuery();
            return affected > 0;
        }

        public bool AddVehicle(int id, string name, decimal cost, bool change, bool dontReplace = false)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $@"
                INSERT INTO `{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}` 
                (`id`, `vehiclename`, `cost`) 
                VALUES (@id, @name, @cost)
                ON DUPLICATE KEY UPDATE 
                `vehiclename` = VALUES(`vehiclename`),
                `cost` = CASE 
                    WHEN @dontReplace = 1 THEN `cost`
                    ELSE VALUES(`cost`)
                END";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@cost", cost);
            command.Parameters.AddWithValue("@dontReplace", dontReplace);

            connection.Open();
            int affected = command.ExecuteNonQuery();
            return affected > 0;
        }

        public decimal GetItemCost(int id)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT `cost` FROM `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` WHERE `id` = @id";
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            object result = command.ExecuteScalar();
            return result != null && decimal.TryParse(result.ToString(), out decimal cost) ? cost : 0;
        }

        public decimal GetVehicleCost(int id)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT `cost` FROM `{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}` WHERE `id` = @id";
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            object result = command.ExecuteScalar();
            return result != null && decimal.TryParse(result.ToString(), out decimal cost) ? cost : 0;
        }

        public bool DeleteItem(int id)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"DELETE FROM `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` WHERE id=@id";
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            int affected = command.ExecuteNonQuery();
            return affected > 0;
        }

        public bool DeleteVehicle(int id)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"DELETE FROM `{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}` WHERE id=@id";
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            int affected = command.ExecuteNonQuery();
            return affected > 0;
        }

        public bool SetBuyPrice(int id, decimal cost)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"UPDATE `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` SET `buyback`=@cost WHERE id=@id";
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@cost", cost);

            connection.Open();
            int affected = command.ExecuteNonQuery();
            return affected > 0;
        }

        public decimal GetItemBuyPrice(int id)
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT `buyback` FROM `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}` WHERE `id` = @id";
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            object result = command.ExecuteScalar();
            return result != null && decimal.TryParse(result.ToString(), out decimal buyback) ? buyback : 0;
        }

        public IEnumerable<ItemShop> GetAllItemShop()
        {
            List<ItemShop> items = [];

            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT * FROM `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}`";

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new ItemShop
                {
                    Id = reader.GetUInt16("id"),
                    ItemName = reader.GetString("itemname"),
                    BuyPrice = reader.GetDecimal("cost"),
                    SellPrice = reader.GetDecimal("buyback")
                });
            }

            return items;
        }

        public int DeleteItemShop()
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"DELETE FROM `{ZaupShop.Instance.Configuration.Instance.ItemShopTableName}`";

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public IEnumerable<VehicleShop> GetAllVehicleShop()
        {
            List<VehicleShop> vehicles = [];

            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"SELECT * FROM `{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}`";

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                vehicles.Add(new VehicleShop
                {
                    Id = reader.GetUInt16("id"),
                    VehicleName = reader.GetString("vehiclename"),
                    BuyPrice = reader.GetDecimal("cost")
                });
            }

            return vehicles;
        }

        public int DeleteVehicleShop()
        {
            using var connection = createConnection();
            using var command = connection.CreateCommand();

            command.CommandText = $"DELETE FROM `{ZaupShop.Instance.Configuration.Instance.VehicleShopTableName}`";

            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
}