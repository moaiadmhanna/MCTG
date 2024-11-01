using MCTG.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MCTG.Data;
using MCTG.Data.Repositories;
using MCTG.Server;

namespace MCTG;
class Program
{
    static async Task Main(string[] args)
    {
        DatabaseConf databaseConf = new DatabaseConf("localhost", "admin","adminIf23b191","mctg");
        DatabaseInitializer db = new DatabaseInitializer();
        db.InitializeDB();
        HttpServer server = new HttpServer(IPAddress.Any, 10001);
        await server.Listen();
        //Testing the DB Initializer
            // DatabaseInitializer db = new DatabaseInitializer();
            // db.InitializeDB();
        UserRepo userRepo = new UserRepo();
        // Testing the AddUser Method
            // PasswordService passwordService = new PasswordService();
            // byte[] salt = passwordService.GenerateSalt();
            // string hashedPassword = passwordService.PasswordHash("123456Mn",salt);
            // User newUser = new User("Muayad", hashedPassword,salt);
            // await userRepo.AddUser(newUser);
        // Testing the UserExist method
           // Console.WriteLine(await userRepo.UserExists("Muayad") ? "YES" : "NO");
        // Testing the GetUserId method
            // Console.WriteLine(await userRepo.GetUserId("Muayad"));
        // Testing the GetUser method without stack or deck
            //User? user = await userRepo.GetUser("Muayad");
            //Console.WriteLine(user.Password);
    }
}