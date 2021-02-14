using Lucida.Krab.Characters;

namespace Lucida.Krab.CodeDocument
{
    public class ReferenceExpression : Expression
    {
        public ICharacterSource Reference { get; set; }

        public ReferenceExpression(ICharacterSource reference)
        {
            Reference = reference;
        }

        public override string ToString()
        {
            return $"{Reference}";
        }
    }
}
