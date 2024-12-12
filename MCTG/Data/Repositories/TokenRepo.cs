using Npgsql;

namespace MCTG.Data.Repositories;

public class TokenRepo : BaseRepo
{
    public async Task<bool> HasToken(Guid? userId)
    {
        const string searchQuery = "SELECT COUNT(1) FROM usertokens WHERE user_id = @userId AND expires_at > @currentTime";
        DateTime currentTime = DateTime.UtcNow;
    
        long tokenCount = await ExecuteScalarAsync<long>(searchQuery, new Dictionary<string, object>
        {
            { "@userId", userId },
            { "@currentTime", currentTime }
        });
    
        if (tokenCount > 0)
            return true;
        
        await DeleteToken(userId);
        return false;
    }

    private async Task DeleteToken(Guid? userId)
    {
        const string deleteQuery = "DELETE FROM usertokens WHERE user_id = @userId";
        await ExecuteNonQueryAsync(deleteQuery, new Dictionary<string, object>{{"userId", userId}});
    }

    public async Task AddToken(Guid? userid, string token)
    {
        const string insertQuery = "INSERT INTO usertokens (user_id, token) VALUES (@userid, @token)";
        await ExecuteNonQueryAsync(insertQuery,new Dictionary<string, object>{{"userid", userid}, {"token", token}});
    }

    public async Task<string?> GetToken(Guid? userid)
    {
        const string searchQuery = "SELECT token FROM usertokens WHERE user_id = @userId";
        string? token = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            token = reader.GetString(reader.GetOrdinal("token"));
        },new Dictionary<string, object>{{"userId", userid}});
        return token;
    }

    public async Task<Guid?> GerUserUid(string token)
    {
        const string searchQuery = "SELECT user_id FROM usertokens WHERE token = @token";
        Guid? userUid = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userUid = reader.GetGuid(reader.GetOrdinal("user_id"));
        },new Dictionary<string, object>{{"token", token}});
        return userUid;
    }
}