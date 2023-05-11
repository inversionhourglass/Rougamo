using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class Parser
    {
        private static readonly Dictionary<string, ModifierPattern.Flags> _Modifiers = Enum.GetNames(typeof(ModifierPattern.Flags)).ToDictionary(x => x.ToLower(), x => (ModifierPattern.Flags)Enum.Parse(typeof(ModifierPattern.Flags), x));

        public static ModifierPattern ParseModifier(TokenSource tokens)
        {
            var required = ModifierPattern.Flags.Default;
            var forbidden = ModifierPattern.Flags.Default;
            while (tokens.TryPeekNotSpace(out var token) && token != null)
            {
                var offset = 0u;
                if (token.IsNot())
                {
                    token = tokens.Peek(1);
                    if (token == null) break;
                    offset = 1;
                }
                if (!_Modifiers.TryGetValue(token.ToString().ToLower(), out var value)) break;

                if (offset == 0)
                {
                    required |= value;
                }
                else
                {
                    forbidden |= value;
                }
                tokens.Read(offset);
            }

            return new ModifierPattern(required, forbidden);
        }
    }
}
