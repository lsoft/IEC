using System;
using System.Collections.Concurrent;

namespace IEC.Target.SqlServer
{
    public static class TargetFileLocker
    {
        private static readonly ConcurrentDictionary<string, object> _lockers = new(StringComparer.InvariantCultureIgnoreCase);


        public static object GetFileLocker(
            string filePath
            )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var result = _lockers.GetOrAdd(
                filePath,
                fp =>
                {
                    return new object();
                }
                );

            return result;
        }
    }
}
