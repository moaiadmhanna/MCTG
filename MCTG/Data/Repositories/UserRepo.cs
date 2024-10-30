namespace MCTG.Data.Repositories;

public class UserRepo
{
    private readonly string _connectionString;

    public UserRepo()
    {
        _connectionString = DatabaseConf.ConnectionString;
    }
}