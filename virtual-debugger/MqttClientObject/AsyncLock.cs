using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttClientObject
{
    internal class AsyncLock
    {
        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _lock;

            public Releaser(AsyncLock @lock)
            {
                _lock = @lock;
            }

            public void Dispose()
            {
                _lock._semaphore.Release();
            }
        }


        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _releaser;

        public AsyncLock()
        {
            _releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semaphore.WaitAsync();

            if (wait.IsCompleted)
            {
                return _releaser;
            }
            else
            {
                return wait.ContinueWith(
                    (_, state) => (IDisposable)state,
                    _releaser.Result,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
}
