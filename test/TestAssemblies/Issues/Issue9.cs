using Issues.Attributes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue9
    {
        static AsyncLocal<Node> Local = new();

        public static List<string> Nodestrings { get; } = new();

        public static async Task ManualTestAsync()
        {
            for (int i = 0; i < 2; i++)
            {
                await M3Async();
            }
        }

        public static async Task WeaveTestAsync()
        {
            for (int i = 0; i < 2; i++)
            {
                await M1Async();
            }
        }

        [_9_Test]
        private async static Task M1Async()
        {
            await M2Async();
        }

        [_9_Test]
        private async static Task M2Async()
        {
            await Task.Delay(123);
        }

        private async static Task M3Async()
        {
            var parent = Local.Value;
            var value = parent == null ? 0 : parent.Value;
            var node = new Node(parent, value + 1);
            Local.Value = node;
            Nodestrings.Add(node.ToString());

            await M4Async();

            Local.Value = Local.Value?.Parent;
        }

        private async static Task M4Async()
        {
            var parent = Local.Value;
            var value = parent == null ? 0 : parent.Value;
            var node = new Node(parent, value + 1);
            Local.Value = node;
            Nodestrings.Add(node.ToString());

            await Task.Delay(123);

            Local.Value = Local.Value?.Parent;
        }

        public class Node
        {
            public Node(Node parent, int value)
            {
                Parent = parent;
                Value = value;
            }

            public Node Parent { get; set; }

            public int Value { get; set; }

            public override string ToString()
            {
                var list = new List<int>();
                var node = this;
                do
                {
                    list.Insert(0, node.Value);
                } while ((node = node.Parent) != null);

                return string.Join(" -> ", list);
            }
        }
    }
}
