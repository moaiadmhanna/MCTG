namespace MCTG.Data.Repositories;

public class TradeRepo : BaseRepo
{
    public async Task<List<string>> GetTrades()
    {
        const string searchQuery = @"
        SELECT 
            t.*, 
            u.username, 
            c.name AS card_name, 
            c.type AS card_type, 
            c.damage AS card_damage
        FROM trades t
        JOIN users u ON t.trader_id = u.id
        JOIN userstack us ON t.offered_card_id = us.id
        JOIN cards c ON us.card_id = c.id;
        ";
        List<string> trades = new();
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            string tradeId = reader.GetGuid(reader.GetOrdinal("id")).ToString();
            string username = reader.GetString(reader.GetOrdinal("username"));
            string cardName = reader.GetString(reader.GetOrdinal("card_name"));
            string cardType = reader.GetString(reader.GetOrdinal("card_type"));
            string cardDamage = reader.GetInt32(reader.GetOrdinal("card_damage")).ToString();
            string desiredCardtype = reader.GetString(reader.GetOrdinal("desired_card_type"));
            string minimumDamageRequired = reader.GetInt32(reader.GetOrdinal("minimum_damage_required")).ToString();
            string result =
                $"TradeId : '{tradeId}' , User : {username}, CardData : [Name: {cardName}, Type : {cardType}, Damage : {cardDamage}], Desired Card Type : '{desiredCardtype}', Desired Card Damage {minimumDamageRequired}";
            trades.Add(result);
        });
        return trades;
    }

    public async Task<bool> CreateTrade(Guid? userId, Trade tradeData)
    {
        const string insertQuery = @"
        INSERT INTO trades (id, trader_id, offered_card_id, desired_card_type, minimum_damage_required)
        VALUES (@Id, @TraderId, @OfferedCardId, @DesiredCardType, @MinimumDamageRequired);";
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Id", tradeData.Id },
                { "@TraderId", userId },
                { "@OfferedCardId", tradeData.CardToTrade },
                { "@DesiredCardType", tradeData.Type },
                { "@MinimumDamageRequired", tradeData.MinimumDamage }
            };
            await ExecuteNonQueryAsync(insertQuery, parameters);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message); // Log error for debugging purposes
            return false;
        }
    }
    public async Task<Guid?> GetUserUid(string tradeId)
    {
        const string searchQuery = "SELECT trader_id FROM trades WHERE id = @tradeId";
        Guid? userUid = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userUid = reader.GetGuid(reader.GetOrdinal("trader_id"));
        },new Dictionary<string, object>{{"tradeId", Guid.Parse(tradeId)}});
        return userUid;
    }

    public async Task<bool> DeleteTrade(string tradeId)
    {
        const string deleteQuery = @"DELETE FROM trades WHERE id = @TradeId;";
        try
        {
            // Prepare parameters for the DELETE query
            var parameters = new Dictionary<string, object>
            {
                { "@TradeId", Guid.Parse(tradeId) } // Convert the tradeId to a Guid
            };
            await ExecuteNonQueryAsync(deleteQuery, parameters);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<Guid?> GetCardId(string tradeId)
    {
        const string searchQuery = "SELECT offered_card_id FROM trades WHERE id = @tradeId";
        Guid? cardId = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            cardId = reader.GetGuid(reader.GetOrdinal("offered_card_id"));
        },new Dictionary<string, object>{{"tradeId", Guid.Parse(tradeId)}});
        return cardId;
    }
    

}