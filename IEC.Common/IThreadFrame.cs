using System.Collections.Generic;
using System.Text;

namespace IEC.Common
{
    public interface IThreadFrame
    {
        IEnumerable<object?> GetElementaries();

        void GenerateStringRepresentation(
            StringBuilder stringBuilder
            );
    }
}