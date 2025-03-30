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

var parser = new MessageParser();
var buffer = new byte[1024];
var receivedSoFar = new List<byte>();

var redisStreamReader = new RedisStreamReader();
var commandHandler = new CommandHandler();

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

    var pm = commandHandler.HandleCommand(parsedMessage);

    var send = pm.Encode();

    handler.Send(send);
}

//
// foreach (var e in read)
// {
//     Console.WriteLine(e);
// }


// var received = handler.Receive(buffer, 1024, SocketFlags.None);


namespace RedisClone {}