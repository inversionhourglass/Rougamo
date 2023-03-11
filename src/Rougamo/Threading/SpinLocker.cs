using System;
using System.Threading;

namespace Rougamo.Threading
{
    internal struct SpinLocker
    {
        private SpinLock _locker;
        private readonly int _retryTimes;

        public SpinLocker(int retryTimes = 3)
        {
            _locker = new();
            _retryTimes = retryTimes;
        }

        public IDisposable Enter()
        {
            var taken = false;
            var retryTimes = _retryTimes;
            while (retryTimes >= 0)
            {
                _locker.Enter(ref taken);
                if (taken) return new AutoExit(_locker);
                retryTimes--;
            }

            throw new InvalidOperationException("take locker failed");
        }

        struct AutoExit : IDisposable
        {
            private SpinLock _locker;

            public AutoExit(SpinLock locker)
            {
                _locker = locker;
            }

            public void Dispose()
            {
                _locker.Exit();
            }
        }
    }
}
