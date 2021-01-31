using System;
using System.Collections.Generic;
using System.Text;
using IEC.Common.Other;

namespace IEC.Common.Publish
{
    public class PublishedFrameCollection
    {
        private readonly List<IThreadFrame> _frames;

        public Exception? Exception
        {
            get;
        }

        public IReadOnlyList<IThreadFrame> Frames => _frames;

        public PublishedFrameCollection(
            List<IThreadFrame> frames,
            Exception? exception
            )
        {
            if (frames == null)
            {
                throw new ArgumentNullException(nameof(frames));
            }

            Exception = exception;

            _frames = frames;
        }

        public void LogException(
            StringBuilder sb
            )
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            if (Exception is null)
            {
                return;
            }

            var exception = Exception;

            var exceptionIndex = 0;
            while (exception is not null)
            {
                sb.AppendLine($"Exception #{exceptionIndex}   {ReflectionHelper.GetHumanReadableTypeName(exception.GetType())}   {exception.Message}");
                sb.AppendLine(exception.StackTrace);
                sb.AppendLine();

                exception = exception.InnerException;
                exceptionIndex++;
            }

            sb.AppendLine();
            sb.AppendLine();
        }
    }
}