namespace RedisClone;

public record ParsedMessage()
{
    public record SimpleStringMessage(string Value): ParsedMessage;

    public record ErrorMessage(string Value): ParsedMessage;

    public record IntegerMessage(int Value): ParsedMessage;

    public record BulkStringMessage(byte[] Value): ParsedMessage;

}