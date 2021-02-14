using Lucida.Krab.Characters;

namespace Lucida.Krab.Parsing
{
    public struct Token : ICharacterSource
    {
        public char this[long index] => Value[index];
        public long Length => Value.Length;

        public TokenType Type;
        public ICharacterSource Value;

        public Token(TokenType type, ICharacterSource value)
        {
            Type = type;
            Value = value;
        }

        public string GetName(long index)
        {
            return Value.GetName(index);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum TokenType
    {
        Symbol,
        Name,
        Value
    }
}
