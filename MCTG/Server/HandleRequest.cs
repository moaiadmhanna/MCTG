using System.Text;
using System.Text.Json;
using MCTG.Services;

namespace MCTG.Server;
public class HandleRequest
{
    private readonly LoginService _loginService = new LoginService();
    private readonly RegisterService _registerService = new RegisterService();
    private readonly PackageService _packageService = new PackageService();
    private BattleService _battleService;
    public async Task ProcessRequest(StreamReader reader, StreamWriter writer,BattleService battleService)
    {
        _battleService = battleService;
        string requestLine = await reader.ReadLineAsync();
        string[] requestParts = requestLine.Split(" ");
        if (requestParts.Length < 3)
        {
            throw new Exception("Invalid request line");
        }
        string method = requestParts[0]; // The HTTP method
        string path = requestParts[1];   // The requested path
        switch (method)
        {
            //TODO Put Method for the Battle Service
            case "GET":
                break;
            case "POST":
                if (path == "/sessions")
                {
                    await HandleLogin(reader, writer);
                }
                else if (path == "/users")
                {
                    await HandleRegister(reader,writer);
                }
                else if (path == "/package")
                {
                    await HandlePackage(reader,writer);
                }
                else if (path == "/battles")
                {
                    await HandleBattle(reader, writer);
                }
                break;
        }
    }
    

    public async Task<string> ReadRequestBody(StreamReader reader)
    {
        string? line;
        StringBuilder body = new StringBuilder();
        int content_length = 0;
        while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
        {
            if (line == "")
            {
                break;  // empty line indicates the end of the HTTP-headers
            }
            // Parse the header
            string[] parts = line.Split(':');
            if (parts.Length == 2 && parts[0] == "Content-Length")
            {
                content_length = int.Parse(parts[1].Trim());
            }
        }
        if ( content_length > 0 )
        {
            char[] chars = new char[content_length];
            int bytesReadTotal = 0;
            while ( bytesReadTotal < content_length)
            {
                int bytesToRead = content_length - bytesReadTotal;
                int bytesRead = await reader.ReadAsync(chars, bytesReadTotal, bytesToRead);
                if (bytesRead == 0)
                    break;
                bytesReadTotal += bytesRead;
            }
            body.Append(chars, 0, bytesReadTotal);
        }
        return body.ToString();
    }
    public async Task<string?> ReadToken(StreamReader reader)
    {
        string? line;
        string? token = null;

        while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
        {
            // Parse the header
            if (line.StartsWith("Authorization:", StringComparison.OrdinalIgnoreCase))
            {
                // Split on the space to get the token after "Bearer "
                var parts = line.Split(' ', 2);
                if (parts.Length == 2)
                {
                    parts = parts[1].Split(' ', 2);
                    if(parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                        token = parts[1]; // Get the token
                }
            }
        }
        return token; // Return the extracted token
    }

    public async Task SendResponse(StreamWriter writer, string status, string body)
    {
        await writer.WriteLineAsync($"HTTP/1.1 {status}");
        await writer.WriteLineAsync("Content-Type: text/plain");
        await writer.WriteLineAsync($"Content-Length: {body.Length}");
        await writer.WriteLineAsync("Connection: keep-alive"); // Ensure keep-alive is set if desired
        await writer.WriteLineAsync();
        await writer.WriteLineAsync(body);
        await writer.FlushAsync();
    }
    public async Task HandleRegister(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Register request...");
        try
        {
            string requestBody = await ReadRequestBody(reader);
            var registerRequest = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
            if (registerRequest != null)
            {
                // Check for required properties
                if (!registerRequest.TryGetValue("Username", out var usernameValue) || 
                    !registerRequest.TryGetValue("Password", out var passwordValue))
                {
                    await SendResponse(writer, "400 Bad Request", "Username and Password are required.");
                    return;
                }
                string username = usernameValue.ToString();
                string password = passwordValue.ToString();
                if(await _registerService.RegisterUser(username, password))
                    await SendResponse(writer, "200 OK", "User registered successfully.");
                else
                    await SendResponse(writer, "400 Bad Request", "User not registered.");
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }

    public async Task HandleLogin(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Login request...");
        try
        {
            string requestBody = await ReadRequestBody(reader);
            var loginRequest = JsonSerializer.Deserialize<Dictionary<string,object>>(requestBody);
            if (loginRequest != null)
            {
                if (!loginRequest.TryGetValue("Username", out var usernameValue) || 
                    !loginRequest.TryGetValue("Password", out var passwordValue))
                {
                    await SendResponse(writer, "400 Bad Request", "Username and Password are required.");
                    return;
                }
                string username = usernameValue.ToString();
                string password = passwordValue.ToString();
                string? token = await _loginService.LoginUser(username, password);
                if(token != null)
                    await SendResponse(writer, "200 OK", $"Login successful. token generated: {token}");
                else 
                    await SendResponse(writer,"400 Bad Request", "Invalid username or password.");
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }

    public async Task HandlePackage(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Package request...");
        try
        {
            string? token = await ReadToken(reader);
            if (token != null)
            {
                // TODO Should be in the Package Service
                User? user = await _loginService.GetUser(token);
                if (user == null)
                {
                    await SendResponse(writer,"400 Bad Request","There is no valid User");
                    return;
                }
                if(await _packageService.PurchasePackage(user))
                    await SendResponse(writer, "200 OK", "Package purchased successfully.");
                else
                    await SendResponse(writer,"400 Bad Request","Package not purchased.");
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Invalid Token.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }
    private async Task HandleBattle(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Battle request...");
        try
        {
            string? token = await ReadToken(reader);
            if (token != null)
            {
                string? battleLog = await _battleService.Matchmaking(token);
                if (battleLog != null)
                {
                    await SendResponse(writer, "200 OK", battleLog);
                    Console.WriteLine(battleLog);
                }
                else
                    await SendResponse(writer,"400 Bad Request","Battle cannot be started.");
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Invalid Token.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }
}