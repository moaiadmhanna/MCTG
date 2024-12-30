using Npgsql;

namespace MCTG.Data.Repositories;

public class CardRepo : BaseRepo
{
    private UserRepo _userRepo = new UserRepo();
    private Random _random = new Random();
    public async Task<long> GetNumberOfCards()
    {
        const string searchQuery = "SELECT COUNT(*) FROM cards WHERE quantity > 0";
        return await ExecuteScalarAsync<long>(searchQuery);
    }
    
    public async Task UpdateUserStack(string username, string cardName)
    {
        Guid? cardId = await GetCardId(cardName);
        bool cardExist = await CardExistsInUserStack(cardId);
        string updateQuery = $"UPDATE userstack SET quantity = quantity + 1 WHERE card_id = @card_id";
        Guid? userId = await _userRepo.GetUserId(username);
        if(userId == null)
            return;
        string insertQuery = $"INSERT INTO userstack (user_id,card_id,quantity) VALUES(@user_id,@card_id,@quantity)";
        if (cardExist)
            await ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object> { { "@card_id", cardId } });
        else
            await ExecuteNonQueryAsync(insertQuery, new Dictionary<string, object> { {"user_id",userId},{ "@card_id", cardId }, {"quantity", 1}});
    }

    public async Task AddCardToPackages(Guid packageId, Guid cardId)
    {
        string insertQuery = $"INSERT INTO packages(package_id, card_id) VALUES(@packageId, @cardId)";
        await ExecuteNonQueryAsync(insertQuery,new Dictionary<string, object>{{"packageId",packageId},{"cardId",cardId}});
        // Update the Quantity of the card in cards table
        await UpdateCardQuantity(cardId);
    }
    public async Task<List<Card>?> GetAllCardsFromPackage()
    {
        Guid? packageId = await GetRandomPackage();
        if (packageId == null)
            return null;
        string searchQuery = "SELECT c.name, c.type, c.element_type, c.damage, c.monster_type FROM packages AS pk JOIN cards AS c ON pk.card_id = c.id WHERE package_id = @packageId";
        // List of cards to save cards to it
        List<Card> cards = new List<Card>();
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            cards.Add(CardFromReader(reader));
            
        },new Dictionary<string, object> { { "@packageId", packageId }});
        await DeletePackage(packageId);
        return cards;
    }
    public async Task<bool> CardExists(Guid cardId)
    {
        string searchQuery = $"SELECT COUNT(1) FROM cards WHERE id = @id AND quantity > 0";
        long count = await ExecuteScalarAsync<long>(searchQuery, new Dictionary<string, object> { { "id", cardId } });
        return count > 0;
    }

    public async Task<List<Card>> GetAllCardsFromStack(Guid? userId)
    {
        string searchQuery = @"SELECT c.name, c.type, c.element_type, c.damage, c.monster_type, us.quantity 
                                FROM userstack AS us JOIN cards AS c ON us.card_id = c.id 
                                WHERE us.user_id = @userId";
        // List of cards to save cards to it
        List<Card> cards = new List<Card>();
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
            for(int i = 0; i < quantity; i++)
                cards.Add(CardFromReader(reader));
        },new Dictionary<string, object> { { "@userId", userId }});
        return cards;
    }

    public async Task<List<Card>> GetAllCardsFromDeck(Guid? userId)
    {
        string searchQuery = @"SELECT c.name, c.type, c.element_type, c.damage, c.monster_type, ud.quantity 
                                FROM userdeck AS ud JOIN userstack AS us ON ud.user_stack_id = us.id
                                JOIN cards AS c ON us.card_id = c.id 
                                WHERE us.user_id = @userId";
        // List of cards to save cards to it
        List<Card> cards = new List<Card>();
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
            for(int i = 0; i < quantity; i++)
                cards.Add(CardFromReader(reader));
        },new Dictionary<string, object> { { "@userId", userId }});
        return cards;
    }

    public async Task<bool> DeckConfigured(Guid? userId)
    {
        // Query to check if the total entries in userdeck for the user equals 4
        string searchQuery = @"
        SELECT COALESCE(SUM(ud.quantity), 0) AS TotalCards
        FROM userdeck ud
        JOIN userstack us ON ud.user_stack_id = us.id
        WHERE us.user_id = @userId";

        // Execute the query and get the total card count
        long totalCards = await ExecuteScalarAsync<long>(searchQuery, new Dictionary<string, object> { { "userId", userId } });
        // Check if the total cards is 4
        return totalCards == 4;
    }

    public async Task<bool> AddCardToDeck(Guid stackId, Guid? userId)
    {
        // Check if the stackId exists with the same userId and if the Card Exist
        string searchQuery = $"SELECT COUNT(1) FROM userstack WHERE id = @stackId AND user_id = @userId AND quantity > 0";
        long count = await ExecuteScalarAsync<long>(searchQuery,new Dictionary<string, object> { { "stackId", stackId }, { "userId", userId } });
        if(count < 1)
            return false;
        // Add the entry to the userdeck table
        string insertQuery = $"INSERT INTO userdeck (user_stack_id, quantity) VALUES(@stackId, @quantity)";
        string updateQuery = $"UPDATE userdeck SET quantity = quantity + 1 WHERE user_stack_id = @stackId";
        // Check if the entry exists in the userdeck table
        bool exists = await ExistInUserDeck(stackId);
        try
        {
            if (!exists)
                await ExecuteNonQueryAsync(insertQuery,
                    new Dictionary<string, object> { { "stackId", stackId }, { "quantity", 1 } });
            else
                await ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object> { { "stackId", stackId } });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    public async Task<bool> DeleteCardFromDeck(Guid stackId)
    {
        string editQuery = @"
        UPDATE userdeck 
        SET quantity = quantity - 1 
        WHERE user_stack_id = @stackId AND quantity > 0;

        DELETE FROM userdeck 
        WHERE user_stack_id = @stackId AND quantity = 0;
    ";

        try
        {
            await ExecuteNonQueryAsync(editQuery, new Dictionary<string, object> { { "stackId", stackId} });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting card from deck: {ex.Message}");
            return false;
        }
    }

    private async Task UpdateCardQuantity(Guid id)
    {
        const string updateQuery = "UPDATE cards SET quantity = quantity - 1 WHERE id = @id";
        await ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object> { { "@id", id } });
    }

    private async Task<Guid?> GetRandomPackage()
    {
        string searchQuery = $"SELECT package_id FROM packages ORDER BY RANDOM() LIMIT 1";
        Guid? packageId = await ExecuteScalarAsync<Guid>(searchQuery);
        return packageId;
    }

    private async Task DeletePackage(Guid? packageID)
    {
        string deleteQuery = $"DELETE FROM packages WHERE package_id = @packageId";
        await ExecuteNonQueryAsync(deleteQuery, new Dictionary<string, object> {{ "@packageId", packageID }});
    }
    private async Task<Guid?> GetCardId(string cardName)
    {
        const string searchQuery = "SELECT id FROM cards WHERE name = @name";
        Guid? id = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            id = reader.GetGuid(reader.GetOrdinal("id"));
        },
            new Dictionary<string, object> {{ "@name", cardName }});
        return id;
    }

    private async Task<bool> CardExistsInUserStack(Guid? id)
    {
        const string searchQuery = "SELECT COUNT(1) FROM userstack WHERE card_id = @id";
        long count = await ExecuteScalarAsync<long>(searchQuery, new Dictionary<string, object> { { "id", id } });
        return count > 0;
    }

    private Card CardFromReader(NpgsqlDataReader reader)
    {
        var name = reader.GetString(reader.GetOrdinal("name"));
        var type = reader.GetString(reader.GetOrdinal("type"));
        var elementType = reader.GetString(reader.GetOrdinal("element_type"));
        var damage = reader.GetInt32(reader.GetOrdinal("damage"));
        var element = (ElementType)Enum.Parse(typeof(ElementType), elementType);
        if (type == "monster")
        {
            var monsterType = reader.GetString(reader.GetOrdinal("monster_type"));
            var monster = (TypeOfMonster)Enum.Parse(typeof(TypeOfMonster), monsterType);
            return new MonsterCard(name, damage, element, monster);
        }
        return new SpellCard(name, damage, element);
    }

    private async Task<bool> ExistInUserDeck(Guid userStackId)
    {
        const string searchQuery = "SELECT COUNT(1) FROM userdeck WHERE user_stack_id = @userStackId";
        long count = await ExecuteScalarAsync<long>(searchQuery, new Dictionary<string, object> { { "userStackId", userStackId } });
        return count > 0;
    }
}