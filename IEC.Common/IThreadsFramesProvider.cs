using System.Collections.Generic;

namespace IEC.Common
{
    public interface IThreadsFramesProvider
    {
        List<IThreadFrame> ExtractFrames();
    }
}