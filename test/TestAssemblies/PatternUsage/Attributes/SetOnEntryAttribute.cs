using Rougamo;
using Rougamo.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PatternUsage.Attributes
{
    public abstract class SetOnEntryAttribute : MoAttribute
    {
        private const string LIST_KEY = nameof(LIST_KEY);
        private const string MAP_KEY = nameof(MAP_KEY);

        public override Feature Features => Feature.EntryReplace;

        public virtual string Name => GetType().Name;

        public override void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 1)
            {
                var arg = context.Arguments[0];
                if (arg is List<string> executedMos)
                {
                    executedMos.Add(Name);
                }
                else if (arg is IDictionary map)
                {
                    map[Name] = Name;
                }
            }
            else if (context.Arguments.Length == 0 && context.Method is MethodInfo mi)
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
    }
}
