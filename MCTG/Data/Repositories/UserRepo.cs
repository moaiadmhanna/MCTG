
using System.Data;
using Npgsql;

namespace MCTG.Data.Repositories;

public class UserRepo
{
    private readonly string _connectionString;

    public UserRepo()
    {
        _connectionString = DatabaseConf.ConnectionString;
    }

    public async Task<bool> AddUser(User user)
    {
        const string insertQuery = @"
        INSERT INTO Users (username,password,password_salt)
        VALUES (@UserName,@Password,@Salt);
        ";
        using(NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@UserName", user.UserName);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@Salt", user.Salt);
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    public async Task<bool> UserExists(string username)
    {
        const string searchQuery = "SELECT * FROM users WHERE username = @UserName;";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@UserName", username);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    // Check if there's at least one row returned
                    return await reader.ReadAsync(); // Returns true if there is a row
                }
            }
        }
    }

    public async Task<User?> GetUser(Guid? userId)
    {
        const string searchQuery = "SELECT * FROM Users WHERE id = @userid;";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@userid", userId);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) // Check if a row was returned
                    {
                        string userName = reader.GetString(reader.GetOrdinal("username"));
                        string password = reader.GetString(reader.GetOrdinal("password"));
                        byte[] salt = (byte[])reader["password_salt"];
                        int coins = reader.GetInt32(reader.GetOrdinal("coins"));
                        int elo = reader.GetInt32(reader.GetOrdinal("elo"));
                        User user = new User(userName, password, salt, coins, elo);
                        // Update the Stock und Deck for User
                        const string stackQuery = @"
                        SELECT c.name, c.type, c.element_type, c.damage, us.quantity, c.monster_type
                        FROM userstack AS us
                        JOIN cards AS c ON us.card_id = c.id
                        WHERE us.user_id = @userID;
                        ";
                        await GetUserStackOrDeck(user, stackQuery,'s',userId);
                        const string deckQuery = @"
                        SELECT c.name, c.type, c.element_type, c.damage, ud.quantity, c.monster_type
                        FROM userdeck AS ud
                        JOIN userstack AS us ON ud.user_stack_id = us.id
                        JOIN cards AS c ON us.card_id = c.id
                        WHERE us.user_id = @userID;
                        ";
                        await GetUserStackOrDeck(user, deckQuery,'d',userId);
                        return user;
                    }
                }
            }
        }
        return null;
    }

    public async Task<Guid> GetUserId(string username)
    {
        const string searchQuery = "SELECT id FROM users WHERE username = @UserName;";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@UserName", username);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())  // Check if a row is available
                    {
                        return reader.GetGuid(reader.GetOrdinal("id"));
                    }
                    throw new InvalidOperationException("User not found.");
                }
            }
        }
    }

    private async Task GetUserStackOrDeck(User user, string query, char where, Guid? userId)
    {
        if (userId == null) return;
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userID", userId);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var name = reader.GetString(reader.GetOrdinal("name"));
                        var type = reader.GetString(reader.GetOrdinal("type"));
                        var elementType = reader.GetString(reader.GetOrdinal("element_type"));
                        var damage = reader.GetInt32(reader.GetOrdinal("damage"));
                        var quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
                        var monsterType = reader.GetString(reader.GetOrdinal("monster_type"));
                        var monster = (TypeOfMonster)Enum.Parse(typeof(TypeOfMonster), monsterType);
                        var element = (ElementType)Enum.Parse(typeof(ElementType), elementType);
                        Card newCard;
                        // Create a monsterCard type or Spell according to the type
                        if (type == "monster")
                            newCard = new MonsterCard(name, damage, element, monster);
                        else newCard = new SpellCard(name, damage, element);
                        // if a card quantity more than 1
                        for (var cnt = 0; cnt < quantity; cnt++)
                            if (where == 's')
                                user.UserStack.AddCardToStack(newCard);
                            else
                                user.UserDeck.AddCardToDeck(newCard);
                    }
                }
            }
        }
    }
}