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

/*
 * So that the Parser does not hold state, I'm going to make the following assumptions about its input:
 * - The input always begins with a type indicator message or it is unparsable
 *  - the input is non-empty
 *
 * The parser will need to return the unparsed portion of its message
 */

public class Tests
{

    private MessageParser _parser;

    [SetUp]
    public void Setup()
    {
        _parser = new MessageParser();
    }

    // No nothing
    [TestCase("f")]
    // No carriage return
    [TestCase("+OK")]
    [TestCase("+")]
    [TestCase("?OK\r\n")]
    // No prefixed text
    [TestCase("asdfdsa+BOB\r\nafdsa")]
    public void NullTest(string input)
    {
        var parser = new MessageParser();
        var message = Encoding.ASCII.GetBytes(input);
        var parsedMessage = parser.Parse(message);
        Assert.That(parsedMessage.ParsedMessage, Is.Null);
    }

    // Parse simple string
    [TestCase("+OK\r\n", "OK", "")]
    [TestCase("+BOB\r\n", "BOB", "")]

    // Return non parsed portion of string
    [TestCase("+OK\r\n+A", "OK", "+A")]
    public void SimpleString(string input, string expectedString, string expectedNonParsedString)
    {
        var message = Encoding.ASCII.GetBytes(input);
        var parserResponse = _parser.Parse(message);
        var expectedByteArray = Encoding.ASCII.GetBytes(expectedString);
        var expectedNonParsedByteArray = Encoding.ASCII.GetBytes(expectedNonParsedString);

        if (parserResponse.ParsedMessage is not ParsedMessage.SimpleStringMessage ssm)
        {
            Assert.Fail();
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(ssm.Value, Is.EqualTo(expectedString));
            Assert.That(ssm.RawThatWasParsed, Is.EquivalentTo(expectedByteArray));
            Assert.That(parserResponse.UnparsedRemainder, Is.EquivalentTo(expectedNonParsedByteArray));
        });
    }


    // Parse Errors
    [Test]
    public void ErrorsAreNotSimpleStrings()
    {
        var message = Encoding.ASCII.GetBytes("-Error message\r\n");
        var parsedMessage = _parser.Parse(message);
        Assert.That(parsedMessage.ParsedMessage is not ParsedMessage.SimpleStringMessage);
    }

    [Test]
    public void ErrorsAreErrors()
    {
        var message = Encoding.ASCII.GetBytes("-Error message\r\n");
        var parsedMessage = _parser.Parse(message);
        Assert.That(parsedMessage.ParsedMessage is ParsedMessage.ErrorMessage);
    }
    // Check error messages

    // Parse Integers
    [Test]
    public void IntegersAreIntegers()
    {
        var message = Encoding.ASCII.GetBytes(":42\r\n");
        var parsedMessage = _parser.Parse(message);
        Assert.That(parsedMessage.ParsedMessage is ParsedMessage.IntegerMessage);
    }


    // Parse Null Strings

    // Parse Bulk Strings

    // Parse Arrays (oooh recursive)
}