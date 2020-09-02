using Mono.Cecil;
using Mono.Collections.Generic;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    public static class MonoCollectionExtensions
    {
        public static bool TryGet(this Collection<CustomAttributeNamedArgument> arguments, string name, out CustomAttributeNamedArgument? argument)
        {
            foreach (var arg in arguments)
            {
                if(arg.Name == name)
                {
                    argument = arg;
                    return true;
                }
            }
            argument = null;
            return false;
        }

        public static void AddRange<T>(this Collection<T> collection, IList<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
