using BasicUsage.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class Issue8
    {
        private ExecuteCommand _command => new ExecuteCommand(obj =>
        {
            File.AppendAllLines(@"D:\issue8.txt", new[] { "act in" });
            Command2();
            File.AppendAllLines(@"D:\issue8.txt", new[] { "act out" });
        });

        [Log]
        public async Task Command()
        {
            await Task.Delay(500);
            throw new Exception("1231232");
        }

        [Log]
        public async void Command1()
        {
            await Task.Delay(500);
            throw new Exception("1231232");
        }

        [Log]
        public void Command2()
        {
            File.AppendAllLines(@"D:\issue8.txt", new[] { "command2 in" });
            throw new Exception("1231232");
        }

        public void Execute()
        {
            _command.Execute();
        }

        public class ExecuteCommand
        {
            private Action<object> _action;

            public ExecuteCommand(Action<object> action)
            {
                _action = action;
            }

            public void Execute()
            {
                _action(null);
            }
        }
    }
}
