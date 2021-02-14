using Lucida.Krab.Characters;
using System.Collections.Generic;

namespace Lucida.Krab.CodeDocument
{
    public class Structure : Member
    {
        public IList<Member> Members { get; set; } = new List<Member>();

        public Structure(ICharacterSource name) : base(name) { }

        public override string ToString()
        {
            return $"[Modifiers: {AccessModifiers}] Structure ({Name}):\n\t{string.Join("\n", Members).Replace("\n", "\n\t")}";
        }
    }
}
