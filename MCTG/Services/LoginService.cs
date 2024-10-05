using System.ComponentModel.DataAnnotations;
using MCTG.Data;

namespace MCTG.Services;

public class LoginService
{
    PasswordService PasswordService = new PasswordService();
    public User LoginUser(string username, string password)
    {
        if (Database.UserExists(username))
        {
            User loginUser = Database.getUser(username);
            if (PasswordService.ValidatePassword(loginUser.Password, password,loginUser.Salt))
            {
                Console.WriteLine($"User {username} logged in");
                return loginUser;
                // Generate the token
            }
            else
            {
                throw new ValidationException("Invalid username or password");
            }
        }
        else
        {
            throw new ArgumentException("User does not exist");
        }
    }
}