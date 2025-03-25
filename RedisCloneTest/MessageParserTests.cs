using System.Text;
using StringParserNS;

namespace RedisCloneTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var parser = new MessageParser();
        var message = Encoding.ASCII.GetBytes("nonMessage");
        var parsedMessage = parser.Parse(message);
        Assert.That(parsedMessage, Is.Null);
    }
}