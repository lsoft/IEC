using System;
using System.Collections.Generic;
using System.Text;

namespace IEC.Common.Publish
{
    public class Publisher : IPublisher
    {
        private readonly IPublishQueue _publishQueue;
        private readonly IThreadsFramesProvider _threadsFramesProvider;

        public Publisher(
            IPublishQueue publishQueue,
            IThreadsFramesProvider threadsFramesProvider
            )
        {
            if (publishQueue == null)
            {
                throw new ArgumentNullException(nameof(publishQueue));
            }

            if (threadsFramesProvider == null)
            {
                throw new ArgumentNullException(nameof(threadsFramesProvider));
            }

            _publishQueue = publishQueue;
            _threadsFramesProvider = threadsFramesProvider;
        }

        /// <inheritdoc />
        public event PublishedDelegate? PublishedEvent
        {
            add => _publishQueue.PublishedEvent += value;
            remove => _publishQueue.PublishedEvent -= value;
        }

        public void PublishFrames(
            Exception? excp
            )
        {
            var frames = _threadsFramesProvider.ExtractFrames();

            var pfc = new PublishedFrameCollection(
                frames,
                excp
                );
            _publishQueue.AppendToQueue(pfc);
        }
    }
}
