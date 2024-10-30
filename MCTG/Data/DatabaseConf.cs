namespace MCTG.Data;

public class DatabaseConf
{
    public static string ConnectionString { get; private set; }

    public DatabaseConf(string host, string username, string password, string database)
    {
        ConnectionString = $"Host={host};Username={username};Password={password};Database={database}";
    }
}