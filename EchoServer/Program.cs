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
            var listenerAcceptPromise = listener.AcceptTcpClientAsync(); //.ConfigureAwait(false);
            var client = await listenerAcceptPromise;
            HandleClient(client);
            // Console.WriteLine("I will write this when a new client connects");
        }
    }

    public static async void HandleClient(TcpClient client)
    {
        Console.WriteLine("new connection");
        var stream = client.GetStream();

        var buffer = new byte[1204];

        try
        {
            while (true)
            {
                var gotten = await stream.ReadAsync(buffer);
                if (gotten > 0)
                {
                    await stream.WriteAsync(buffer, 0, gotten);
                }
            }
        }
        catch (SocketException se) when (se.SocketErrorCode is SocketError.ConnectionReset)
        {
            Console.WriteLine("Connection Reset");
        }
        catch (SocketException e)
        {
            Console.WriteLine("Socket Exception" + e.Message);
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }
}