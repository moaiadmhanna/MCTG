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
    
    public async Task UpdateUserStackOrDeck(string username, string cardName, string type)
    {
        Guid? cardId = await GetCardId(cardName);
        bool cardExist = await CardExistsInUserStack(cardId);
        string updateQuery = $"UPDATE {type} SET quantity = quantity + 1 WHERE card_id = @card_id";
        Guid? userId = await _userRepo.GetUserId(username);
        if(userId == null)
            return;
        string insertQuery = $"INSERT INTO {type}(user_id,card_id,quantity) VALUES(@user_id,@card_id,@quantity)";
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
}