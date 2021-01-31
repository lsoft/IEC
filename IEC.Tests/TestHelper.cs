using System;
using System.Collections.Generic;
using System.Linq;

namespace IEC.Tests
{
    public static class TestHelper
    {
        public static T OnlyFirst<T>(
            this IEnumerable<object> collection
            )
        {
            var list = collection.ToList();

            if (list.Count != 1)
            {
                throw new InvalidOperationException("Collection should contains only 1 element");
            }

            var f = list[0];

            return (T) f;
        }
    }
}