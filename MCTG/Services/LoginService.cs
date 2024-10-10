using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using MCTG.Data;

namespace MCTG.Services;

public class LoginService
{
   // Dictonary<token,user>
    private static Dictionary<string,User> _tokens { get; set; } = new Dictionary<string, User>();
    PasswordService PasswordService = new PasswordService();
    public string LoginUser(string username, string password)
    {
        if (Database.UserExists(username))
        {
            User loginUser = Database.GetUser(username);
            
            if (PasswordService.ValidatePassword(loginUser.Password, password,loginUser.Salt))
            {
                Console.WriteLine($"User {username} logged in");
                if (!_tokens.ContainsValue(loginUser))
                {
                    string generatedToken = GenerateToken(loginUser.UserName);
                    _tokens.Add(generatedToken, loginUser);
                    return generatedToken;
                }
                return IsLoggedIn(loginUser); // return the token
            }
            throw new ValidationException("Invalid username or password");
        }
        throw new ArgumentException("User does not exist");
    }
    private string GenerateToken(string username)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(username + DateTime.UtcNow));
            return Convert.ToBase64String(hash);
        }
    }
    public User GetUser(string token)
    {
        return _tokens[token];
    }
    private string IsLoggedIn(User user)
    {
        return _tokens.FirstOrDefault(x => x.Value == user).Key;
    }
}