using Lucida.Krab.Characters;
using System.Collections;
using System.Collections.Generic;

namespace Lucida.Krab.CodeDocument
{
    public class Namespace : Member, IReadOnlyList<Member>
    {
        public Member this[int index]
        {
            get => Members[index];
            set => Members[index] = value;
        }

        public int Count => Members.Count;

        public IList<Member> Members { get; set; } = new List<Member>();

        public Namespace(ICharacterSource name) : base(name) { }

        public override string ToString()
        {
            return $"[Modifiers: {AccessModifiers}] Namespace ({Name}):\n\t{string.Join("\n", Members).Replace("\n", "\n\t")}";
        }

        public IEnumerator<Member> GetEnumerator()
        {
            return Members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
