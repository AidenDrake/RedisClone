// See https://aka.ms/new-console-template for more information

using System.Text;

Console.WriteLine("Hello world");

namespace RedisClone
{

    public class MessageParser // Let the kingdom of nouns hereby be established
    {
        public record ParserResponse(ParsedMessage? ParsedMessage, byte[] UnparsedRemainder);

        public ParserResponse Parse(byte[] bytes)
        {
            if (bytes[0] != (byte) '+')
            {
                return new ParserResponse(null, bytes);
            }
            var carriageIndex = -1;
            for (var i = 1; i < bytes.Length - 1; i++)
            {
                var cur = bytes[i];
                if (cur == '\r' && bytes[i + 1] == '\n')
                {
                    carriageIndex = i;
                }
            }

            if (carriageIndex == -1) return new ParserResponse(null, bytes);

            var slice = bytes[1..carriageIndex];

            var stringySlice = Encoding.ASCII.GetString(slice);

            return new ParserResponse( new ParsedMessage.SimpleStringMessage(stringySlice, slice), bytes[(carriageIndex+1)..]);
        }
    }
}