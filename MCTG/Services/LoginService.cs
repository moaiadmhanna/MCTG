using MCTG.Data;

namespace MCTG.Services;

public class LoginService
{
    PasswordService PasswordService = new PasswordService();
    LoginService(string username, string password)
    {
        if (Database.UserExists(username) &&
            PasswordService.ValidatePassword(Database.getUser(username).Password, password))
        {
            Console.WriteLine($"User {username} logged in");
            // Generate the token
        }
        else
        {
            Console.WriteLine("Invalid username or password");
        }
    }
}