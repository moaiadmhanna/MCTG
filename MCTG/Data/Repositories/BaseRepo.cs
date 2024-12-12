using Npgsql;

namespace MCTG.Data.Repositories;

public abstract class BaseRepo
{
    private readonly string _connectionString;

    protected BaseRepo()
    {
        _connectionString = DatabaseConf.ConnectionString;
    }
    protected async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        await using var command = await OpenCommandAsync(query);
        AddParameters(command, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    protected async Task<T?> ExecuteScalarAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        await using var command = await OpenCommandAsync(query);
        AddParameters(command, parameters);
        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? default : (T?)result;
    }

    protected async Task ExecuteReaderAsync(string query, Func<NpgsqlDataReader, Task> handleRow, Dictionary<string, object>? parameters = null)
    {
        await using var command = await OpenCommandAsync(query);
        AddParameters(command, parameters);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            await handleRow(reader);
        }
    }

    private static void AddParameters(NpgsqlCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }
    }

    private async Task<NpgsqlCommand> OpenCommandAsync(string query)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new NpgsqlCommand(query, connection);
        return command;
    }
}