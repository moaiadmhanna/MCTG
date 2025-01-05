using System.Data;
using MCTG.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MCTG.Data;
using MCTG.Data.Repositories;
using MCTG.Server;
using Npgsql;

namespace MCTG;
class Program
{
    static async Task Main(string[] args)
    {
        DatabaseConf databaseConf = new DatabaseConf("localhost", "admin","adminIf23b191","mctg");
        //DatabaseInitializer db = new DatabaseInitializer();
        //db.InitializeDB();
        HttpServer server = new HttpServer(IPAddress.Any, 10001);
        await server.Listen();
        //Testing the DB Initializer
            // DatabaseInitializer db = new DatabaseInitializer();
            // db.InitializeDB();
    }
}