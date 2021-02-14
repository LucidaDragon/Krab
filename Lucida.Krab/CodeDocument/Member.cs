using Lucida.Krab.Characters;

namespace Lucida.Krab.CodeDocument
{
    public abstract class Member
    {
        public ICharacterSource Name { get; set; }
        public AccessModifiers AccessModifiers { get; set; } = AccessModifiers.None;

        public Member(ICharacterSource name)
        {
            Name = name;
        }
    }
}
