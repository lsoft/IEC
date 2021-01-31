using System;
using System.Collections.Generic;
using System.Threading;
using IEC.Common.Scope;

namespace IEC.Common
{
    public class ThreadsFrames : IThreadsFrames
    {
        private readonly ThreadLocal<ThreadFrames> _threads = new(() => new ThreadFrames());

        private readonly ScopeFactory _scopeFactory;

        private long _disposed = 0L;

        public ThreadsFrames(
            
            )
        {
            _scopeFactory = new ScopeFactory(ExitFrame);
        }

        private void ExitFrame(
            IThreadFrame frame
            )
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            _threads.Value!.PopFrame(frame);
        }

        public List<IThreadFrame> ExtractFrames()
        {
            var result = _threads.Value!.ExtractFrames();

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            _threads.Dispose();
        }

        public T CreateMutableScope<T>(
            )
            where T : MutableScope
        {
            var scope = _scopeFactory.CreateMutableScope<T>();
            _threads.Value!.AddFrame(scope);

            return scope;
        }

        public ImmutableScope CreateImmutableScope(
            params object?[] objects
            )
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            var scope =  _scopeFactory.CreateImmutableScope(objects);
            _threads.Value!.AddFrame(scope);

            return scope;
        }
    }
}