using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucida.Krab.Characters
{
    public class CombinedSources : ICharacterSource
    {
        public char this[long index] => GetSource(ref index)[index];
        public long Length => Sources.Select(s => s.Length).Sum();

        public IEnumerable<ICharacterSource> Sources { get; set; }

        public CombinedSources() { }

        public CombinedSources(params ICharacterSource[] sources)
        {
            Sources = sources;
        }

        public CombinedSources(IEnumerable<ICharacterSource> sources)
        {
            Sources = sources;
        }

        public string GetName(long index)
        {
            return GetSource(ref index).GetName(index);
        }

        private ICharacterSource GetSource(ref long index)
        {
            foreach (var source in Sources)
            {
                if (source.Length > index)
                {
                    return source;
                }
                else
                {
                    index -= source.Length;
                }
            }

            throw new IndexOutOfRangeException();
        }

        public override string ToString()
        {
            return this.Build();
        }
    }
}
