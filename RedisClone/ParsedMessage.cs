namespace RedisClone;

public record ParsedMessage(byte[] RawThatWasParsed)
{
    public record SimpleStringMessage(string Value, byte[] RawThatWasParsed): ParsedMessage(RawThatWasParsed);

    public record ErrorMessage(string Value, byte[] RawThatWasParsed): ParsedMessage(RawThatWasParsed);

    public record IntegerMessage(int Value, byte[] RawThatWasParsed): ParsedMessage(RawThatWasParsed);

    public record BulkStringMessage(string? Value, byte[] RawThatWasParsed): ParsedMessage(RawThatWasParsed);

}