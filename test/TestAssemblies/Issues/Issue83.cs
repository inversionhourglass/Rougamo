using Issues.Attributes;
using System;

namespace Issues
{
    public class Issue83
    {
        [_83_]
        public int M()
        {
            try
            {
                throw new Exception("test");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
