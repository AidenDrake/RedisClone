// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using RedisClone;

var service = new AsyncService();
await service.Run();

// var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
// listener.Blocking = false;
// var endpoint = new IPEndPoint(IPAddress.Any, 6379);
//
// listener.Bind(endpoint);
// listener.Listen(100);
// var handler = await listener.AcceptAsync();
//
// var parser = new MessageParser();
// var buffer = new byte[1024];
// var receivedSoFar = new List<byte>();
//
// var commandHandler = new CommandHandler();
//
// var state = new Dictionary<string, string>();

// while (true)
// {

    // var read = await handler.ReceiveAsync(buffer, SocketFlags.None);
    //
    // var response1 = Encoding.ASCII.GetString(buffer, 0, read);
    //
    // if (response1.Length > 0)
    // {
    //     Console.WriteLine($"Socket server received message: {response1}");

        // receivedSoFar.AddRange(buffer[..read]);

        // switch (receivedSoFar.Count)
        // {
        //     case 0: continue;
        //     case 1 when receivedSoFar.Single() == '\n': receivedSoFar.Clear();
        //         break;
        // }
        //
        // var response = parser.Parse(receivedSoFar.ToArray());
        // var parsedMessage = response.ParsedMessage;
        //
        // if (parsedMessage == null)
        // {
        //     continue;
        // }
        //
        // receivedSoFar.Clear();
        // receivedSoFar.AddRange(response.UnparsedRemainder);
        //
        // var pm = commandHandler.HandleCommand(parsedMessage, state);
        //
        // var send = pm.Encode();

        // var responseList = Encoding.ASCII.GetBytes(response1).ToList().Append((byte) '\r').Append((byte) '\n').ToArray() ;
        //
        // await handler.SendAsync( responseList, 0);
//     }
// }

//
// foreach (var e in read)
// {
//     Console.WriteLine(e);
// }

// var received = handler.Receive(buffer, 1024, SocketFlags.None);


namespace RedisClone
{
    public class AsyncService
    {
        public async Task Run()
        {
            var listener = new TcpListener(IPAddress.Any, 6379);
            listener.Start();
            Console.WriteLine("Service running");
            while (true)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    await Process(client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        public async Task Process (TcpClient tcpClient)
        {
            var clientEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from " + clientEndpoint);
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (true)
                {
                    string request = await reader.ReadLineAsync();
                    if (request != null)
                    {
                        Console.WriteLine("Received service request: " + request);
                        string response = Encoding.ASCII.GetString(new ParsedMessage.SimpleString("PONG").Encode());
                        Console.WriteLine("Computed response is: " + response + "\n");
                        await writer.WriteAsync(response);
                    }
                    else
                        break; // Client closed connection
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }
    }
}