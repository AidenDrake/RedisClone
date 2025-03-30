using System.Text;

namespace RedisClone;

public class CommandHandler
{
    public ParsedMessage HandleCommand(ParsedMessage parsedMessage)
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


        var first = am.Value.First();

        if (first is not ParsedMessage.BulkString bm)
        {
            return new ParsedMessage.Error("ERR unknown command");
        }

        if (ArrayMessageMatches(new []{"COMMAND", "DOCS"}, am))
        {
            return new ParsedMessage.SimpleString("Welcome to Redis Clone");
        }
        if (BulkStringMatches("PING", bm))
        {
            switch (am.Value.Length)
            {
                case 1: return new ParsedMessage.SimpleString("PONG");
                case 2:
                {
                    if (am.Value[1] is not ParsedMessage.BulkString bm1)
                    {
                        return new ParsedMessage.Error("ERR wrong argument for PING command");
                    }
                    return bm1;
                }
                default: return new ParsedMessage.Error("ERR wrong number of arguments for PING command");
            }
        }

        if ( BulkStringMatches("ECHO", bm))
        {
            if (am.Value.ElementAtOrDefault(1) is not ParsedMessage.BulkString bs1)
            {
                return new ParsedMessage.Error("ERR wrong number of arguments for 'echo' command");
            }
            return bs1;
        }

        return new ParsedMessage.Error($"ERR unknown command '{Encoding.ASCII.GetString(bm.Value ?? Array.Empty<byte>())}'");


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
        return Encoding.ASCII.GetString(bm.Value) == checkString;
    }
}