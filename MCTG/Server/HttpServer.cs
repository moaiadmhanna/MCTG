using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MCTG.Server;

public class HttpServer
{
    private TcpListener _server;

    public HttpServer(IPAddress address, int port)
    {
        _server = new TcpListener(address, port);
        _server.Start();
        Console.WriteLine($"Listening on port {port}.....");
    }

    public void Listen()
    {
        while (true)
        {
            using (TcpClient client = _server.AcceptTcpClient())
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream);
                using StreamWriter writer = new StreamWriter(stream);
                HandleRequest requestHandler = new HandleRequest();
                requestHandler.ProcessRequest(reader, writer);
            }
        }
    }
}