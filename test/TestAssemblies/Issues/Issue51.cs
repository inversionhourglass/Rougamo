using Issues.Attributes;
using System;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue51
    {
        [_51_]
        public static async Task TestAsync()
        {
            try
            {
                await Task.Yield();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
