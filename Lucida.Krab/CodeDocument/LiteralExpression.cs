using Lucida.Krab.Characters;
using System.Globalization;

namespace Lucida.Krab.CodeDocument
{
    public class LiteralExpression : Expression
    {
        public long Value { get; set; }

        public LiteralExpression(ICharacterSource value)
        {
            var str = value.Build();

            if (str.Length > 2 && str.ToLower().StartsWith("0x") && long.TryParse(str.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long hexValue))
            {
                Value = hexValue;
            }
            else if (long.TryParse(str, out long decimalValue))
            {
                Value = decimalValue;
            }
            else
            {
                throw new SourceError(value, "Invalid literal.");
            }
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
