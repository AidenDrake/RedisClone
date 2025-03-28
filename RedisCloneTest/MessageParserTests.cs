using System.Runtime.InteropServices.Marshalling;
using System.Security.AccessControl;
using System.Text;
using FluentAssertions;
using NUnit.Framework.Internal;
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
    // No prefixed text -- maybe this should throw error instead
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
        var expectedNonParsedByteArray = Encoding.ASCII.GetBytes(expectedNonParsedString);

        if (parserResponse.ParsedMessage is not ParsedMessage.SimpleString ssm)
        {
            Assert.Fail();
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(ssm.Value, Is.EqualTo(expectedString));
            Assert.That(parserResponse.UnparsedRemainder, Is.EquivalentTo(expectedNonParsedByteArray));
        });
    }


    // Parse Errors
    [Test]
    public void ErrorsAreNotSimpleStrings()
    {
        var message = Encoding.ASCII.GetBytes("-Error message\r\n");
        var parsedMessage = _parser.Parse(message);
        Assert.That(parsedMessage.ParsedMessage is not ParsedMessage.SimpleString);
    }

    [Test]
    public void ErrorsAreErrors()
    {
        var message = Encoding.ASCII.GetBytes("-Error message\r\n");
        var parserResponse = _parser.Parse(message);

        Assert.That(parserResponse.ParsedMessage,Is.InstanceOf(typeof(ParsedMessage.Error)));
        if (parserResponse.ParsedMessage is not ParsedMessage.Error em)
        {
            Assert.Fail();
            return;
        }

        Assert.Multiple(() =>
        {
            // Check error messages
            Assert.That(em.Value, Is.EqualTo("Error message"));

            Assert.That(parserResponse.UnparsedRemainder, Is.Empty);
        });
    }

    // Parse Integers
    [TestCase(":42\r\n",42, TestName = "Non-negative number")]
    [TestCase(":-42\r\n",-42, TestName = "Negative number")]
    [TestCase(":+42\r\n",+42, TestName = "Definitely positive number")]
    public void IntegersAreIntegers(string messageString, int expectedValue)
    {
        var message = Encoding.ASCII.GetBytes(messageString);
        var parserResponse = _parser.Parse(message);
        Assert.That(parserResponse.ParsedMessage, Is.InstanceOf(typeof(ParsedMessage.Integer)));
        if (parserResponse.ParsedMessage is not ParsedMessage.Integer im)
        {
            Assert.Fail();
            return;
        }


        Assert.That(im.Value, Is.EqualTo(expectedValue));
    }

    // Parse Bulk Strings (incl. null - "$-1\r\n"), (empty "$0\r\n\r\n") (hello $5\r\nhello\r\n)
    // ("$$$ Money's great, so are escaped characters \r\n, \" don't you agree?)
    // [TestCase("$$$ Money's great, so are escaped characters \\r\\n, \" \\ don't you agree?")]
    // several long, 556 e.g.
    // check raw that was parsed
    [TestCase("")]
    [TestCase("hello")]
    [TestCase("$hello")]
    [TestCase("$$$ Money's great, so are escaped characters \\r\\n, \" \\ don't you agree?")]
    [TestCase("And did those feet, in ancient times, walk upon england's pleasant fields")]
    public void TestBulkStrings(string bulkString)
    {
        var encodedString = $"${bulkString.Length}\r\n{bulkString}\r\n";
        var message = Encoding.ASCII.GetBytes(encodedString);
        var parserResponse = _parser.Parse(message);

        if (parserResponse.ParsedMessage is not ParsedMessage.BulkString bgm)
        {
            Assert.Fail();
            return;
        }

        Assert.That(bgm.Value, Is.EquivalentTo(Encoding.ASCII.GetBytes(bulkString)));
    }

    // Bulk string with extra detail

    // invalid bulk strings
    [TestCase("$5hello\r\n")]
    [TestCase("$70hello\r\n")]
    [TestCase("$70\r\nhello\r\n")]
    [TestCase("$70\r\nhello")]
    [TestCase("$5\r\nhello")]
    [TestCase("$5\r\nhello+OP")]
    public void TestInvalidBulkStrings(string testString)
    {
        var message = Encoding.ASCII.GetBytes(testString);
        var parserResponse = _parser.Parse(message);
        // The whole message is unparseable
        Assert.That(parserResponse.UnparsedRemainder, Is.EquivalentTo(message));
    }

    // Test null bulk string
    [Test]
    public void NullBulkString()
    {
        var nullEncoding = "$-1\r\n"u8.ToArray();
        var parserResponse = _parser.Parse(nullEncoding);

        Assert.That(parserResponse.ParsedMessage,Is.InstanceOf(typeof(ParsedMessage.BulkString)));
        if (parserResponse.ParsedMessage is not ParsedMessage.BulkString bsm)
        {
            Assert.Fail();
            return;
        }

        Assert.Multiple(() =>
        {
            // Check error messages
            Assert.That(bsm.Value, Is.EqualTo(null));

            Assert.That(parserResponse.UnparsedRemainder, Is.Empty);
        });
    }

    [Test]
    public void NullArray()
    {
        var nullEncoding = "*-1\r\n"u8.ToArray();
        var parserResponse = _parser.Parse(nullEncoding);

        Assert.That(parserResponse.ParsedMessage,Is.InstanceOf(typeof(ParsedMessage.ArrayMessage)));
        if (parserResponse.ParsedMessage is not ParsedMessage.ArrayMessage am)
        {
            Assert.Fail();
            return;
        }

        // Check error messages
        Assert.That(am.Value, Is.EqualTo(null));
    }

    [Test]
    public void BulkStringRemainder()
    {
        var stringWithExtra = "$5\r\nhello\r\n+OK"u8.ToArray();
        var parserResponse = _parser.Parse(stringWithExtra);

        Assert.That(parserResponse.ParsedMessage,Is.InstanceOf(typeof(ParsedMessage.BulkString)));
        if (parserResponse.ParsedMessage is not ParsedMessage.BulkString bsm)
        {
            Assert.Fail();
            return;
        }

        Assert.Multiple(() =>
        {
            // Check error messages
            Assert.That(bsm.Value, Is.EqualTo("hello"u8.ToArray()));
            Assert.That(parserResponse.UnparsedRemainder, Is.EqualTo("+OK"u8.ToArray()));
        });
    }

    [Test]
    public void Array()
    {
        var testBytes = "*2\r\n$4\r\necho\r\n$11\r\nhello world\r\n+extrastuff"u8.ToArray();
        var expected = new ParsedMessage[]
            { new ParsedMessage.BulkString(stba("echo")), new ParsedMessage.BulkString(stba("hello world")) };

        var parserResponse = _parser.Parse(testBytes);
        var parsedMessage = parserResponse?.ParsedMessage;
        var unparsed = parserResponse?.UnparsedRemainder;
        Assert.That(parsedMessage, Is.InstanceOf(typeof(ParsedMessage.ArrayMessage)));
        if (parsedMessage is ParsedMessage.ArrayMessage am)
        {
            am.Value.Should().NotBeEmpty();
            am.Value.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
            unparsed.Should().BeEquivalentTo("+extrastuff"u8.ToArray());

        }
        else
        {
            Assert.Fail();
        }
    }


    [TestCaseSource(nameof(DivideCases))]
    public void DivideTest(int n, int d, int q)
    {
        Assert.AreEqual(q, n / d);
    }

    public static object[] DivideCases =
    {
        new object[] { 12, 3, 4 },
        new object[] { 12, 2, 6 },
        new object[] { 12, 4, 3 }
    };

    private static TestCaseData[] _arrayInputsSource =
    {
        new (
            "*2\r\n$4\r\necho\r\n$11\r\nhello world\r\n+extrastuff"u8.ToArray(),
            new ParsedMessage[]
            {
                new ParsedMessage.BulkString(stba("echo")) ,
                new ParsedMessage.BulkString(stba("hello world"))
            },
            "+extrastuff"u8.ToArray()
        ),
        new (
            "*0\r\n\r\n+extrastuff"u8.ToArray(),
            new ParsedMessage[]
            {
            },
            "+extrastuff"u8.ToArray()
        )
    };


    // [TestCase("*2\r\n$4\r\necho\r\n$11\r\nhello world\r\n+extrastuff"u8.ToArray(), new ParsedMessage[]
    //     { new ParsedMessage.BulkString(stba("echo")), new ParsedMessage.BulkString(stba("hello world")) }; )]
    [TestCaseSource(nameof(_arrayInputsSource))]
    public void ArrayInputs(byte[] test, ParsedMessage[] expectedMessages, byte[] extraStuff)
    {
        var parserResponse = _parser.Parse(test);
        var parsedMessage = parserResponse?.ParsedMessage;
        var unparsed = parserResponse?.UnparsedRemainder;
        Assert.That(parsedMessage, Is.InstanceOf(typeof(ParsedMessage.ArrayMessage)));
        if (parsedMessage is ParsedMessage.ArrayMessage am)
        {
            am.Value.Should().BeEquivalentTo(expectedMessages, options => options.RespectingRuntimeTypes());
            unparsed.Should().BeEquivalentTo(extraStuff);
        }
        else
        {
            Assert.Fail();
        }
    }


    private static byte[] stba(string inp)
    {
        return Encoding.ASCII.GetBytes(inp);
    }

    // // Parse Arrays (oooh recursive)
    // [Test]
    // public void TestArrays()
    // {
    //     var encodedString = ""u8;
    //     var message = Encoding.ASCII.GetBytes(encodedString);
    //     var parserResponse = _parser.Parse(message);
    //
    //     if (parserResponse.ParsedMessage is not ParsedMessage.BulkStringMessage bgm)
    //     {
    //         Assert.Fail();
    //         return;
    //     }
    //
    //     Assert.That(bgm.Value, Is.EqualTo(bulkString));
    // }

    // Handle a large buffer with multiple messages

}