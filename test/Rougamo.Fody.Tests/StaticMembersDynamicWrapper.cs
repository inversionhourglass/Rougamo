using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace Rougamo.Fody.Tests
{
    public class StaticMembersDynamicWrapper : DynamicObject
    {
        Type type;

        public StaticMembersDynamicWrapper(Type type)
        {
            this.type = type;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prop = type.GetProperty(
                binder.Name,
                BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            if (prop == null)
            {
                result = null;
                return false;
            }

            result = prop.GetValue(null, null);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var method = type.GetMethod(
                binder.Name,
                BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                result = null;
                return false;
            }

            result = method.Invoke(null, args);
            return true;
        }
    }
}
