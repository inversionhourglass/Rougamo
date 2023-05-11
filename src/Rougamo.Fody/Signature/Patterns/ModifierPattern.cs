using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class ModifierPattern
    {
        private static readonly IReadOnlyDictionary<Modifier, Flags> _ModifierMap;

        static ModifierPattern()
        {
            var map = new Dictionary<Modifier, Flags>();
            var modifiers = Enum.GetValues(typeof(Modifier));

            foreach (Modifier modifier in modifiers)
            {
                if (!Enum.TryParse<Flags>(modifier.ToString(), out var flag)) continue;

                map[modifier] = flag;
                map[modifier | Modifier.Static] = flag | Flags.Static;
            }

            _ModifierMap = map;
        }

        public ModifierPattern(Flags required, Flags forbidden)
        {
            Required = required;
            Forbidden = forbidden;
        }

        public Flags Required { get; }

        public Flags Forbidden { get; }

        public bool IsMatch(Modifier modifier)
        {
            if (!_ModifierMap.TryGetValue(modifier, out var flag)) throw new ArgumentException($"Unknow modifier: {modifier}");

            return (Required & flag) == Required && (Forbidden & flag) == 0;
        }

        [Flags]
        public enum Flags
        {
            Default = 0x0,
            Private = 0x1,
            PrivateProtected = 0x2,
            Internal = 0x4,
            Protected = 0x8,
            ProtectedInternal = 0x10,
            Public = 0x20,
            Static = 0x40,
        }
    }
}
