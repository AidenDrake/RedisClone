namespace RedisClone;

public record ParsedMessage(byte[] RawThatWasParsed)
{
    public record SimpleStringMessage(string? Value, byte[] RawThatWasParsed): ParsedMessage(RawThatWasParsed);
}