using Rougamo;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicUsage.Attributes
{
    internal class XAttribute : MoAttribute
    {
        public override string Pattern => "x";
    }

    internal class YAttribute : MoAttribute
    {
        public YAttribute()
        {
            Pattern = "y";
        }
    }

    internal class ZAttribute : MoAttribute
    {
        public override string Pattern { get; set; } = "z";
    }
}
