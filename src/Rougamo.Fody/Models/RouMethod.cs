using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class RouMethod
    {
        private bool? _isAsync;
        private bool? _isIterator;
        private bool? _isAsyncIterator;

        public RouMethod(MethodDefinition methodDef)
        {
            Mos = new HashSet<Mo>(Mo.Comparer);
            MethodDef = methodDef;
        }

        public MethodDefinition MethodDef { get; set; }

        public HashSet<Mo> Mos { get; set; }

        public AccessFlags Flags(MoFrom from)
        {
            var flags = AccessFlags.All;
            bool? isPublic = null;
            if(from > MoFrom.Method)
            {
                isPublic = MethodDef.IsPublic;
                flags = MethodDef.HasThis ? AccessFlags.Instance : AccessFlags.Static;
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

        public bool IsIterator
        {
            get
            {
                if (!_isIterator.HasValue)
                {
                    _isIterator = MethodDef.CustomAttributes.Any(attr => attr.Is(Constants.TYPE_IteratorStateMachineAttribute));
                }
                return _isIterator.Value;
            }
        }

        public bool IsAsyncIterator
        {
            get
            {
                if (!_isAsyncIterator.HasValue)
                {
                    _isAsyncIterator = MethodDef.CustomAttributes.Any(attr => attr.Is(Constants.TYPE_AsyncIteratorStateMachineAttribute));
                }
                return _isAsyncIterator.Value;
            }
        }

        public bool IsAsyncTaskOrValueTask
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
