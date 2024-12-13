using System.Net;
using System.Net.Sockets;
using System.Text;
using MCTG.Services;

namespace MCTG.Server;

public class HttpServer
{
    private TcpListener _server;
    private readonly BattleService _battleService = new BattleService();
    public HttpServer(IPAddress address, int port)
    {
        _server = new TcpListener(address, port);
        _server.Start();
        Console.WriteLine($"Listening on port {port}.....");
    }

    public async Task Listen()
    {
        while (true)
        {
            TcpClient client = await _server.AcceptTcpClientAsync();
            // Handle the client in a new task to allow for concurrent processing
            Task.Run(async () =>
            {
                using (client)
                {
                    using NetworkStream stream = client.GetStream();
                    using (StreamReader reader = new StreamReader(stream))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        HandleRequest requestHandler = new HandleRequest();
                        await requestHandler.ProcessRequest(reader, writer,_battleService); // Process the client's request asynchronously
                    }
                }
            });
        }
    }
}