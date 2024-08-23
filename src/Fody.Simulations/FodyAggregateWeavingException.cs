using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fody
{
    internal class FodyAggregateWeavingException(IEnumerable<FodyWeavingException> exceptions) : FodyWeavingException
    {
        public ReadOnlyCollection<FodyWeavingException> Exceptions => new(exceptions.ToArray());
    }
}
