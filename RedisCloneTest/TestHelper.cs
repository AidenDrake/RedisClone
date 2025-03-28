using System.Text;

namespace RedisCloneTest;

public class TestHelper
{
    public byte[] StringArrayToBulkStringEncoding(IEnumerable<string> stringArray)
    {
        var output = new List<byte>();
        var enumerable = stringArray as string[] ?? stringArray.ToArray();

        var arrLength = enumerable.Length;
        output.AddRange(Encoding.ASCII.GetBytes($"*{arrLength}\r\n"));

        foreach (var s in enumerable)
        {
            var encodedString = $"${s.Length}\r\n{s}\r\n";
            output.AddRange(Encoding.ASCII.GetBytes(encodedString));
        }

        return output.ToArray();
    }
}