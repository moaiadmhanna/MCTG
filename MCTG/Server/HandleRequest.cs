using System.Text;
using System.Text.Json;
using MCTG.Services;

namespace MCTG.Server;
public class HandleRequest
{
    private readonly LoginService _loginService = new LoginService();
    private readonly RegisterService _registerService = new RegisterService();
    private readonly PackageService _packageService = new PackageService();
    private readonly UserService _userService = new UserService();
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
            case "GET":
                if (path == "/cards")
                {
                    await HandleDisplayCards(reader, writer);
                }
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
                else if (path == "/packages")
                {
                    await HandleCreatePackage(reader,writer);
                }
                else if (path == "/transactions/package")
                {
                    await HandleAcquirePackage(reader,writer);
                }
                else if (path == "/battles")
                {
                    await HandleBattle(reader, writer);
                }
                break;
        }
    }
    #region ReadBody
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
                    break;
                }
            }
        }
        return token; // Return the extracted token
    }
    #endregion
    
    #region SendResponse
    private async Task SendResponse(StreamWriter writer, string status, string body)
    {
        await writer.WriteLineAsync($"HTTP/1.1 {status}");
        await writer.WriteLineAsync("Content-Type: text/plain");
        await writer.WriteLineAsync($"Content-Length: {body.Length}");
        await writer.WriteLineAsync("Connection: keep-alive"); // Ensure keep-alive is set if desired
        await writer.WriteLineAsync();
        await writer.WriteLineAsync(body);
        await writer.FlushAsync();
    }
    private async Task SendResponseWithJson(StreamWriter writer, string status, string jsonBody)
    {
        await writer.WriteLineAsync($"HTTP/1.1 {status}");
        await writer.WriteLineAsync("Content-Type: application/json"); // Set content type to JSON
        await writer.WriteLineAsync($"Content-Length: {jsonBody.Length}");
        await writer.WriteLineAsync("Connection: keep-alive");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync(jsonBody); // Write the JSON body
        await writer.FlushAsync();
    }
    #endregion

    #region POST
    private async Task HandleRegister(StreamReader reader, StreamWriter writer)
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

    private async Task HandleLogin(StreamReader reader, StreamWriter writer)
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
    private async Task HandleCreatePackage(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Package create request...");
        try
        {
            string? adminToken = await ReadToken(reader);
            if(adminToken == null)
                await SendResponse(writer,"400 Bad Request", "Invalid admin token.");
            var packageIds = JsonSerializer.Deserialize<List<Guid>>(await ReadRequestBody(reader));
            if (packageIds == null)
            {
                await SendResponse(writer, "400 Bad Request", "Invalid request body.");
                return;
            }
            if(await _packageService.CreatePackage(packageIds))
                await SendResponse(writer,"200 OK","Package created successfully.");
            else
                await SendResponse(writer, "400 Bad Request", "Package could not be created.");
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await SendResponse(writer, "400 Bad Request", e.Message);
        }
    }
    private async Task HandleAcquirePackage(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Package acquire request...");
        try
        {
            string? token = await ReadToken(reader);
            if (token != null)
            {
                bool? packagePurchased = await _packageService.AcquirePackage(token);
                if(packagePurchased == null)
                    await SendResponse(writer,"400 Bad Request","Invalid user.");
                else if(packagePurchased == true)
                    await SendResponse(writer, "200 OK", "Package acquired successfully.");
                else
                    await SendResponse(writer,"400 Bad Request","Not enough Money.");
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Unauthorized.");
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
                    await SendResponse(writer,"400 Bad Request","Invalid user.");
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }
    #endregion

    #region GET

    private async Task HandleDisplayCards(StreamReader reader, StreamWriter writer)
    {
        Console.WriteLine("Cards display request...");
        try
        {
            string? token = await ReadToken(reader);
            if (token != null)
            {
                List<Card>? cards = await _userService.ShowCards(token);
                if(cards == null)
                    await SendResponse(writer,"400 Bad Request","Invalid user.");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(cards, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson(writer, "200 OK", json);
                }
            }
            else
            {
                await SendResponse(writer, "400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }
    #endregion
}