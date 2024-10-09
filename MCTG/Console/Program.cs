using MCTG.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MCTG.Server;

namespace MCTG;
public class RequestBody // for login and register
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class PackageRequest // for package service
{
    public string Username { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        HttpServer server = new HttpServer(IPAddress.Any, 10001);
        server.Listen();
    }
}