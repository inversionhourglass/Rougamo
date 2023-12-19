using System;
using System.Threading;

namespace Rougamo.Threading
{
    internal struct SpinLocker(int retryTimes)
    {
        private SpinLock _locker = new();
        private readonly int _retryTimes = retryTimes;

        public void Enter()
        {
            var taken = false;
            var retryTimes = _retryTimes;
            while (retryTimes >= 0)
            {
                _locker.Enter(ref taken);
                if (taken) return;
                retryTimes--;
            }

            throw new InvalidOperationException("take locker failed");
        }

        public void Exit()
        {
            _locker.Exit();
        }
    }
}
