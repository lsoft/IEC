using System;

namespace IEC.Common.Publish
{
    public interface IPublishQueue : IDisposable
    {
        event PublishedDelegate? PublishedEvent;

        void AppendToQueue(
            PublishedFrameCollection collection
            );

    }
}