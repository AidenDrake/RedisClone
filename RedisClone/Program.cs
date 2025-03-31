// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using RedisClone;

var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
listener.Blocking = true;
var endpoint = new IPEndPoint(IPAddress.Any, 6379);
listener.Bind(endpoint);
listener.Listen(100);
var handler = await listener.AcceptAsync();

var parser = new MessageParser();
var buffer = new byte[1024];
var receivedSoFar = new List<byte>();

var commandHandler = new CommandHandler();

var state = new Dictionary<string, string>();

while (true)
{

    var read = await handler.ReceiveAsync(buffer, SocketFlags.None);

    receivedSoFar.AddRange(buffer[..read]);

    switch (receivedSoFar.Count)
    {
        case 0: continue;
        case 1 when receivedSoFar.Single() == '\n': receivedSoFar.Clear();
            break;
    }

    var response = parser.Parse(receivedSoFar.ToArray());
    var parsedMessage = response.ParsedMessage;

    if (parsedMessage == null)
    {
        continue;
    }

    receivedSoFar.Clear();
    receivedSoFar.AddRange(response.UnparsedRemainder);

    var pm = commandHandler.HandleCommand(parsedMessage, state);

    var send = pm.Encode();

    await handler.SendAsync(send, 0);
}

//
// foreach (var e in read)
// {
//     Console.WriteLine(e);
// }


// var received = handler.Receive(buffer, 1024, SocketFlags.None);


namespace RedisClone {}