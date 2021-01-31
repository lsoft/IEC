using System;
using System.Collections.Generic;
using System.Text;

namespace IEC.Common.Read
{
    public interface IReader
    {
        List<StoredFrame> ReadBetween(
            DateTime inclusiveFrom,
            DateTime exclusiveTo
            );
    }

    public class StoredFrame
    {
        public DateTime InsertDate
        {
            get;
        }

        public string FilePath
        {
            get;
        }

        public long Offset
        {
            get;
        }

        public long Length
        {
            get;
        }

        public string Body
        {
            get;
            private set;
        }

        public StoredFrame(
            DateTime insertDate,
            string filePath,
            long offset,
            long length
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            InsertDate = insertDate;
            FilePath = filePath;
            Offset = offset;
            Length = length;

            Body = string.Empty;
        }

        internal void AppendBody(
            string body
            )
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            Body = body;
        }
    }
}
