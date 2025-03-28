using System.Text;
using FluentAssertions;
using RedisClone;

namespace RedisCloneTest;

public class StreamReaderTest
{
    private RedisStreamReader _reader;
    private TestHelper _helper;


    [SetUp]
    public void Setup()
    {
        _reader = new RedisStreamReader();
        _helper = new TestHelper();
    }

    // [Test]
    // public void DocTest()
    // {
    //     var byteEncoding = _helper.StringArrayToBulkStringEncoding(new[] { "COMMAND", "DOCS" });
    //
    //     var testStream = new MemoryStream(byteEncoding);
    //
    //     var docOut = _reader.ReadStream(testStream);
    //
    //
    //     docOut.Should().BeEquivalentTo("Welcome to Redis Clone");
    //
    // }

}