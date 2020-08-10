using System;

namespace Rougamo.Fody
{
    internal class RougamoException : Exception
    {
        public RougamoException() : base() { }

        public RougamoException(string message) : base(message) { }
    }
}
