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
                    name VARCHAR(100) NOT NULL,
                    type VARCHAR(10) CHECK (type IN ('monster', 'spell')) NOT NULL,
                    element_type VARCHAR(10) CHECK (element_type IN ('fire', 'water', 'normal')) NOT NULL,
                    damage INT NOT NULL,
                    quantity INT NOT NULL CHECK (quantity > 0),
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
            }
        }
    }
}