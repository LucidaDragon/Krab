using Lucida.Krab.Characters;
using Lucida.Krab.CodeDocument;
using System.Collections.Generic;
using System.Linq;

namespace Lucida.Krab.Parsing
{
    public class Node : ICharacterSource
    {
        private ICharacterSource Values => new CombinedSources(Name, Type, Children.Concat());

        public NodeType NodeType { get; set; } = NodeType.Undefined;
        public AccessModifiers AccessModifiers { get; set; } = AccessModifiers.None;
        public ICharacterSource Name { get; set; }
        public ICharacterSource Type { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();

        public long Length => Values.Length;
        public char this[long index] => Values[index];

        public string GetName(long index)
        {
            return Values.GetName(index);
        }

        public override string ToString()
        {
            return Values.Build();
        }
    }

    public enum NodeType
    {
        Undefined,
        Variable,
        Method
    }
}