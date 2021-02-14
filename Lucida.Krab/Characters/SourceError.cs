using System;

namespace Lucida.Krab.Characters
{
    public class SourceError : Exception
    {
        public ICharacterSource ErrorSource { get; set; }

        public SourceError(ICharacterSource source, string message) : base(message)
        {
            ErrorSource = source;
        }

        public SourceError(ICharacterSource source, long index, long length, string message) : base(message)
        {
            ErrorSource = new CharacterRegion(source, index, length);
        }
    }
}
