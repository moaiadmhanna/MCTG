using Npgsql;

namespace MCTG.Data.Repositories;
public interface ITokenRepo
{
    Task<bool> HasToken(Guid? userId);
    Task AddToken(Guid? userid, string token);
    Task<string?> GetToken(Guid? userid);
    Task<Guid?> GetUserUid(string token);
}
public class TokenRepo : BaseRepo,ITokenRepo
{
    public async Task<bool> HasToken(Guid? userId)
    {
        if (userId == null)
            return false;
        const string searchQuery = "SELECT COUNT(1) FROM usertokens WHERE user_id = @userId";
        DateTime currentTime = DateTime.UtcNow;
    
        long tokenCount = await ExecuteScalarAsync<long>(searchQuery, new Dictionary<string, object>
        {
            { "@userId", userId }
        });
    
        if (tokenCount > 0)
            return true;
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

    public async Task<Guid?> GetUserUid(string token)
    {
        const string searchQuery = "SELECT user_id FROM usertokens WHERE token = @token";
        Guid? userUid = null;
        await ExecuteReaderAsync(searchQuery, async reader =>
        {
            userUid = reader.GetGuid(reader.GetOrdinal("user_id"));
        },new Dictionary<string, object>{{"token", token}});
        if(await HasToken(userUid))
            return userUid;
        return null;
    }
}