
using System.Data;
using Npgsql;

namespace MCTG.Data.Repositories;

public class UserRepo : BaseRepo
{
    public async Task<bool> AddUser(User user)
    {
        const string insertQuery = @"
        INSERT INTO Users (username, password, password_salt)
        VALUES (@UserName, @Password, @Salt);
    ";

        var parameters = new Dictionary<string, object>
        {
            { "@UserName", user.UserName },
            { "@Password", user.Password },
            { "@Salt", user.Salt }
        };

        int rowsAffected = await ExecuteNonQueryAsync(insertQuery, parameters);
        return rowsAffected > 0;
    }


    public async Task<bool> UserExists(string username)
    {
        const string searchQuery = "SELECT COUNT(1) FROM users WHERE username = @UserName;";
        long userCount = await ExecuteScalarAsync<long>(searchQuery,new Dictionary<string, object>{{ "userName" , username }});
        return userCount > 0;
    }
    public async Task<User?> GetUser(Guid? userId)
    {
        if (userId == null)
            return null;

        const string searchQuery = "SELECT * FROM Users WHERE id = @userid;";
        User? user = null;

        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            string userName = reader.GetString(reader.GetOrdinal("username"));
            string password = reader.GetString(reader.GetOrdinal("password"));
            byte[] salt = (byte[])reader["password_salt"];
            int coins = reader.GetInt32(reader.GetOrdinal("coins"));
            int elo = reader.GetInt32(reader.GetOrdinal("elo"));
            user = new User(userName, password, salt, coins, elo);

            // Update the Stock and Deck for User
            const string stackQuery = @"
            SELECT c.name, c.type, c.element_type, c.damage, us.quantity, c.monster_type
            FROM userstack AS us
            JOIN cards AS c ON us.card_id = c.id
            WHERE us.user_id = @userID;
        ";
            await GetUserStackOrDeck(user, stackQuery, userId, 'S');

            const string deckQuery = @"
            SELECT c.name, c.type, c.element_type, c.damage, ud.quantity, c.monster_type
            FROM userdeck AS ud
            JOIN userstack AS us ON ud.user_stack_id = us.id
            JOIN cards AS c ON us.card_id = c.id
            WHERE us.user_id = @userID;
        ";
            await GetUserStackOrDeck(user, deckQuery, userId, 'D');
        }, new Dictionary<string, object> { { "@userid", userId } });

        return user;
    }

    
    public async Task<Guid?> GetUserId(string username)
    {
        const string searchQuery = "SELECT id FROM users WHERE username = @UserName;";
        Guid? userId = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userId = reader.GetGuid(reader.GetOrdinal("id"));
        },new Dictionary<string, object> { { "@UserName", username }});
        return userId;
    }

    private async Task GetUserStackOrDeck(User user, string query, Guid? userId, char where)
    {
        if (userId == null) return;
        await ExecuteReaderAsync(query,  async reader =>
        {
            var name = reader.GetString(reader.GetOrdinal("name"));
            var type = reader.GetString(reader.GetOrdinal("type"));
            var elementType = reader.GetString(reader.GetOrdinal("element_type"));
            var damage = reader.GetInt32(reader.GetOrdinal("damage"));
            var quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
            var element = (ElementType)Enum.Parse(typeof(ElementType), elementType);
            Card newCard;
            // Create a monsterCard type or Spell according to the type
            if (type == "monster")
            {
                var monsterType = reader.GetString(reader.GetOrdinal("monster_type"));
                var monster = (TypeOfMonster)Enum.Parse(typeof(TypeOfMonster), monsterType);
                newCard = new MonsterCard(name, damage, element, monster);
            }
            else newCard = new SpellCard(name, damage, element);

            // if a card quantity more than 1
            for (var cnt = 0; cnt < quantity; cnt++)
                if (where == 'S')
                    user.UserStack.AddCardToStack(newCard);
                else
                    user.UserDeck.AddCardToDeck(newCard);
        }, new Dictionary<string, object> {{ "@userID", userId }});
    }

    public async Task UpdateCoins(int coins,string username)
    {
        Guid? userId = await GetUserId(username);
        if (userId == null) return;
        const string updateQuery = "UPDATE users SET coins = coins - @coins WHERE id = @userId";
        await ExecuteNonQueryAsync(updateQuery,new Dictionary<string, object>{{"@coins",coins},{ "@userId", userId }});
    }

    public async Task UpdateElo(int elo, string username)
    {
        Guid? userId = await GetUserId(username);
        if (userId == null) return;
        const string updateQuery = "UPDATE users SET elo = elo + @elo WHERE id = @userId";
        await ExecuteNonQueryAsync(updateQuery,new Dictionary<string, object>{{"@elo",elo},{ "@userId", userId }});    
    }

    public async Task<string> GetUserName(Guid? userId)
    {
        const string searchQuery = "SELECT username FROM users WHERE id = @userId";
        string userName = "";
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userName = reader.GetString(reader.GetOrdinal("username"));
        },new Dictionary<string, object> { { "@userId", userId }});
        return userName;
    }

    public async Task<List<string>> GetUserData(Guid? userId)
    {
        const string searchQuery = "SELECT username,created_at,bio,image FROM users WHERE id = @userId";
        List<string> userData = new();
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userData.Add("Username: " + reader.GetString(reader.GetOrdinal("username")));
            DateTime timestamp = reader.GetDateTime(reader.GetOrdinal("created_at"));
            userData.Add("Date created: " + timestamp.ToString());
            userData.Add("Bio: " + reader.GetString((reader.GetOrdinal("bio"))));
            userData.Add("image: " + reader.GetString(reader.GetOrdinal("image")));
        },new Dictionary<string, object> { { "@userId", userId } });
        return userData;
    }

    public async Task<bool> ChangeUserData(Guid? userId, List<string> userData)
    {
        const string updateQuery = @"UPDATE users 
                                    SET username = @userName,
                                        bio = @bio,
                                        image = @image
                                    WHERE id = @userId";
        var parameters = new Dictionary<string, object>
        {
            { "username", userData[0] },  // First value is username
            { "bio", userData[1] },      // Second value is bio
            { "image", userData[2] },    // Third value is image
            { "userId", userId }         // userId is always passed
        };
        try
        {
            
            await ExecuteNonQueryAsync(updateQuery,parameters);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<List<string>> GetUserStats(Guid? userId)
    {
        const string searchQuery = "SELECT coins,elo FROM users WHERE id = @userId";
        List<string> userStats = new();
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userStats.Add("Coins: " + reader.GetInt32(reader.GetOrdinal("coins")).ToString());
            userStats.Add("Elo: " + reader.GetInt32(reader.GetOrdinal("elo")).ToString());
            
        }, new Dictionary<string, object> { { "@userId", userId } });
        return userStats;
    }
}