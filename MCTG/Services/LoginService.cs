using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using MCTG.Data;

namespace MCTG.Services;

public class LoginService
{
    PasswordService PasswordService = new PasswordService();
    public string LoginUser(string username, string password)
    {
        if (Database.UserExists(username))
        {
            User loginUser = Database.GetUser(username);
            if (PasswordService.ValidatePassword(loginUser.Password, password,loginUser.Salt))
            {
                Console.WriteLine($"User {username} logged in");
                loginUser.setToken(GenerateToken((loginUser.UserName)));
                return loginUser.Token;
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
}