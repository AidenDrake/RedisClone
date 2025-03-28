// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
listener.Blocking = true;
var endpoint = new IPEndPoint(IPAddress.Any, 6379);
listener.Bind(endpoint);
listener.Listen(100);
var handler = listener.Accept();

while (true)
{
    var buffer = new byte[1024];
    var received = handler.Receive(buffer, SocketFlags.None);

    var response = Encoding.ASCII.GetString(buffer, 0, received);
    if (response.Length > 0)
    {
        Console.WriteLine($"Received {response}");
        break;
    }
}

    if (received > 0)
    {
        // Parsable?


        var parsedMessage = parser.Parse(response);
        break;
    }
}


namespace RedisClone {}