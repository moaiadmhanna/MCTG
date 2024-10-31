using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using MCTG.Data;
using MCTG.Data.Repositories;

namespace MCTG.Services;

public class LoginService
{
    private PasswordService PasswordService = new PasswordService();
    private UserRepo _userRepo = new UserRepo();
    private TokenRepo _tokenRepo = new TokenRepo();
    public async Task<string> LoginUser(string username, string password)
    {
        if (await _userRepo.UserExists(username))
        {
            Guid userId = await _userRepo.GetUserId(username);
            User? loginUser = await _userRepo.GetUser(userId);
            
            if (PasswordService.ValidatePassword(loginUser.Password, password,loginUser.Salt))
            {
                Console.WriteLine($"User {username} logged in");
                if (! await _tokenRepo.HasToken(userId))
                {
                    string generatedToken = GenerateToken(loginUser.UserName);
                    await _tokenRepo.AddToken(userId, generatedToken);
                    return generatedToken;
                }
                return await IsLoggedIn(userId); // return the token
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
    public async Task<User?> GetUser(string token)
    {
        Guid? userId = await _tokenRepo.GerUserUid(token);
        if (userId == null)
            return null;
        return await _userRepo.GetUser(userId);
    }
    private async Task<string> IsLoggedIn(Guid userId)
    {
        return await _tokenRepo.GetToken(userId);
    }
}