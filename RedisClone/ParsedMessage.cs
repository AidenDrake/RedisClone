using System.Text;

namespace RedisClone;

public abstract class ParsedMessage : IEquatable<ParsedMessage>
{
    public abstract byte[] Encode();

    public bool Equals(ParsedMessage? pm)
    {
        return Encode().SequenceEqual(Encode());
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(ParsedMessage) && Equals((ParsedMessage)obj);
    }

    public override int GetHashCode()
    {
        return this.Encode().GetHashCode();
    }

    public class SimpleString(string Value) : ParsedMessage
    {
        public override byte[] Encode()
        {
            return Encoding.ASCII.GetBytes($"+{Value}\r\n");
        }

        public override string ToString()
        {
            return $"+{Value}";
        }


        public string Value { get; init; } = Value;


    };

    public class Error(string Value) : ParsedMessage
    {
        public override byte[] Encode()
        {
            return Encoding.ASCII.GetBytes($"-{Value}\r\n");
        }
        public override string ToString()
        {
            return $"-{Value}";
        }

        public string Value { get; init; } = Value;

    };

    public class Integer(int Value) : ParsedMessage
    {
        public override byte[] Encode()
        {
            return Encoding.ASCII.GetBytes($":{Value}\r\n");
        }
        public override string ToString()
        {
            return $"i:{Value}";
        }

        public int Value { get; init; } = Value;

    };

    public class BulkString(byte[]? Value) : ParsedMessage
    {
        public override string ToString()
        {
            return $"b:{Encoding.ASCII.GetString(Value)}";
        }
        public override byte[] Encode()
        {
            if (Value is null)
            {
                return "$-1\r\n"u8.ToArray();
            }

            var output = new List<byte>();
            output.AddRange(Encoding.ASCII.GetBytes($"${Value.Length}\r\n"));
            output.AddRange(Value);
            output.AddRange("\r\n"u8.ToArray());
            return output.ToArray();
        }

        public byte[]? Value { get; init; } = Value;
    };

    public class ArrayMessage(ParsedMessage[]? Value) : ParsedMessage
    {

        public override byte[] Encode()
        {
            if (Value is null)
            {
                return "*-1\r\n"u8.ToArray();
            }

            var output = new List<byte>();
            output.AddRange(Encoding.ASCII.GetBytes($"*{Value.Length}\r\n"));
            foreach (var e in Value)
            {
                output.AddRange(e.Encode());
            }
            output.AddRange("\r\n"u8.ToArray());
            return output.ToArray();
        }

        public override string ToString()
        {
            return Value is null ? "null" : Value.Aggregate("", (s, message) => s + message);
        }

        public ParsedMessage[]? Value { get; init; } = Value;

    };
}