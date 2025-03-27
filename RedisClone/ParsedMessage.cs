namespace RedisClone;

public record ParsedMessage
{
    public record SimpleString(string Value): ParsedMessage;

    public record Error(string Value): ParsedMessage;

    public record Integer(int Value): ParsedMessage;

    public record BulkString(byte[]? Value): ParsedMessage;

    public record Array(ParsedMessage[]? Value) : ParsedMessage;
}