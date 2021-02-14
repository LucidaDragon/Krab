using System;

namespace Lucida.Krab.CodeDocument
{
    [Flags]
    public enum AccessModifiers
    {
        None = 0,
        Public = 1,
        Private = 2,
        Static = 4,
        Readonly = 8
    }
}
