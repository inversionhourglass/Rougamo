using System.Collections.Generic;

namespace SignatureUsage
{
    public class GenericCls<TX>
    {
        public List<List<TY>> Public<TY, TZ>(TX a, TZ? b) where TZ : struct
        {
            return null;
        }

        internal static List<TX> Internal<TY, TZ>(TY a, TZ b)
        {
            return new List<TX>();
        }

        private TX Private(TX a)
        {
            return a;
        }

        protected class ProtectedICls<TA, TB>
        {
            public TX Public<TC>(TA a, TB b, TC c)
            {
                return default;
            }

            private class PrivateICls<TU, TV>
            {
                internal (TV, TU, TX) Internal<TM, TN>(TA a, TB b, TM m, TN n)
                {
                    return default;
                }
            }
        }
    }
}
