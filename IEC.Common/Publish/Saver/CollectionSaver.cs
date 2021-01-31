using System;
using System.Text;
using System.Threading;
using IEC.Common.Other;

namespace IEC.Common.Publish.Saver
{
    public abstract class CollectionSaver : ICollectionSaver
    {
        private long _disposed = 0L;

        public CollectionSaver()
        {
        }

        /// <inheritdoc />
        public void SaveCollections(
            PublishedFrameCollection[] collection,
            int actualCollectionCount
            )
        {
            var sb = new StringBuilder();
            for (var index = 0; index < actualCollectionCount; index++)
            {
                var pfc = collection[index];

                pfc.LogException(sb);

                var frameIndex = 0;
                foreach (var frame in pfc.Frames)
                {
                    if (frame is null)
                    {
                        //something strange, we cannot get null here, but we did
                        //let's skip this null
                        continue;
                    }

                    sb.AppendLine($"Frame #{frameIndex}");

                    frame.GenerateStringRepresentation(sb);
                    sb.AppendLine();

                    frameIndex++;
                }

                sb.AppendLine();
                sb.AppendLine();
            }

            LogCompleteMessage(sb.ToString());
        }

        protected abstract void LogCompleteMessage(
            string message
            );

        /// <inheritdoc />
        public abstract void Commit();

        protected abstract void DoDispose();

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1L) != 0L)
            {
                return;
            }

            DoDispose();
        }
    }
}