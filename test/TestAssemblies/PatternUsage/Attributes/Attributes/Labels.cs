using System;

namespace PatternUsage.Attributes.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class Label1Attribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    public class Label2Attribute : Attribute { }
}
