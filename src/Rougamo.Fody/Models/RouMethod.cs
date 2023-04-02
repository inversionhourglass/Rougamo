using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal sealed class RouMethod
    {
        private bool? _isAsync;
        private bool? _isIterator;
        private bool? _isAsyncIterator;
        private readonly HashSet<Mo> _mos;
        private Mo[]? _sortedMos;

        public RouMethod(MethodDefinition methodDef)
        {
            _mos = new HashSet<Mo>(Mo.Comparer);
            MethodDef = methodDef;
        }

        public MethodDefinition MethodDef { get; set; }

        public Mo[] Mos
        {
            get
            {
                if (_sortedMos == null)
                {
                    _sortedMos = _mos.OrderBy(x => x.Order).ToArray();
                }
                return _sortedMos;
            }
        }

        public void AddMo(Mo mo)
        {
            _sortedMos = null;

            _mos.Add(mo);
        }

        public void AddMo(IEnumerable<Mo> mos)
        {
            foreach (var mo in mos)
            {
                AddMo(mo);
            }
        }

        public bool MosAny() => _mos.Any();

        public bool MosAny(Func<Mo, bool> predicate) => _mos.Any(predicate);

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
