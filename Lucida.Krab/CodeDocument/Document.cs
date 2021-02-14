using System.Collections;
using System.Collections.Generic;

namespace Lucida.Krab.CodeDocument
{
    public class Document : IReadOnlyList<Member>
    {
        public Member this[int index]
        {
            get => Members[index];
            set => Members[index] = value;
        }

        public int Count => Members.Count;

        public IList<Member> Members { get; set; } = new List<Member>();

        public void Combine(Document otherDocument)
        {
            for (int i = 0; i < otherDocument.Count; i++) Members.Add(otherDocument[i]);
        }

        public override string ToString()
        {
            return $"Document:\n\t{string.Join("\n", Members).Replace("\n", "\n\t")}";
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
