using Rougamo;
using Rougamo.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BasicUsage.Mos
{
    public abstract class SetOnEntry : IMo
    {
        private const string LIST_KEY = nameof(LIST_KEY);
        private const string MAP_KEY = nameof(MAP_KEY);

        public double Order => 1;

        public virtual string Name => GetType().Name;

        public void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length >= 1)
            {
                var arg = context.Arguments[0];
                if (arg is List<string> executedMos)
                {
                    executedMos.Add(Name);
                    return;
                }
                if (arg is IDictionary map)
                {
                    map[Name] = Name;
                    return;
                }
            }
            if (context.Method is MethodInfo mi)
            {
                if (mi.ReturnType == typeof(List<string>))
                {
                    if (!context.Datas.Contains(LIST_KEY) || context.Datas[LIST_KEY] is not List<string> mos)
                    {
                        mos = new List<string>();
                        context.Datas[LIST_KEY] = mos;
                    }
                    mos.Add(Name);
                    context.ReplaceReturnValue(this, mos);
                }
                else if (typeof(IDictionary).IsAssignableFrom(mi.ReturnType))
                {
                    if (!context.Datas.Contains(MAP_KEY) || context.Datas[MAP_KEY] is not IDictionary map)
                    {
                        map = (IDictionary)Activator.CreateInstance(mi.ReturnType);
                        context.Datas[MAP_KEY] = map;
                    }
                    map[Name] = Name;
                    context.ReplaceReturnValue(this, map);
                }
            }
        }

        public void OnException(MethodContext context)
        {
        }

        public void OnExit(MethodContext context)
        {
        }

        public void OnSuccess(MethodContext context)
        {
        }

        public ValueTask OnEntryAsync(MethodContext context)
        {
            OnEntry(context);
            return default;
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            OnException(context);
            return default;
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            OnSuccess(context);
            return default;
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            OnExit(context);
            return default;
        }
    }
}
