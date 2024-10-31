using System.Security.Cryptography;
using MCTG.Data;
using MCTG.Data.Repositories;

namespace MCTG.Services;

public class RegisterService
{
    private PasswordService PasswordService = new PasswordService();
    private UserRepo _userRepo = new UserRepo();
    public async Task<bool> RegisterUser(string name, string password)
    {
        if (!await _userRepo.UserExists(name))
        {
            byte[] salt = PasswordService.GenerateSalt();
            string hashedPassword = PasswordService.PasswordHash(password,salt);
            User newUser = new User(name, hashedPassword,salt);
            await _userRepo.AddUser(newUser);
            Console.WriteLine("User has been added to Database successfully");
            return true;
        }
        Console.WriteLine("User already exists");
        return false;
    }
}