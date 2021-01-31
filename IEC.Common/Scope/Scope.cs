using System;
using System.Collections.Generic;
using System.Text;
using IEC.Common.Dump;

namespace IEC.Common.Scope
{
    public abstract class Scope : IThreadFrame, IDisposable
    {
        private readonly Action<IThreadFrame> _disposeAction;

        protected Scope(
            Action<IThreadFrame> disposeAction
            )
        {
            if (disposeAction == null)
            {
                throw new ArgumentNullException(nameof(disposeAction));
            }

            _disposeAction = disposeAction;
        }


        /// <inheritdoc />
        public void GenerateStringRepresentation(
            StringBuilder stringBuilder
            )
        {
            var dumper = new ObjectDumper(int.MaxValue, int.MaxValue);

            //dumper.Dump(stringBuilder, this);

            foreach (var o in GetElementaries())
            {
                dumper.Dump(stringBuilder, o);
            }
        }

        /// <inheritdoc />
        public abstract IEnumerable<object?> GetElementaries();

        /// <inheritdoc />
        public void Dispose()
        {
            _disposeAction(this);
        }

    }
}