using System;

namespace Lucida.Krab.Characters
{
    public class CharacterRegion : ICharacterSource
    {
        public long Index { get; set; }
        public long Length { get; set; }
        public ICharacterSource Source { get; set; }

        public char this[long index]
        {
            get
            {
                if (index >= Length || index < 0) throw new IndexOutOfRangeException();

                return Source[Index + index];
            }
        }

        public CharacterRegion() { }

        public CharacterRegion(ICharacterSource source, long index, long length)
        {
            Source = source;

            if (index >= Source.Length || index < 0) throw new IndexOutOfRangeException();

            Index = index;

            if (index + length > Source.Length) throw new IndexOutOfRangeException();

            Length = length;
        }

        public string GetName(long index)
        {
            if (index >= Length) throw new IndexOutOfRangeException();

            var absolute = Index + index;

            if (absolute >= Source.Length) throw new IndexOutOfRangeException();

            return GetName(index);
        }

        public override string ToString()
        {
            return this.Build();
        }
    }
}
