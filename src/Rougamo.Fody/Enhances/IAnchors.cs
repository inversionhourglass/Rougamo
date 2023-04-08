using System;

namespace Rougamo.Fody.Enhances
{
    internal interface IAnchors
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class SkipAttribute : Attribute { }
}
