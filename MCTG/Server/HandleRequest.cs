using System.Text;
using System.Text.Json;
using MCTG.Services;

namespace MCTG.Server;
public class HandleRequest
{
    public async Task ProcessRequest(StreamReader reader, StreamWriter writer)
    {
        string requestLine = await reader.ReadLineAsync();
        string[] requestParts = requestLine.Split(" ");
        if (requestParts.Length < 3)
        {
            throw new Exception("Invalid request line");
        }
        string method = requestParts[0]; // The HTTP method
        string path = requestParts[1];   // The requested path
        string requestBody = await ReadRequestBody(reader);
        switch (method)
        {
            //TODO Put Method for the Battle Service
            case "GET":
                break;
            case "POST":
                if (path == "/sessions")
                {
                    await HandleLogin(reader, writer, requestBody);
                }
                else if (path == "/users")
                {
                    await HandleRegister(reader,writer, requestBody);
                }
                else if (path == "/package")
                {
                    await HandlePackage(reader,writer,requestBody);
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
    public async Task HandleRegister(StreamReader reader, StreamWriter writer, string requestBody)
    {
        Console.WriteLine("Register request...");
        try
        {
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
                RegisterService registerService = new RegisterService();
                if(await registerService.RegisterUser(username, password))
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

    public async Task HandleLogin(StreamReader reader, StreamWriter writer, string requestBody)
    {
        Console.WriteLine("Login request...");
        try
        {
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
                LoginService loginService = new LoginService();
                string? token = await loginService.LoginUser(username, password);
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

    public async Task HandlePackage(StreamReader reader, StreamWriter writer,string requestBody)
    {
        Console.WriteLine("Package request...");
        try
        {
            var packageRequest = JsonSerializer.Deserialize<Dictionary<string,object>>(requestBody);
            if (packageRequest != null)
            {
                if (!packageRequest.TryGetValue("Token", out var tokenValue))
                {
                    await SendResponse(writer, "400 Bad Request", "Token is required.");
                    return;
                }
                string token = tokenValue.ToString();
                PackageService packageService = new PackageService();
                LoginService loginService = new LoginService();
                User? user = await loginService.GetUser(token);
                if (user == null)
                {
                    await SendResponse(writer,"400 Bad Request","There is no valid User");
                    return;
                }
                if(await packageService.PurchasePackage(user))
                    await SendResponse(writer, "200 OK", "Package purchased successfully.");
                else
                    await SendResponse(writer,"400 Bad Request","Package not purchased.");
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
}