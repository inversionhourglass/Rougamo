﻿using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Mos
{
    public struct ValueOmitMos : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public string Pattern => null;

        public Feature Features => Feature.OnEntry;

        public double Order => 1;

        public Omit MethodContextOmits => Omit.Mos;

        public void OnEntry(MethodContext context)
        {
            if (context.Mos.Count == 0)
            {
                this.SetOnEntry(context);
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
    }
}
