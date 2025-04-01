using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace EchoServer;

class Program
{
    public static void Main(string[] args)
    {
        StartListener();

        Console.ReadLine();
    }

    public static async void StartListener()
    {
        var listener = new TcpListener(IPAddress.Any, 8000);
        listener.Start();

        while (true)
        {
            var listenerAcceptPromise = listener.AcceptTcpClientAsync().ConfigureAwait(false);
            var client = await listenerAcceptPromise;
            HandleClient(client);
        }
    }

    public static async void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();

        var buffer = new byte[1204];

        while (true)
        {
            var gotten = await stream.ReadAsync(buffer);
            if (gotten > 0)
            {
                await stream.WriteAsync(buffer, 0, gotten);
            }
        }

        // stream.Close();
        // client.Close();
    }
}