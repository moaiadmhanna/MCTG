using System.Security.Cryptography;
using MCTG.Data;

namespace MCTG.Services;

public class RegisterService
{
    PasswordService PasswordService = new PasswordService();
    RegisterService(string name, string password)
    {
        if (!Database.UserExists(name))
        {
            string hashedPassword = PasswordService.PasswordHash(password);
            User newUser = new User(name, hashedPassword);
            Database.AddUser(newUser);
            Console.WriteLine("User has been added to Database successfully");
        }
        else
        {
            Console.WriteLine("User already exists");
        }
    }
}