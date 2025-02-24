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
    private StreamReader _reader;
    private StreamWriter _writer;
    private BattleService _battleService;
    private readonly Dictionary<string, Dictionary<string, Func<Task>>> _handlers = new();

    public HandleRequest(StreamReader reader, StreamWriter writer, BattleService battleService)
    {
        _reader = reader;
        _writer = writer;
        _battleService = battleService;
        HandlerInitializer();
    }
    public async Task ProcessRequest()
    {
        string requestLine = await _reader.ReadLineAsync();
        string[] requestParts = requestLine.Split(" ");
        if (requestParts.Length < 3)
        {
            throw new Exception("Invalid request line");
        }
        string method = requestParts[0]; // The HTTP method
        string path = requestParts[1];   // The requested path
        if (_handlers.TryGetValue(method, out var pathHandlers))
        {
            if (pathHandlers.TryGetValue(path, out var handler))
            {
                await handler();
            }
            else if (path.StartsWith("/users"))
            {
                string name = path.Split('/').Last();
                switch (method)
                {
                    case "GET":
                        await HandleDisplayUserData(name);
                        break;
                    case "PUT":
                        await HandleChangeUserData(name);
                        break;
                }
            }
            else if (path.StartsWith("/tradings"))
            {
                string tradeId = path.Split('/').Last();
                switch (method)
                {
                    case "POST":
                        await HandleAcceptTrade(tradeId);
                        break;
                    case "DELETE":
                        await HandleDeleteTrade(tradeId);
                        break;
                }
            }
        }
    }
    private void HandlerInitializer()
    {
        _handlers["GET"] = new Dictionary<string, Func<Task>>
        {
            ["/cards"] = HandleDisplayCardsFromStack,
            ["/deck"] = HandleDisplayCardsFromDeck,
            ["/stats"] = HandleDisplayUserStats,
            ["/scoreboard"] = HandleDisplayScoreboard,
            ["/tradings"] = HandleDisplayTrades
        };

        _handlers["POST"] = new Dictionary<string, Func<Task>>
        {
            ["/sessions"] = HandleLogin,
            ["/users"] = HandleRegister,
            ["/packages"] = HandleCreatePackage,
            ["/transactions/packages"] = HandleAcquirePackage,
            ["/battles"] = HandleBattle,
            ["/tradings"] = HandleCreateTrade
        };
        _handlers["PUT"] = new Dictionary<string, Func<Task>>
        {
            ["/deck"] = HandleConfigureDeck
        };
        _handlers["DELETE"] = new Dictionary<string, Func<Task>>
        {

        };
    }

    #region ReadBody
    private async Task<string> ReadRequestBody()
    {
        string? line;
        StringBuilder body = new StringBuilder();
        int content_length = 0;
        while (!string.IsNullOrWhiteSpace(line = await _reader.ReadLineAsync()))
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
                int bytesRead = await _reader.ReadAsync(chars, bytesReadTotal, bytesToRead);
                if (bytesRead == 0)
                    break;
                bytesReadTotal += bytesRead;
            }
            body.Append(chars, 0, bytesReadTotal);
        }
        return body.ToString();
    }
    private async Task<string?> ReadToken()
    {
        string? line;
        string? token = null;

        while (!string.IsNullOrWhiteSpace(line = await _reader.ReadLineAsync()))
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
    private async Task SendResponse(string status, string body)
    {
        await _writer.WriteLineAsync($"HTTP/1.1 {status}");
        await _writer.WriteLineAsync("Content-Type: text/plain");
        await _writer.WriteLineAsync($"Content-Length: {body.Length}");
        await _writer.WriteLineAsync("Connection: keep-alive"); // Ensure keep-alive is set if desired
        await _writer.WriteLineAsync();
        await _writer.WriteLineAsync(body);
        await _writer.FlushAsync();
    }
    private async Task SendResponseWithJson(string status, string jsonBody)
    {
        await _writer.WriteLineAsync($"HTTP/1.1 {status}");
        await _writer.WriteLineAsync("Content-Type: application/json"); // Set content type to JSON
        await _writer.WriteLineAsync($"Content-Length: {jsonBody.Length}");
        await _writer.WriteLineAsync("Connection: keep-alive");
        await _writer.WriteLineAsync();
        await _writer.WriteLineAsync(jsonBody); // Write the JSON body
        await _writer.FlushAsync();
    }
    #endregion

    #region POST
    private async Task HandleRegister()
    {
        Console.WriteLine("Register request...");
        try
        {
            string requestBody = await ReadRequestBody();
            var registerRequest = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
            if (registerRequest != null)
            {
                // Check for required properties
                if (!registerRequest.TryGetValue("Username", out var usernameValue) || 
                    !registerRequest.TryGetValue("Password", out var passwordValue))
                {
                    await SendResponse("400 Bad Request", "Username and Password are required.");
                    return;
                }
                string username = usernameValue.ToString();
                string password = passwordValue.ToString();
                if(await _registerService.RegisterUser(username, password))
                    await SendResponse("200 OK", "User registered successfully.");
                else
                    await SendResponse("400 Bad Request", "User not registered.");
            }
            else
            {
                await SendResponse("400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleLogin()
    {
        Console.WriteLine("Login request...");
        try
        {
            string requestBody = await ReadRequestBody();
            var loginRequest = JsonSerializer.Deserialize<Dictionary<string,object>>(requestBody);
            if (loginRequest != null)
            {
                if (!loginRequest.TryGetValue("Username", out var usernameValue) || 
                    !loginRequest.TryGetValue("Password", out var passwordValue))
                {
                    await SendResponse("400 Bad Request", "Username and Password are required.");
                    return;
                }
                string username = usernameValue.ToString();
                string password = passwordValue.ToString();
                string? token = await _loginService.LoginUser(username, password);
                if(token != null)
                    await SendResponse("200 OK", $"Login successful. token generated: {token}");
                else 
                    await SendResponse("400 Bad Request", "Invalid username or password.");
            }
            else
            {
                await SendResponse("400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }
    private async Task HandleCreatePackage()
    {
        Console.WriteLine("Package create request...");
        try
        {
            string? adminToken = await ReadToken();
            if(adminToken == null || adminToken != "jzJBvuMWdlOUSw1zBD5oIVIhXyigycAt8rNAQhErOPw=")
                await SendResponse("400 Bad Request", "Invalid admin token.");
            var packageIds = JsonSerializer.Deserialize<List<Guid>>(await ReadRequestBody());
            if (packageIds == null)
            {
                await SendResponse("400 Bad Request", "Invalid request body.");
                return;
            }
            if(await _packageService.CreatePackage(packageIds))
                await SendResponse("201 OK","Package created successfully.");
            else
                await SendResponse("400 Bad Request", "Package could not be created.");
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await SendResponse("400 Bad Request", e.Message);
        }
    }
    private async Task HandleAcquirePackage()
    {
        Console.WriteLine("Package acquire request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                string? packagePurchased = await _packageService.AcquirePackage(token);
                if(packagePurchased == null)
                    await SendResponse("400 Bad Request","Invalid user.");
                else if(packagePurchased == "Package acquired successfully")
                    await SendResponse("200 OK", packagePurchased);
                else
                    await SendResponse("400 Bad Request",packagePurchased);
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }
    private async Task HandleBattle()
    {
        Console.WriteLine("Battle request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                string? battleLog = await _battleService.Matchmaking(token);
                if (battleLog != null)
                {
                    await SendResponse("200 OK", battleLog);
                }
                else
                    await SendResponse("400 Bad Request","Invalid user.");
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleCreateTrade()
    {
        Console.WriteLine("Trade create request...");
        try
        {
            string? token = await ReadToken();
            if(token == null)
                await SendResponse("400 Bad Request", "Invalid admin token.");
            string requestBody = await ReadRequestBody();
            // Deserialize the JSON body into the Trade model
            var changeRequest = JsonSerializer.Deserialize<Trade>(requestBody);
            if (changeRequest != null)
            {
                // Check for required properties
                if (changeRequest.Id == Guid.Empty ||
                    changeRequest.CardToTrade == Guid.Empty ||
                    string.IsNullOrWhiteSpace(changeRequest.Type) ||
                    changeRequest.MinimumDamage <= 0)
                {
                    await SendResponse("400 Bad Request", "Id, CardToTrade, Type, and MinimumDamage are required.");
                    return;
                }
                // Create a Trade instance with the validated data
                Trade tradeData = new Trade
                {
                    Id = changeRequest.Id,
                    CardToTrade = changeRequest.CardToTrade,
                    Type = changeRequest.Type,
                    MinimumDamage = changeRequest.MinimumDamage
                };
                bool? tradeCreated = await _userService.CreateTrade(token, tradeData);
                if(tradeCreated == null)
                    await SendResponse("400 Bad Request", "Invalid user.");
                else if(tradeCreated == false)
                    await SendResponse("400 Bad Request", "Error by creating the trade.");
                else
                {
                    await SendResponse("200 OK", "trade created successfully.");
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Invalid request format.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await SendResponse("400 Bad Request", e.Message);
        }
    }

    public async Task HandleAcceptTrade(string tradeId)
    {
        Console.WriteLine("Accept trade request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                var cardId = JsonSerializer.Deserialize<string>(await ReadRequestBody());
                if (cardId == null)
                {
                    await SendResponse("400 Bad Request", "Invalid request body.");
                    return;
                }
                bool? tradeAccepted = await _userService.AcceptTrade(token,tradeId,cardId);
                if(tradeAccepted == null)
                    await SendResponse("400 Bad Request","Invalid user.");
                else if(tradeAccepted == true)
                    await SendResponse("200 OK", "Trade accepted successfully.");
                else
                    await SendResponse("400 Bad Request","Error by Accepting the trade.");
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }
    #endregion

    #region GET

    private async Task HandleDisplayCardsFromStack()
    {
        Console.WriteLine("Cards display request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<Card>? cards = await _userService.ShowCards(token,"Stack");
                if(cards == null)
                    await SendResponse("400 Bad Request","Invalid user.");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(cards, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson("200 OK", json);
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleDisplayCardsFromDeck()
    {
        Console.WriteLine("Deck display request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<Card>? cards = await _userService.ShowCards(token,"Deck");
                if(cards == null)
                    await SendResponse("400 Bad Request","Invalid User. ");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(cards, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson("200 OK", json);
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleDisplayUserData(string name)
    {
        Console.WriteLine("User Data display request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<string>? userData = await _userService.ShowUserData(token,name);
                if(userData == null)
                    await SendResponse("400 Bad Request","Invalid User. ");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(userData, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson("200 OK", json);
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleDisplayUserStats()
    {
        Console.WriteLine("User stats display request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<string>? userStats = await _userService.ShowUserStats(token);
                if(userStats == null)
                    await SendResponse("400 Bad Request","Invalid User. ");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(userStats, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson("200 OK", json);
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleDisplayScoreboard()
    {
        Console.WriteLine("Scoreboard display request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<string>? scoreboard = await _userService.ShowScoreboard(token);
                if(scoreboard == null)
                    await SendResponse("400 Bad Request","Invalid User. ");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(scoreboard, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson("200 OK", json);
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleDisplayTrades()
    {
        Console.WriteLine("Trades display request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<string>? trades = await _userService.ShowTrades(token);
                if(trades == null)
                    await SendResponse("400 Bad Request","Invalid User. ");
                else
                {
                    // Serialize the cards list to JSON
                    string json = JsonSerializer.Serialize(trades, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    // Send JSON response
                    await SendResponseWithJson("200 OK", json);
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }
    #endregion

    #region PUT
    
    private async Task HandleConfigureDeck()
    {
        Console.WriteLine("Deck configure request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                List<Guid>? stackIds = JsonSerializer.Deserialize<List<Guid>>(await ReadRequestBody());
                if (stackIds == null)
                {
                    await SendResponse("400 Bad Request", "Invalid request body.");
                    return;
                }
                bool? deckConfigured = await _userService.ConfigureDeck(stackIds, token);
                if(deckConfigured == null)
                    await SendResponse("400 Bad Request", "Invalid user.");
                else if(deckConfigured == false)
                    await SendResponse("400 Bad Request", "Error by Configuring the Deck.");
                else
                {
                    await SendResponse("200 OK", "Deck configured successfully.");
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            await SendResponse("400 Bad Request", ex.Message);
        }
    }

    private async Task HandleChangeUserData(string name)
    {
        Console.WriteLine("User Data changing request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                string requestBody = await ReadRequestBody();
                var changeRequest = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);
                if (changeRequest != null)
                {
                    List<string> userData = new();
                    // Check for required properties
                    if (!changeRequest.TryGetValue("Name", out var nameValue) ||
                        !changeRequest.TryGetValue("Bio", out var bioValue) ||
                        !changeRequest.TryGetValue("Image", out var imageValue))
                    {
                        await SendResponse("400 Bad Request", "Username and Password are required.");
                        return;
                    }
                    userData.Add(nameValue);
                    userData.Add(bioValue);
                    userData.Add(imageValue);
                    bool? userDataChanged = await _userService.ChangeUserData(token, name, userData);
                    if(userDataChanged == null)
                        await SendResponse("400 Bad Request", "Invalid user.");
                    else if(userDataChanged == false)
                        await SendResponse("400 Bad Request", "Error by changing the user data.");
                    else
                    {
                        await SendResponse("200 OK", "User data changed successfully.");
                    }
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }
    #endregion

    #region DELETE

    public async Task HandleDeleteTrade(string tradeId)
    {
        Console.WriteLine("User Data changing request...");
        try
        {
            string? token = await ReadToken();
            if (token != null)
            {
                bool? tradeDeleted = await _userService.DeleteTrade(token, tradeId);
                if(tradeDeleted == null)
                    await SendResponse("400 Bad Request", "Invalid user.");
                else if(tradeDeleted == false)
                    await SendResponse("400 Bad Request", "Error by deleting the trade.");
                else
                {
                    await SendResponse("200 OK", "trade deleted successfully.");
                }
            }
            else
            {
                await SendResponse("400 Bad Request", "Unauthorized.");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex);
            await SendResponse("400 Bad Request", ex.Message);
        }
    }
    #endregion
}