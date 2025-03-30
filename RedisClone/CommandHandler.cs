using System.Text;

namespace RedisClone;

public class CommandHandler
{
    public ParsedMessage HandleCommand(ParsedMessage parsedMessage, Dictionary<string, string> state)
    {
        if (parsedMessage is not ParsedMessage.ArrayMessage am)
        {
            return new ParsedMessage.Error("ERR unknown command");
        }

        if (am.Value is null or { Length: 0 })
        {
            // IDK why the real redis doesn't throw an error for these, but it doesn't
            return new ParsedMessage.BulkString(null);
        }

        var arguments = new List<ParsedMessage.BulkString>();
        foreach (var message in am.Value)
        {
            if (message is ParsedMessage.BulkString x)
            {
                arguments.Add(x);
            }
            else
            {
                return new ParsedMessage.Error("ERR unknown command");
            }
        }


        var first = arguments.First();

        if (ArrayMessageMatches(new []{"COMMAND", "DOCS"}, am))
        {
            return new ParsedMessage.SimpleString("Welcome to Redis Clone");
        }
        if (BulkStringMatches("PING", first))
        {
            return arguments.Count switch
            {
                1 => new ParsedMessage.SimpleString("PONG"),
                2 => arguments[1],
                _ => new ParsedMessage.Error("ERR wrong number of arguments for PING command")
            };
        }

        if ( BulkStringMatches("ECHO", first))
        {
            if (arguments.Count != 2)
            {
                return new ParsedMessage.Error("ERR wrong number of arguments for 'echo' command");
            }
            return arguments[1];
        }

        if (BulkStringMatches("SET", first))
        {
            if (arguments.Count != 3)
            {
                return new ParsedMessage.Error("ERR syntax error");
            }

            var key = arguments[1];
            var value = arguments[2];

            if (key.Value is null || value.Value is null)
            {
                return new ParsedMessage.Error("ERR syntax error");
            }

            var keyString = Encoding.ASCII.GetString(key.Value);
            state[keyString] = Encoding.ASCII.GetString(value.Value);

            return new ParsedMessage.SimpleString("OK");
        }

        if (BulkStringMatches("GET", first))
        {
            if (arguments.Count != 2)
            {
                return new ParsedMessage.Error("ERR syntax error");
            }

            var key = arguments[1];

            if (key.Value is null)
            {
                return new ParsedMessage.Error("ERR syntax error");
            }

            var keyString = Encoding.ASCII.GetString(key.Value);
            var result = state.GetValueOrDefault(keyString);
            if (result is null) return new ParsedMessage.BulkString(null);
            return new ParsedMessage.BulkString(Encoding.ASCII.GetBytes(result));
        }

        return new ParsedMessage.Error($"ERR unknown command '{Encoding.ASCII.GetString(first.Value ?? Array.Empty<byte>())}'");


    }

    public ParsedMessage.ArrayMessage ConvertStringListToParsedMessage(List<string> list)
    {
        var one = list.Select(x => new ParsedMessage.BulkString(Encoding.ASCII.GetBytes(x)));
        var two = one.ToArray<ParsedMessage>();
        var three = new ParsedMessage.ArrayMessage(two);
        return three;
    }

    public bool ArrayMessageMatches(IEnumerable<string> check, ParsedMessage.ArrayMessage am)
    {
        if (am.Value is null) return false;
        var checkArray = check as string[] ?? check.ToArray();
        if (checkArray.Length != am.Value.Length) return false;

        foreach (var (checkString, pm) in checkArray.Zip(am.Value))
        {
            if (!BulkStringMatches(checkString, pm)) return false;
        }

        return true;
    }

    public bool BulkStringMatches(string checkString, ParsedMessage pm)
    {
        if (pm is not ParsedMessage.BulkString bm) return false;
        if (bm.Value is null) return false;
        return string.Equals(Encoding.ASCII.GetString(bm.Value), checkString, StringComparison.InvariantCultureIgnoreCase);
    }
}