using System.Security.AccessControl;
using System.Text;
using RedisClone;

namespace RedisCloneTest;

/*
 * We will need a module to extract messages from the stream.
   Each time we read from the stream we will get either:
   A partial message.
   A whole message.
   A whole message, followed by either 1 or 2.
   We will need to remove parsed bytes from the stream.
   *
 */

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    // No nothing
    [TestCase("f")]
    // No carriage return
    [TestCase("+OK")]
    [TestCase("+")]
    [TestCase("?OK\r\n")]
    public void NullTest(string input)
    {
        var parser = new MessageParser();
        var message = Encoding.ASCII.GetBytes(input);
        var parsedMessage = parser.Parse(message);
        Assert.That(parsedMessage, Is.Null);
    }

    // Parse simple string
    [TestCase("+OK\r\n", "OK", "OK")]
    [TestCase("+BOB\r\n", "BOB", "BOB")]
    [TestCase("asdfdsa+BOB\r\nafdsa", "BOB", "BOB")]
    public void SimpleString(string input, string expectedString, string expectedByteArrayString)
    {
        var parser = new MessageParser();
        var message = Encoding.ASCII.GetBytes(input);
        var parsedMessage = parser.Parse(message);
        var expectedByteArray = Encoding.ASCII.GetBytes(expectedByteArrayString);

        if (parsedMessage is not ParsedMessage.SimpleStringMessage ssm)
        {
            Assert.Fail();
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(ssm.Value, Is.EqualTo(expectedString));
            Assert.That(ssm.Raw, Is.EquivalentTo(expectedByteArray));
        });
    }
    // Parse Errors

    // Parse Integers

    // Parse Bulk Strings

    // Parse Arrays (oooh recursive)
}