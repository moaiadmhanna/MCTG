using MCTG.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MCTG.Data;
using MCTG.Server;

namespace MCTG;
class Program
{
    static void Main(string[] args)
    {
        //HttpServer server = new HttpServer(IPAddress.Any, 10001);
        //server.Listen();
        DatabaseConf databaseConf = new DatabaseConf("localhost", "admin","adminIf23b191","mctg");
        DatabaseInitializer db = new DatabaseInitializer();
        db.InitializeDB();
    }
}