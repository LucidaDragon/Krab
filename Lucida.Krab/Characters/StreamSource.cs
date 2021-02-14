using System.IO;

namespace Lucida.Krab.Characters
{
    public class StreamSource : ICharacterSource
    {
        public char this[long index]
        {
            get
            {
                Source.Position = index;
                return (char)Source.ReadByte();
            }
        }
        public long Length => Source.Length;

        public string Name { get; set; }
        public Stream Source { get; set; }

        public StreamSource() { }

        public StreamSource(string name, Stream source)
        {
            Name = name;
            Source = source;
        }

        public string GetName(long index)
        {
            return Name;
        }

        public override string ToString()
        {
            return this.Build();
        }
    }
}
