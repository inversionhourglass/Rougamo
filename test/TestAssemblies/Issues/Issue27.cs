using Issues.Attributes;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue27
    {
        [_27Ex_]
        [_27_]
        public T Get<T>() where T : new()
        {
            return new T();
        }

        [_27Ex_]
        [_27_]
        public Task<T> GetAsync<T>() where T : new()
        {
            return Task.FromResult(new T());
        }
    }
}
