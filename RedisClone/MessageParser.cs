using System.Text;

namespace RedisClone;

// Stateless class
public class MessageParser
{
    public record ParserResponse(ParsedMessage? ParsedMessage, byte[] UnparsedRemainder);


    private readonly HashSet<char> _validSimpleTypeIdentifiers = ['+', '-', ':'];
    private readonly HashSet<char> _validTypeQuiDoitEtreSuiviParLengthIdentifiers = ['$', '*'];

    public MessageParser()
    {
    }

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


        var abc = bytes[1..carriageIndex];

        var stringifiedSlice = Encoding.ASCII.GetString(abc);

        var remainder = bytes[(carriageIndex + 2)..];

        // Case for a simple type
        if (_validSimpleTypeIdentifiers.Contains(firstChar))
        {
            ParsedMessage message = firstChar switch
            {
                '+' => new ParsedMessage.SimpleString(stringifiedSlice),
                '-' => new ParsedMessage.Error(stringifiedSlice),
                ':' => new ParsedMessage.Integer(int.Parse(stringifiedSlice)),
                _ => throw new ArgumentOutOfRangeException(nameof(bytes))
            };

            return new ParserResponse(message, remainder);
        }

        //Non - simple type case

        // Null bulk string
        if (stringifiedSlice == "-1")
        {
            return new ParserResponse(
                firstChar switch
                {
                    '$' => new ParsedMessage.BulkString(null),
                    '*' => new ParsedMessage.ArrayMessage(null),
                } ,
                remainder);
        }

        // The stringy slice contains the length
        if (!int.TryParse(stringifiedSlice, out var specifiedLength))
        {
            return new ParserResponse(null, bytes);
        }

        // Array case
        if (firstChar == '*')
        {
            var parsedArray = new List<ParsedMessage>();
            var arrayRemainder = remainder[..];

            if (specifiedLength == 0)
            {
                return new ParserResponse(new ParsedMessage.ArrayMessage(Array.Empty<ParsedMessage>()), remainder[2..]);
            }

            for (var i = 0; i < specifiedLength; i++)
            {
                var parsedRestOfTheArray = Parse(arrayRemainder);
                if (parsedRestOfTheArray.ParsedMessage == null)
                {
                    return new ParserResponse(null, bytes);
                }
                parsedArray.Add(parsedRestOfTheArray.ParsedMessage);
                arrayRemainder = parsedRestOfTheArray.UnparsedRemainder;
            }
            // handle rest of the string

            return new ParserResponse(new ParsedMessage.ArrayMessage(parsedArray.ToArray()) , arrayRemainder);

        }


        // BULK STRING CASE
        // assert that remainder[length] == \r and remainder[length + 1] == \n
        if (remainder.Length < specifiedLength + 2)
        {
            // Unparsable
            return new ParserResponse(null, bytes);
        }

        if (remainder[specifiedLength] != '\r' || remainder[specifiedLength+1] != '\n') return new ParserResponse(null, bytes);

        var bulkStringSlice = remainder[..specifiedLength];

        var remainingRemainder = remainder[(specifiedLength+2)..];

        return new ParserResponse(
            new ParsedMessage.BulkString(bulkStringSlice),
            remainingRemainder);
    }
}