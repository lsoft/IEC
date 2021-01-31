using System;

namespace IEC.Common.Publish
{
    public interface IPublisher
    {
        event PublishedDelegate? PublishedEvent;

        void PublishFrames(
            Exception? excp
            );
    }
}
