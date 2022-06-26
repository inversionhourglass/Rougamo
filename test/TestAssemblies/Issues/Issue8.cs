using Issues.Attributes;
using System;
using System.Threading.Tasks;

namespace Issues
{
    public class Issue8
    {
        private ExecuteCommand _command => new ExecuteCommand(obj =>
        {
            Command2();
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
