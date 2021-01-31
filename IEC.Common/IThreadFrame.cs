using System.Collections.Generic;
using System.Text;

namespace IEC.Common
{
    public interface IThreadFrame
    {
        IReadOnlyCollection<object?> GetElementaries();

        void GenerateStringRepresentation(
            StringBuilder stringBuilder
            );
    }
}