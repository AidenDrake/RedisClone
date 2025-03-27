// See https://aka.ms/new-console-template for more information

using System.Text;

Console.WriteLine("Hello world");

namespace RedisClone
{

    public class MessageParser // Let the kingdom of nouns hereby be established
    {
        public record ParserResponse(ParsedMessage? ParsedMessage, byte[] UnparsedRemainder);


        private readonly HashSet<char> _validSimpleTypeIdentifiers = ['+', '-', ':'];
        private readonly HashSet<char> _validTypeQuiDoitEtreSuiviParLengthIdentifiers = ['$', '*'];

        public ParserResponse Parse(byte[] bytes)
        {
            if (bytes.Length < 3)
            {
                return new ParserResponse(null, bytes);
            }

            var firstChar = (char) bytes[0];
            if (!_validSimpleTypeIdentifiers.Contains(firstChar) && !_validTypeQuiDoitEtreSuiviParLengthIdentifiers.Contains(firstChar))
            {
                return new ParserResponse(null, bytes);
            }

            var carriageIndex = -1;
            for (var i = 1; i < bytes.Length - 1; i++)
            {
                var cur = bytes[i];
                if (cur == (byte) '\r' && bytes[i + 1] == (byte) '\n')
                {
                    carriageIndex = i;
                    break;
                }
            }

            if (carriageIndex == -1) return new ParserResponse(null, bytes);


            var slice = bytes[1..carriageIndex];

            var stringifiedSlice = Encoding.ASCII.GetString(slice);

            var remainder = bytes[(carriageIndex + 2)..];



            // Case for a simple type
            if (_validSimpleTypeIdentifiers.Contains(firstChar))
            {
                ParsedMessage message = firstChar switch
                {
                    '+' => new ParsedMessage.SimpleStringMessage(stringifiedSlice, slice),
                    '-' => new ParsedMessage.ErrorMessage(stringifiedSlice, slice),
                    ':' => new ParsedMessage.IntegerMessage(int.Parse(stringifiedSlice), slice),
                    _ => throw new ArgumentOutOfRangeException(nameof(bytes))
                };

                return new ParserResponse(message, remainder);
            }

            //Non - simple type case

            // Null bulk string
            if (slice.Length >= 5 && stringifiedSlice == "-1")
            {
                return new ParserResponse(
                    new ParsedMessage.BulkStringMessage(null, slice),
                    remainder);
            }

            // if remainder exactly looks like ['$', -1, r, n] ret null


            // The stringy slice contains the length

            if (!int.TryParse(stringifiedSlice, out var bulkStringLength))
            {
                return new ParserResponse(null, bytes);
            }



            // assert that remainder[length] == \r and remainder[length + 1] == \n
            if (remainder.Length < bulkStringLength + 2)
            {
                // Unparsable
                return new ParserResponse(null, bytes);
            }

            if (remainder[bulkStringLength] != '\r' || remainder[bulkStringLength+1] != '\n') return new ParserResponse(null, bytes);

            var bulkStringSlice = remainder[..bulkStringLength];
            var bulkString = Encoding.ASCII.GetString(bulkStringSlice);

            var remainingRemainder = remainder[(bulkStringLength+1)..];

            return new ParserResponse(
                new ParsedMessage.BulkStringMessage(bulkString, bulkStringSlice),
                remainingRemainder);
        }
    }
}