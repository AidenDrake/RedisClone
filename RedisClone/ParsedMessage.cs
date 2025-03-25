namespace RedisClone;

public record ParsedMessage(byte[] Raw)
{
    public record SimpleStringMessage(string? Value, byte[] Raw): ParsedMessage(Raw);
}