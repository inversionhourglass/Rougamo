using Mono.Cecil;
using Rougamo.Fody.Models;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class RouMethod
    {
        private bool? _isAsync;

        public RouMethod(MethodDefinition methodDef)
        {
            Mos = new HashSet<Mo>(Mo.Comparer);
            MethodDef = methodDef;
        }

        public MethodDefinition MethodDef { get; set; }

        public HashSet<Mo> Mos { get; set; }

        public AccessFlags Flags(MoFrom from)
        {
            var flags = MethodDef.HasThis ? AccessFlags.Instance : AccessFlags.Static;
            bool? isPublic = null;
            if(from > MoFrom.Method)
            {
                isPublic = MethodDef.IsPublic;
            }
            if(from > MoFrom.Class)
            {
                isPublic &= MethodDef.DeclaringType.IsPublic;
            }
            if (isPublic.HasValue)
            {
                flags &= isPublic.Value ? AccessFlags.Public : AccessFlags.NonPublic;
            }
            return flags;
        }

        public bool IsAsync
        {
            get
            {
                if (!_isAsync.HasValue)
                {
                    _isAsync = MethodDef.CustomAttributes.Any(attr => attr.Is(Constants.TYPE_AsyncStateMachineAttribute));
                }
                return _isAsync.Value;
            }
        }
    }
}
