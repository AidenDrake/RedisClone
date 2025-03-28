// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using RedisClone;

var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
listener.Blocking = true;
var endpoint = new IPEndPoint(IPAddress.Any, 6379);
listener.Bind(endpoint);
listener.Listen(100);
var handler = listener.Accept();

var stream = new NetworkStream(handler, true);

var parser = new MessageParser();
var buffer = new byte[1024];
var receivedSoFar = new List<byte>();

var redisStreamReader = new RedisStreamReader();

while (true)
{

    var read = handler.Receive(buffer, SocketFlags.None);

    if (read <= 0) continue;

    receivedSoFar.AddRange(buffer[..read]);

    var response = parser.Parse(receivedSoFar.ToArray());
    var parsedMessage = response.ParsedMessage;

    if (parsedMessage == null)
    {
        continue;
    }

    receivedSoFar.Clear();
    receivedSoFar.AddRange(response.UnparsedRemainder);

    byte[]? send = null;
    if (parsedMessage is ParsedMessage.ArrayMessage am)
    {
        if (redisStreamReader.ArrayMessageMatches(new []{"COMMAND", "DOCS"}, am))
        {
            send = new ParsedMessage.SimpleString("Welcome to Redis Clone").Encode();
        }
        if (redisStreamReader.ArrayMessageMatches(new []{"PING"}, am))
        {
            send = new ParsedMessage.SimpleString("PONG").Encode();
        }

        if (am.Value?[0] is ParsedMessage.BulkString bm && Encoding.ASCII.GetString(bm.Value) == "ECHO")
        {
            if (am.Value.ElementAtOrDefault(1) is not ParsedMessage.BulkString bs1) throw new Exception("whoops");
            send = new ParsedMessage.BulkString(bs1.Value ?? Array.Empty<byte>()).Encode();
        }
    }

    if (send is null) throw new Exception("oops");

    handler.Send(send);
}

//
// foreach (var e in read)
// {
//     Console.WriteLine(e);
// }


// var received = handler.Receive(buffer, 1024, SocketFlags.None);


namespace RedisClone {}