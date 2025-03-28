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

    public bool ArrayMessageMatches(IEnumerable<string> check, ParsedMessage.ArrayMessage am)
    {
        if (am.Value is null) return false;
        var checkArray = check as string[] ?? check.ToArray();
        if (checkArray.Length != am.Value.Length) return false;

        foreach (var (checkString, pm) in checkArray.Zip(am.Value))
        {
            if (pm is not ParsedMessage.BulkString bm) return false;
            if (bm.Value is null) return false;
            if (Encoding.ASCII.GetString(bm.Value) != checkString) return false;
        }

        return true;
    }
}