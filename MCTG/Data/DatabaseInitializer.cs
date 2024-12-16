using System.Data;
using Npgsql;
namespace MCTG.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer()
    {
        _connectionString = DatabaseConf.ConnectionString;
    }
    public void InitializeDB()
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        // Get the DB Name from the connection String
        string dbName = builder.Database;
        // Remove it to establish connection to the DB even if the Database does not exist
        builder.Remove("Database");
        string cs = builder.ToString();
        using (IDbConnection connection = new NpgsqlConnection(cs))
        {
            connection.Open();
            using (IDbCommand command = connection.CreateCommand())
            {
                // Drop the Database if exist
                command.CommandText = $"DROP DATABASE IF EXISTS {dbName} WITH (force)";
                command.ExecuteNonQuery();
                // Then Create it
                command.CommandText = $"CREATE DATABASE {dbName}";
                command.ExecuteNonQuery();
            }
            // Change the connection to the DB
            connection.ChangeDatabase(dbName);
            using (IDbCommand command = connection.CreateCommand())
            {
                // Enabling the function to create UUID for the Users
                command.CommandText = "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"";
                command.ExecuteNonQuery();
                // Create the User table
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users(
                    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                    username VARCHAR(50) NOT NULL UNIQUE,
                    password VARCHAR(100) NOT NULL,
                    password_salt BYTEA NOT NULL,
                    coins INT DEFAULT 20,
                    elo INT DEFAULT 100,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )
                ";
                command.ExecuteNonQuery();
                // Create the UserTokens table
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserTokens(
                    id SERIAL PRIMARY KEY,
                    user_id UUID REFERENCES Users(id),
                    token VARCHAR(256) NOT NULL UNIQUE,
                    expires_at TIMESTAMP DEFAULT (CURRENT_TIMESTAMP + INTERVAL '1 hour')
                )
                ";
                command.ExecuteNonQuery();
                // Create the Cards table instance
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cards(
                    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    name VARCHAR(100) UNIQUE NOT NULL,
                    type VARCHAR(10) CHECK (type IN ('monster', 'spell')) NOT NULL,
                    element_type VARCHAR(10) CHECK (element_type IN ('Fire', 'Water', 'Normal')) NOT NULL,
                    damage INT NOT NULL,
                    quantity INT NOT NULL CHECK (quantity > -1),
                    monster_type varchar(50) CHECK (monster_type IN (
                    'Goblin',
                    'Wizzard',
                    'Dragon',
                    'Knight',
                    'Krake',
                    'FireElve',
                    'Ork')) 
                )
                ";
                command.ExecuteNonQuery();
                // Create the UserStack Table
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserStack(
                    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                    user_id UUID REFERENCES Users(id),
                    card_id UUID REFERENCES Cards(id),
                    quantity INT NOT NULL CHECK (quantity > 0)
                )
                ";
                command.ExecuteNonQuery();
                // Create the Userdeck Table
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserDeck(
                  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                  user_stack_id UUID REFERENCES UserStack(id),
                  quantity INT NOT NULL CHECK (quantity > 0)
                )
                ";
                command.ExecuteNonQuery();
                // Create the Package Table
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS packages(
                    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
                    package_id UUID NOT NULL,
                    card_id UUID REFERENCES Cards(id)
                )";
                command.ExecuteNonQuery();
            }
        }
        InitializeCards();
    }

    private void InitializeCards()
    {
        const string insertQuery = @"
        INSERT INTO Cards (name, type, element_type, damage, quantity, monster_type) VALUES 
        ('Goblin', 'monster', 'Water', 40, 4, 'Goblin'),
        ('Wizard', 'monster', 'Fire', 30, 1, 'Wizzard'),
        ('Dragon', 'monster', 'Fire', 80, 2, 'Dragon'),
        ('Knight', 'monster', 'Normal', 60, 1, 'Knight'),
        ('Kraken', 'monster', 'Water', 70, 1, 'Krake'),
        ('Fire Elf', 'monster', 'Fire', 55, 3, 'FireElve'),
        ('Orc', 'monster', 'Normal', 50, 1, 'Ork'),
        ('Fire Goblin', 'monster', 'Fire', 45, 5, 'Goblin'),
        ('Water Wizard', 'monster', 'Water', 35, 4, 'Wizzard'),
        ('Ice Dragon 1', 'monster', 'Water', 75, 1, 'Dragon'),
        ('Knight of the Flame', 'monster', 'Fire', 65, 1, 'Knight'),
        ('Shadow Kraken', 'monster', 'Normal', 90, 1, 'Krake'),
        ('Forest Fire Elf', 'monster', 'Fire', 60, 12, 'FireElve'),
        ('Savage Orc', 'monster', 'Normal', 55, 1, 'Ork'),
        ('Stone Goblin', 'monster', 'Normal', 40, 1, 'Goblin'),
        ('Lightning Wizard', 'monster', 'Fire', 50, 1, 'Wizzard'),
        ('Ancient Dragon', 'monster', 'Fire', 100, 4, 'Dragon'),
        ('Armored Knight', 'monster', 'Normal', 70, 1, 'Knight'),
        ('Deep Sea Kraken', 'monster', 'Water', 85, 1, 'Krake'),
        ('Firestorm Elf', 'monster', 'Fire', 65, 1, 'FireElve'),
        ('Battle Orc', 'monster', 'Normal', 75, 1, 'Ork'),
        ('Mystic Goblin', 'monster', 'Normal', 45, 6, 'Goblin'),
        ('Frost Wizard', 'monster', 'Water', 55, 1, 'Wizzard'),
        ('Dragon Rider', 'monster', 'Fire', 90, 1, 'Dragon'),
        ('Holy Knight', 'monster', 'Normal', 80, 3, 'Knight'),
        ('Storm Kraken', 'monster', 'Water', 95, 1, 'Krake'),
        ('Inferno Fire Elf', 'monster', 'Fire', 75, 1, 'FireElve'),
        ('Brutal Orc', 'monster', 'Normal', 65, 1, 'Ork'),
        ('Goblin Mage', 'monster', 'Normal', 50, 1, 'Goblin'),
        ('Arcane Wizard', 'monster', 'Fire', 70, 2, 'Wizzard'),
        ('Firebreath Dragon', 'monster', 'Fire', 95, 1, 'Dragon'),
        ('Shadow Knight', 'monster', 'Normal', 85, 1, 'Knight'),
        ('Chained Kraken', 'monster', 'Water', 100, 1, 'Krake'),
        ('Sun Elf', 'monster', 'Fire', 80, 1, 'FireElve'),
        ('War Orc', 'monster', 'Normal', 70, 1, 'Ork'),
        ('Goblin Assassin', 'monster', 'Normal', 55, 5, 'Goblin'),
        ('Enchanted Wizard', 'monster', 'Water', 65, 1, 'Wizzard'),
        ('Ice Dragon', 'monster', 'Water', 90, 1, 'Dragon'),
        ('Cursed Knight', 'monster', 'Normal', 75, 1, 'Knight'),
        ('Kraken of the Abyss', 'monster', 'Water', 100, 1, 'Krake'),
        ('Elder Fire Elf', 'monster', 'Fire', 85, 1, 'FireElve'),
        ('Dread Orc', 'monster', 'Normal', 75, 1, 'Ork'),
        ('Fireball', 'spell', 'Fire', 10, 1, NULL),
        ('Ice Shard', 'spell', 'Water', 15, 1, NULL),
        ('Lightning Bolt', 'spell', 'Normal', 20, 1, NULL),
        ('Healing Light', 'spell', 'Normal', 5, 1, NULL),
        ('Meteor Shower 1', 'spell', 'Fire', 25, 1, NULL),
        ('Water Wave', 'spell', 'Water', 18, 6, NULL),
        ('Stone Skin', 'spell', 'Normal', 12, 1, NULL),
        ('Firestorm', 'spell', 'Fire', 30, 4, NULL),
        ('Tsunami', 'spell', 'Water', 22, 1, NULL),
        ('Frostbite', 'spell', 'Water', 15, 1, NULL),
        ('Shadow Strike', 'spell', 'Normal', 25, 1, NULL),
        ('Whirlwind', 'spell', 'Normal', 18, 3, NULL),
        ('Meteor Strike', 'spell', 'Fire', 30, 1, NULL),
        ('Torrent', 'spell', 'Water', 20, 1, NULL),
        ('Flame Wave', 'spell', 'Fire', 24, 1, NULL),
        ('Normal Spell', 'spell', 'Normal', 10, 1, NULL),
        ('Holy Light', 'spell', 'Normal', 15, 1, NULL),
        ('Ice Shield', 'spell', 'Water', 12, 2, NULL),
        ('Wind Blast', 'spell', 'Normal', 20, 1, NULL),
        ('Earthquake', 'spell', 'Normal', 30, 1, NULL),
        ('Steam Blast', 'spell', 'Water', 22, 2, NULL),
        ('Fire Shield', 'spell', 'Fire', 15, 1, NULL),
        ('Water Shield', 'spell', 'Water', 18, 1, NULL),
        ('Burning Hands', 'spell', 'Fire', 10, 2, NULL),
        ('Water Whip', 'spell', 'Water', 15, 1, NULL),
        ('Sandstorm', 'spell', 'Normal', 20, 2, NULL),
        ('Inferno', 'spell', 'Fire', 35, 1, NULL),
        ('Deluge', 'spell', 'Water', 25, 1, NULL),
        ('Meteor 1', 'spell', 'Fire', 30, 1, NULL),
        ('Shadow Bolt', 'spell', 'Normal', 20, 1, NULL),
        ('Frost Nova', 'spell', 'Water', 25, 7, NULL),
        ('Fire Rain', 'spell', 'Fire', 28, 1, NULL),
        ('Water Spout', 'spell', 'Water', 30, 1, NULL),
        ('Soul Drain', 'spell', 'Normal', 12, 1, NULL),
        ('Healing Rain', 'spell', 'Water', 15, 1, NULL),
        ('Vortex', 'spell', 'Normal', 25, 1, NULL),
        ('Fire Wave', 'spell', 'Fire', 20, 1, NULL),
        ('Icicle', 'spell', 'Water', 18, 3, NULL),
        ('Scorch', 'spell', 'Fire', 12, 1, NULL),
        ('Mend', 'spell', 'Normal', 15, 1, NULL),
        ('Gale Force', 'spell', 'Normal', 20, 1, NULL),
        ('Fire Burst', 'spell', 'Fire', 30, 5, NULL),
        ('Water Surge', 'spell', 'Water', 28, 1, NULL),
        ('Rock Slide', 'spell', 'Normal', 20, 1, NULL),
        ('Wildfire', 'spell', 'Fire', 35, 4, NULL),
        ('Torrent Wave', 'spell', 'Water', 30, 1, NULL);
        ";
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = insertQuery;
                command.ExecuteNonQuery();
            }
        }
    }
}