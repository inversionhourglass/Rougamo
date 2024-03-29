﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage
{
    public class SingleGeneric<T> : NonPublicCaller
    {
        public List<string>? PublicProp { get; set; }

        internal static Task<int> StaticInternalAsync(List<string> executedMos) => Task.FromResult(0);

        protected void ProtectedGeneric<TP>(List<string> executedMos)
        {
            executedMos.Add(Convert.ToString(typeof(T)));
            executedMos.Add(Convert.ToString(default(TP)));
        }
    }
}
