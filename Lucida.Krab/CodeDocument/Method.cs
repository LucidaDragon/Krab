using Lucida.Krab.Characters;
using System.Collections.Generic;

namespace Lucida.Krab.CodeDocument
{
    public class Method : Member
    {
        public IList<Field> Parameters { get; set; } = new List<Field>();
        public IList<Member> Body { get; set; } = new List<Member>();
        public ICharacterSource ReturnType { get; set; }

        public Method(ICharacterSource returnType, ICharacterSource name) : base(name)
        {
            ReturnType = returnType;
        }

        public override string ToString()
        {
            return $"[Modifiers: {AccessModifiers}] Method ({Name}({string.Join(", ", Parameters)})) -> {ReturnType}:\n\t{string.Join("\n", Body).Replace("\n", "\n\t")}";
        }
    }
}
