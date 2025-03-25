// See https://aka.ms/new-console-template for more information

using System.Text;

Console.WriteLine("Hello world");

namespace RedisClone
{
    public class MessageParser // Let the kingdom of nouns hereby be established
    {
        public ParsedMessage? Parse(byte[] bytes)
        {

            var plusIndex = Array.IndexOf(bytes, (byte) '+');
            // from this index we need to find a /r/n
            if (plusIndex == -1) return null;

            var carriageIndex = -1;
            for (var i = plusIndex + 1; i < bytes.Length - 1; i++)
            {
                var cur = bytes[i];
                if (cur == '\r' && bytes[i + 1] == '\n')
                {
                    carriageIndex = i;
                }
            }

            if (carriageIndex == -1) return null;

            var slice = bytes[(plusIndex + 1)..carriageIndex];

            var bob = Encoding.ASCII.GetString(slice);

            return new ParsedMessage.SimpleStringMessage(bob, slice);
        }
    }
}