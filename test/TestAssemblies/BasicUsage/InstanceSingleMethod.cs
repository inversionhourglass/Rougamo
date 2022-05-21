using BasicUsage.Attributes;
using System;

namespace BasicUsage
{
    public class InstanceSingleMethod : MoDataContainer
    {
        [OnEntry]
        public string Entry(int number, string str, object[] array)
        {
            return $"{number}|{str}|{string.Join(",", array)}";
        }

        [OnException]
        public string Exception()
        {
            throw new InvalidOperationException(nameof(Exception));
        }

        [OnSuccess]
        public int Success()
        {
            return nameof(Success).Length;
        }

        [OnExit]
        public string ExitWithException()
        {
            throw new InvalidOperationException(nameof(ExitWithException));
        }

        [OnExit]
        public string ExitWithSuccess()
        {
            return nameof(ExitWithSuccess);
        }
    }
}
