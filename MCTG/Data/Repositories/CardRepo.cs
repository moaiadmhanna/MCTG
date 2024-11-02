using Npgsql;

namespace MCTG.Data.Repositories;

public class CardRepo
{
    private readonly string _connectionString;
    private UserRepo _userRepo = new UserRepo();

    public CardRepo()
    {
        _connectionString = DatabaseConf.ConnectionString;
    }

    public async Task<Card> GetRandomCard()
    {
        const string searchQuery = "SELECT * FROM cards WHERE quantity > 0";
        Card newCard = null;
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    int numberOfCards = await GetNumberOfCards();
                    Random random = new Random();
                    int randomCardNumber = random.Next(1, numberOfCards + 1);
                    int cnt = 1;
                    while (await reader.ReadAsync())
                    {
                        if (cnt == randomCardNumber)
                        {
                            var id = reader.GetGuid(reader.GetOrdinal("id"));
                            var name = reader.GetString(reader.GetOrdinal("name"));
                            var type = reader.GetString(reader.GetOrdinal("type"));
                            var elementType = reader.GetString(reader.GetOrdinal("element_type"));
                            var damage = reader.GetInt32(reader.GetOrdinal("damage"));
                            var element = (ElementType)Enum.Parse(typeof(ElementType), elementType);
                            if (type == "monster")
                            {
                                var monsterType = reader.GetString(reader.GetOrdinal("monster_type"));
                                var monster = (TypeOfMonster)Enum.Parse(typeof(TypeOfMonster), monsterType);
                                newCard = new MonsterCard(name, damage, element, monster);
                            }
                            else newCard = new SpellCard(name, damage, element);
                            await UpdateCardQuantity(id);
                            break;
                        }
                        cnt++;
                    }
                }
            }
        }
        return newCard;
    }

    private async Task UpdateCardQuantity(Guid id)
    {
        const string updateQuery = "UPDATE cards SET quantity = quantity - 1 WHERE id = @id";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }
    }
    private async Task DeleteCard(Guid id)
    {
        const string deleteQuery = "DELETE FROM cards WHERE id=@id";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }
    }
    public async Task<int> GetNumberOfCards()
    {
        const string searchQuery = "SELECT COUNT(*) FROM cards WHERE quantity > 0";
        using(NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                var res =  await command.ExecuteScalarAsync();
                int count = Convert.ToInt32(res);
                return count;
            }
        }
    }

    public async Task UpdateUserStackOrDeck(string username, string cardName, string type)
    {
        Guid cardId = await GetCardId(cardName);
        bool cardExist = await CardExistsInUserStack(cardId);
        string updateQuery = $"UPDATE {type} SET quantity = quantity + 1 WHERE card_id = @card_id";
        Guid userId = await _userRepo.GetUserId(username);
        string insertQuery = $"INSERT INTO {type}(user_id,card_id,quantity) VALUES(@user_id,@card_id,@quantity)";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            if (cardExist)
            {
                using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("card_id", cardId);
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("card_id", cardId);
                    command.Parameters.AddWithValue("quantity", 1);
                    command.ExecuteNonQuery();
                }
            }
        }

    }

    private async Task<Guid> GetCardId(string cardName)
    {
        const string searchQuery = "SELECT id FROM cards WHERE name = @name";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@name", cardName);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    return reader.GetGuid(reader.GetOrdinal("id"));
                }
            }
        }
    }

    private async Task<bool> CardExistsInUserStack(Guid id)
    {
        const string searchQuery = "SELECT * FROM userstack WHERE card_id = @id";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("id", id);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return true;
                    }

                    return false;
                }
            }
        }
    }
}