using Issues.Attributes;
using Rougamo;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue63
    {
        [_63_<int>]
        public int WillBeReplaced(int i) => i;

        [_63_<double>]
        public async ValueTask<float> WontBeReplaced(float f)
        {
            await Task.Yield();
            return f;
        }

        [Rougamo(typeof(_63_Attribute<string>))]
        public static string WillBeReplaced(string s) => s;
    }
}
