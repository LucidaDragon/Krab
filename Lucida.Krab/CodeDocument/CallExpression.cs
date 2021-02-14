using Lucida.Krab.Characters;
using System.Collections.Generic;

namespace Lucida.Krab.CodeDocument
{
    public class CallExpression : Expression
    {
        public ICharacterSource Call { get; set; }
        public IList<Expression> Arguments { get; set; } = new List<Expression>();

        public CallExpression(ICharacterSource call)
        {
            Call = call;
        }

        public override string ToString()
        {
            return $"{Call}({string.Join(", ", Arguments)})";
        }
    }
}
