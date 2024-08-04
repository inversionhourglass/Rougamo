using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rougamo.Fody
{
    internal class AggregateRougamoException(IEnumerable<RougamoException> exceptions) : RougamoException
    {
        public ReadOnlyCollection<RougamoException> Exceptions => new(exceptions.ToArray());
    }
}
