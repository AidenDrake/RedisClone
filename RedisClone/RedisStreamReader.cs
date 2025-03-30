using System.Net.Sockets;
using System.Text;

namespace RedisClone;

public class RedisStreamReader
{
     // public IEnumerable<string> ReadStream(Stream inputStream)
     // {
     //     var parser = new MessageParser();
     //     var buffer = new byte[1024];
     //     var receivedSoFar = new List<byte>();
     //
     //     while (true)
     //     {
     //
     //         var read = inputStream.Read(buffer);
     //
     //
     //         if (read <= 0) continue;
     //
     //         receivedSoFar.AddRange(buffer[..read]);
     //
     //         var response = parser.Parse(receivedSoFar.ToArray());
     //         var parsedMessage = response.ParsedMessage;
     //
     //         if (parsedMessage == null)
     //         {
     //             continue;
     //         }
     //
     //         if (parsedMessage is ParsedMessage.ArrayMessage am)
     //         {
     //             if (ArrayMessageMatches(new []{"COMMAND", "DOCS"}, am))
     //             {
     //                 yield return "Welcome to Redis Clone";
     //             }
     //             if (ArrayMessageMatches(new []{"PING"}, am))
     //             {
     //                 yield return "+PONG\r\n";
     //             }
     //         }
     //     }
     // }


}