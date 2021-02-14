namespace Lucida.Krab.Characters
{
    public class StringSource : ICharacterSource
    {
        public char this[long index] => Source[(int)index];
        public long Length => Source.Length;

        public string Name { get; set; }
        public string Source { get; set; }

        public StringSource() { }

        public StringSource(string name, string source)
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
            return Source;
        }
    }
}
