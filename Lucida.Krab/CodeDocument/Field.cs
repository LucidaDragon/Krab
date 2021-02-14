using Lucida.Krab.Characters;

namespace Lucida.Krab.CodeDocument
{
    public class Field : Member
    {
        public ICharacterSource Type { get; set; }
        public Expression InitialValue { get; set; }

        public Field(ICharacterSource type, ICharacterSource name, Expression initialValue = null) : base(name)
        {
            Type = type;
            InitialValue = initialValue;
        }

        public override string ToString()
        {
            return $"[Modifiers: {AccessModifiers}] {Type} ({Name}) = {(InitialValue == null ? "default" : $"{InitialValue}")}";
        }
    }
}
