using Issues.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue20
    {
        [_20_("1")]
        [_20_("2")]
        public async Task M(List<string> items) { }
    }
}
