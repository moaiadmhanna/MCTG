using System.Text;
using System.Text.Json;
using MCTG.Services;

namespace MCTG.Server;
public class HandleRequest
{
    public void ProcessRequest(StreamReader reader, StreamWriter writer)
    {
        string requestLine = reader.ReadLine();
        string[] requestParts = requestLine.Split(" ");
        if (requestParts.Length < 3)
        {
            throw new Exception("Invalid request line");
        }
        string method = requestParts[0]; // The HTTP method
        string path = requestParts[1];   // The requested path
        string requestBody = ReadRequestBody(reader);
        switch (method)
        {
            case "GET":
                if (path == "/package")
                {
                    HandlePackage(reader,writer,requestBody);
                }
                break;
            case "POST":
                if (path == "/sessions")
                {
                    HandleLogin(reader, writer, requestBody);
                }
                else if (path == "/register")
                {
                    HandleRegister(reader,writer, requestBody);
                }
                break;
        }
    }
    public string ReadRequestBody(StreamReader reader)
    {
        string? line;
        StringBuilder body = new StringBuilder();
        int content_length = 0;
        while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
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
                int bytesRead = reader.Read(chars, bytesReadTotal, bytesToRead);
                if (bytesRead == 0)
                    break;
                bytesReadTotal += bytesRead;
            }
            body.Append(chars, 0, bytesReadTotal);
        }
        return body.ToString();
    }

    public void SendResponse(StreamWriter writer, string status, string body)
    {
        writer.WriteLine($"HTTP/1.1 {status}");
        writer.WriteLine("Content-Type: text/plain");
        writer.WriteLine($"Content-Length: {body.Length}");
        writer.WriteLine();
        writer.WriteLine(body);
    }
    public void HandleRegister(StreamReader reader, StreamWriter writer, string requestBody)
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
                    SendResponse(writer, "400 Bad Request", "Username and Password are required.");
                    return;
                }
                string username = usernameValue.ToString();
                string password = passwordValue.ToString();
                RegisterService registerService = new RegisterService();
                if(registerService.RegisterUser(username, password))
                    SendResponse(writer, "200 OK", "User registered successfully.");
                else
                    SendResponse(writer, "400 Bad Request", "User not registered.");
            }
            else
            {
                SendResponse(writer, "400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }

    public void HandleLogin(StreamReader reader, StreamWriter writer, string requestBody)
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
                    SendResponse(writer, "400 Bad Request", "Username and Password are required.");
                    return;
                }
                string username = usernameValue.ToString();
                string password = passwordValue.ToString();
                LoginService loginService = new LoginService();
                string token = loginService.LoginUser(username, password);
                SendResponse(writer, "200 OK", "Login successful. token generated: " + token);
            }
            else
            {
                SendResponse(writer, "400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }

    public void HandlePackage(StreamReader reader, StreamWriter writer,string requestBody)
    {
        Console.WriteLine("Package request...");
        try
        {
            var packageRequest = JsonSerializer.Deserialize<Dictionary<string,object>>(requestBody);
            if (packageRequest != null)
            {
                if (!packageRequest.TryGetValue("Username", out var usernameValue))
                {
                    SendResponse(writer, "400 Bad Request", "Username is required.");
                    return;
                }
                string username = usernameValue.ToString();
                PackageService packageService = new PackageService();
                packageService.PurchasePackage(username);
                SendResponse(writer, "200 OK", "Package purchased successfully.");
            }
            else
            {
                SendResponse(writer, "400 Bad Request", "Invalid JSON format.");
            }
        }
        catch (JsonException ex)
        {
            SendResponse(writer, "400 Bad Request", ex.Message);
        }
    }
}