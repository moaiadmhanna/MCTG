using Npgsql;

namespace MCTG.Data.Repositories;

public class CardRepo : BaseRepo
{
    private UserRepo _userRepo = new UserRepo();
    public async Task<Card?> GetRandomCard()
    {
        const string searchQuery = "SELECT * FROM cards";
        long numberOfCards = await GetNumberOfCards();
        if (numberOfCards == 0)
            return null;

        Random random = new Random();
        int randomCardNumber = random.Next(1, (int)numberOfCards + 1);

        Card? selectedCard = null;
        int cnt = 1;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            if (cnt == randomCardNumber)
            {
                selectedCard = CardFromReader(reader);
                var id = reader.GetGuid(reader.GetOrdinal("id"));
                int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
                if (quantity - 1 >= 0)
                    await UpdateCardQuantity(id);
            }
            cnt++;
        });
        return selectedCard;
    }

    private async Task UpdateCardQuantity(Guid id)
    {
        const string updateQuery = "UPDATE cards SET quantity = quantity - 1 WHERE id = @id";
        await ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object> { { "@id", id } });
    }
    private async Task DeleteCard(Guid id)
    {
        const string deleteQuery = "DELETE FROM cards WHERE id=@id";
        await ExecuteNonQueryAsync(deleteQuery, new Dictionary<string, object> { { "@id", id } });
    }
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