using System.Linq;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        void FindInjectTypes()
        {
            FindImplementInjectTypes();
        }

        void FindImplementInjectTypes()
        {
            var rougamos = ModuleDefinition.Types.Where(t => t.IsClass && !t.IsAbstract && t.HasInterfaces && _rougamoTd.IsAssignableFrom(t));
            System.Console.WriteLine(rougamos.Count());
        }
    }
}
