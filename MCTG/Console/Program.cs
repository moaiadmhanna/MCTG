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
        //DatabaseInitializer db = new DatabaseInitializer();
        //db.InitializeDB();
        //HttpServer server = new HttpServer(IPAddress.Any, 10001);
        //await server.Listen();
        //Testing the DB Initializer
            // DatabaseInitializer db = new DatabaseInitializer();
            // db.InitializeDB();
        //UserRepo userRepo = new UserRepo();
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
        // Testing the Battle Service after Modification 
        /*
        BattleService battleService = new BattleService();
        User player1 = new User("Muayad","1156780",new byte[16]);
        User player2 = new User("Mina","1156780",new byte[16]);
        player1.UserDeck.AddCardToDeck(new MonsterCard("Goblin", 40, ElementType.Water, TypeOfMonster.Goblin));
        player1.UserDeck.AddCardToDeck(new MonsterCard("Wizard", 30, ElementType.Fire, TypeOfMonster.Wizzard));
        player1.UserDeck.AddCardToDeck(new MonsterCard("Dragon", 80, ElementType.Fire, TypeOfMonster.Dragon));
        player1.UserDeck.AddCardToDeck(new MonsterCard("Knight", 60, ElementType.Normal, TypeOfMonster.Knight));
        player1.UserDeck.AddCardToDeck(new MonsterCard("Kraken", 70, ElementType.Water, TypeOfMonster.Krake));
        player2.UserDeck.AddCardToDeck(new MonsterCard("Fire Elf", 55, ElementType.Fire, TypeOfMonster.FireElve));
        player2.UserDeck.AddCardToDeck(new MonsterCard("Orc", 50, ElementType.Normal, TypeOfMonster.Ork));
        player2.UserDeck.AddCardToDeck(new MonsterCard("Fire Goblin", 45, ElementType.Fire, TypeOfMonster.Goblin));
        player2.UserDeck.AddCardToDeck(new MonsterCard("Water Wizard", 35, ElementType.Water, TypeOfMonster.Wizzard));
        player2.UserDeck.AddCardToDeck(new MonsterCard("Ice Dragon", 75, ElementType.Water, TypeOfMonster.Dragon));
        battleService.AddPlayerToQueue(player1);
        Console.WriteLine(battleService.Matchmaking());
        battleService.AddPlayerToQueue(player2);
        Console.WriteLine(battleService.Matchmaking());
        */
    }
}