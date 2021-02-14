using System.Collections.Generic;

namespace Lucida.Krab.Characters
{
    public interface ICharacterSource
    {
        long Length { get; }
        char this[long index] { get; }
        string GetName(long index);
    }

    public static class CharacterSourceMethods
    {
        public static ICharacterSource Substring(this ICharacterSource source, long index)
        {
            return new CharacterRegion(source, index, source.Length - index);
        }

        public static ICharacterSource Substring(this ICharacterSource source, long index, long length)
        {
            return new CharacterRegion(source, index, length);
        }

        public static ICharacterSource Concat(this ICharacterSource first, ICharacterSource second)
        {
            return new CombinedSources(first, second);
        }

        public static ICharacterSource Concat(this IEnumerable<ICharacterSource> sources)
        {
            return new CombinedSources(sources);
        }

        public static IEnumerable<char> GetEnumerable(this ICharacterSource source)
        {
            for (long i = 0; i < source.Length; i++)
            {
                yield return source[i];
            }
        }

        public static string Build(this ICharacterSource source)
        {
            var result = new char[source.Length];

            for (long i = 0; i < source.Length; i++)
            {
                result[i] = source[i];
            }

            return new string(result);
        }
    }
}