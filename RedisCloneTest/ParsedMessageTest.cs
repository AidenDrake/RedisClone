using FluentAssertions;
using RedisClone;

namespace RedisCloneTest;

public class ParsedMessageTest
{
    [TestCase("hello")]
    public void SimpleStringEqualsTest(string s)
    {
        var ss1 = new ParsedMessage.SimpleString(s);
        var ss2 = new ParsedMessage.SimpleString(s);
        Assert.Multiple(() =>
        {
            Assert.That(ss1.Encode(), Is.EqualTo(ss2.Encode()));
            Assert.That((ss1.Equals(ss2)), Is.True);

            Assert.That(ss1, Is.EqualTo(ss2));
        });
    }
}