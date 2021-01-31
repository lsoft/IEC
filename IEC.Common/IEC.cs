using System;
using System.Collections.Generic;
using System.Text;
using IEC.Common.Publish;
using IEC.Common.Read;

namespace IEC.Common
{
    public class IEC
    {
        public IThreadsFrames TFS
        {
            get;
        }

        public IPublisher Publisher
        {
            get;
        }

        public IReader Reader
        {
            get;
        }

        public IEC(
            IThreadsFrames tfs,
            IPublisher publisher,
            IReader reader
            )
        {
            if (tfs == null)
            {
                throw new ArgumentNullException(nameof(tfs));
            }

            if (publisher == null)
            {
                throw new ArgumentNullException(nameof(publisher));
            }

            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            TFS = tfs;
            Publisher = publisher;
            Reader = reader;
        }
    }
}
