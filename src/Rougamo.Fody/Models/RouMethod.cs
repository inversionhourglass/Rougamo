using Fody;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rougamo.Fody
{
    internal sealed class RouMethod
    {
        private static readonly BindingFlags _BindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private bool? _isAsync;
        private bool? _isIterator;
        private bool? _isAsyncIterator;
        private Feature? _feature;
        private Omit? _omit;
        private readonly HashSet<Mo> _mos;
        private readonly RouType _rouType;
        private Mo[]? _sortedMos;

        private AccessFlags _methodLevelAccessibility;
        private AccessFlags _typeLevelAccessibility;
        private AccessFlags? _methodCategory;

        public RouMethod(RouType rouType, MethodDefinition methodDef)
        {
            _mos = new HashSet<Mo>(Mo.Comparer);
            _rouType = rouType;
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

        public Omit MethodContextOmits => _omit ??= Mos.Aggregate(Omit.All, (x, y) => x & y.MethodContextOmits);

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
            InitFlags();

            return (from == MoFrom.Class ? _methodLevelAccessibility : _typeLevelAccessibility) | _methodCategory!.Value;
        }

        private void InitFlags()
        {
            if (_methodCategory != null) return;

            var methodTarget = MethodDef.HasThis ? AccessFlags.Instance : AccessFlags.Static;
            _methodLevelAccessibility = (MethodDef.IsPublic ? AccessFlags.Public : AccessFlags.NonPublic) & methodTarget;
            _typeLevelAccessibility = (MethodDef.IsPublic && _rouType.TypeDef.IsPublic ? AccessFlags.Public : AccessFlags.NonPublic) & methodTarget;

            if (MethodDef.IsGetter)
            {
                _methodCategory = AccessFlags.PropertyGetter;
            }
            else if (MethodDef.IsSetter)
            {
                _methodCategory = AccessFlags.PropertySetter;
            }
            else if (MethodDef.IsConstructor)
            {
                _methodCategory = AccessFlags.Constructor;
            }
            else
            {
                _methodCategory = AccessFlags.Method;
            }
        }

        public Feature Features => _feature ??= Mos.Aggregate(Feature.None, (v, m) => v | m.Features);

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
                    var returnTypeDef = MethodDef.ReturnType.Resolve();
                    _isAsync = MethodDef.ReturnType.IsTask() || MethodDef.ReturnType.IsGenericTask() ||
                                returnTypeDef != null && returnTypeDef.CustomAttributes.Any(x => x.Is(Constants.TYPE_AsyncMethodBuilderAttribute));
                }
                return _isAsync.Value;
            }
        }
    }
}
