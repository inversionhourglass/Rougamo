using PatternUsage.Attributes.Attributes;
using Rougamo;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: AnyLabel1]
[assembly: TypeLabel1]
[assembly: ExecutionLabel1]
[module: AnyParameterLabel1]
[module: Parameter0Label1]
[module: Parameter2Label1]
[assembly: ReturnLabel1]

namespace PatternUsage
{
    [Label1]
    public class AttributeCase1 : IRougamo<MethodAttrComposeAttribute>
    {
        public List<string> Prop
        {
            get;
            [Label1]
            set;
        }

        [return: Label1]
        public static double SM(List<string> executedMos) => default;

        public async Task MAsync(List<string> executedMos) => await Task.Yield();

        [Label1]
        [return: Label2]
        public async ValueTask<int> M1Async([Label1] List<string> executedMos, int value)
        {
            await Task.Yield();
            return value;
        }

        public long AttributeCompose1([Label1] List<string> executedMos, int v1, int v2) => v1 + v2;

        public long AttributeCompose2([Label2] List<string> executedMos, int v1, [Label1] int v2) => v1 + v2;
    }

    [Label2]
    public class AttributeCase2 : IRougamo<MethodAttrComposeAttribute>
    {
        [Label1]
        public List<string> Prop { get; set; }

        [Label2]
        public static double SM(List<string> executedMos) => default;

        [return: Label1]
        public async Task MAsync([Label1] List<string> executedMos) => await Task.Yield();

        [Label1]
        [return: Label2]
        public async ValueTask<int> M1Async(List<string> executedMos, int value)
        {
            await Task.Yield();
            return value;
        }

        [Label1]
        public static long StaticAttributeCompose1([Label1] List<string> executedMos, [Label1] int v1, [Label1] int v2) => v1 + v2;

        [Label2]
        public static long StaticAttributeCompose2([Label2] List<string> executedMos, int v1, [Label1] int v2) => v1 + v2;
    }
}
