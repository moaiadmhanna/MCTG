using System.Security.Cryptography;
using MCTG.Data;

namespace MCTG.Services;

public class RegisterService
{
    PasswordService PasswordService = new PasswordService();
    public bool RegisterUser(string name, string password)
    {
        if (!Database.UserExists(name))
        {
            byte[] salt = PasswordService.GenerateSalt();
            string hashedPassword = PasswordService.PasswordHash(password,salt);
            User newUser = new User(name, hashedPassword,salt);
            Database.AddUser(newUser);
            Console.WriteLine("User has been added to Database successfully");
            return true;
        }
        Console.WriteLine("User already exists");
        return false;
    }
}