// See https://aka.ms/new-console-template for more information

using System.Text;

Console.WriteLine("Hello world");

namespace RedisClone
{

    public class MessageParser // Let the kingdom of nouns hereby be established
    {
        public record ParserResponse(ParsedMessage? ParsedMessage, byte[] UnparsedRemainder);

        private readonly HashSet<byte> _validTypeIdentifiers = [(byte)'+', (byte)'-'];

        public ParserResponse Parse(byte[] bytes)
        {
            if (bytes.Length < 1)
            {
                return new ParserResponse(null, bytes);
            }

            var firstByte = bytes[0];
            if (!_validTypeIdentifiers.Contains(firstByte))
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

            if (firstByte == (byte)'+')
            {
                return new ParserResponse(
                    new ParsedMessage.SimpleStringMessage(stringySlice, slice),
                    bytes[(carriageIndex + 2)..]);
            }
            if (firstByte == (byte)'-')
            {
                return new ParserResponse(
                    new ParsedMessage.ErrorMessage(stringySlice, slice),
                    bytes[(carriageIndex + 2)..]);
            }

            throw new ArgumentException();
        }
    }
}