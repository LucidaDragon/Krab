using Lucida.Krab.Characters;

namespace Lucida.Krab.CodeDocument
{
    public class Statement : Member
    {
        public Expression Expression { get; set; }

        public Statement(ICharacterSource name, Expression expression) : base(name)
        {
            Expression = expression;
        }

        public override string ToString()
        {
            return $"{Expression}";
        }
    }
}
