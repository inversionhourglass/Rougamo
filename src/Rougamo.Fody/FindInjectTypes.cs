using System.Linq;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void FindInjectTypes()
        {
        }

        void FindImplementInjectTypes()
        {
            ModuleDefinition.Types.Where(t => t.IsClass && !t.IsAbstract && t.HasInterfaces && t.IsAssignableFrom()))
        }
    }
}
