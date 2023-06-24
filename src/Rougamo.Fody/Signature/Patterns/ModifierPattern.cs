using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class ModifierPattern
    {
        private static readonly IReadOnlyDictionary<Modifier, Flags> _ModifierMap;

        private readonly Flags _required;
        private readonly bool _staticRequired;

        static ModifierPattern()
        {
            var map = new Dictionary<Modifier, Flags>();
            var modifiers = Enum.GetValues(typeof(Modifier));

            foreach (Modifier modifier in modifiers)
            {
                var modifierStr = modifier == Modifier.WithinClass ? "Private" : modifier.ToString();
                if (!Enum.TryParse<Flags>(modifierStr, out var flag)) continue;

                map[modifier] = flag;
                map[modifier | Modifier.Static] = flag | Flags.Static;
            }

            _ModifierMap = map;
        }

        public ModifierPattern(Flags required, Flags forbidden)
        {
            _staticRequired = (required & Flags.Static) == Flags.Static;
            required &= Flags.Any;
            _required = required == Flags.None ? Flags.Any : required;
            Forbidden = forbidden;
        }

        public Flags Required => _required == Flags.Any ? (_staticRequired ? Flags.Static : Flags.Any) : (_staticRequired ? _required | Flags.Static : _required);

        public Flags Forbidden { get; }

        public bool IsMatch(Modifier modifier)
        {
            if (!_ModifierMap.TryGetValue(modifier, out var flag)) throw new ArgumentException($"Unknow modifier: {modifier}");

            if ((Forbidden & flag) != 0) return false;

            var hasStatic = (flag & Flags.Static) == Flags.Static;
            return (_required & flag) == (flag & Flags.Any) && flag != Flags.Static && (!_staticRequired || hasStatic);
        }

        [Flags]
        public enum Flags
        {
            None = 0x0,
            Private = 0x1,
            PrivateProtected = 0x2,
            Internal = 0x4,
            Protected = 0x8,
            ProtectedInternal = 0x10,
            Public = 0x20,
            Static = 0x40,

            Any = Private | PrivateProtected | Internal | Protected | ProtectedInternal | Public,
        }
    }
}
