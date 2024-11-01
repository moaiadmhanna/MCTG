using Npgsql;

namespace MCTG.Data.Repositories;

public class TokenRepo
{
    private readonly string _connectionString;

    public TokenRepo()
    {
        _connectionString = DatabaseConf.ConnectionString;
    }

    public async Task<bool> HasToken(Guid userid)
    {
        const string searchQuery = "SELECT * FROM usertokens WHERE user_id = @userId";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@userid", userid);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    // Check if there's at least one row returned
                    if (await reader.ReadAsync())
                    {
                        DateTime currentTime = DateTime.UtcNow;
                        DateTime expiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at"));
                        if(expiresAt > currentTime)
                            return true;
                        int tokenId = reader.GetInt32(reader.GetOrdinal("id"));
                        await DeleteToken(tokenId);
                    }
                    return false;
                }
            }
        }
    }

    private async Task DeleteToken(int tokenid)
    {
        const string deleteQuery = "DELETE FROM usertokens WHERE id = @tokenId";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using(NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@tokenId", tokenid);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task AddToken(Guid userid, string token)
    {
        const string insertQuery = "INSERT INTO usertokens (user_id, token) VALUES (@userid, @token)";
        using(NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@userid", userid);
                command.Parameters.AddWithValue("@token", token);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<string> GetToken(Guid userid)
    {
        const string searchQuery = "SELECT token FROM usertokens WHERE user_id = @userId";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@userId", userid);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    return reader.GetString(reader.GetOrdinal("token"));
                }
            }
        }
    }

    public async Task<Guid?> GerUserUid(string token)
    {
        const string searchQuery = "SELECT user_id FROM usertokens WHERE token = @token";
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (NpgsqlCommand command = new NpgsqlCommand(searchQuery, connection))
            {
                command.Parameters.AddWithValue("@token", token);
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return reader.GetGuid(reader.GetOrdinal("user_id"));
                    }
                    return null;
                }
            }
        }
    }
}