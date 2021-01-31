using System;
using System.Collections.Generic;
using System.Reflection;

namespace IEC.Common.Scope
{

    public abstract class MutableScope : Scope
    {
        private readonly PropertyInfo[] _properties;

        /// <inheritdoc />
        protected MutableScope(
            Action<IThreadFrame> disposeAction
            ) : base(disposeAction)
        {
            _properties = this.GetType().GetProperties();
        }

        /// <inheritdoc />
        public override IEnumerable<object?> GetElementaries()
        {
            foreach (var p in _properties)
            {
                yield return p.GetValue(this);
            }
        }

    }


    public class ImmutableScope : Scope
    {
        public object?[] Objects
        {
            get;
        }

        /// <inheritdoc />
        public ImmutableScope(
            Action<IThreadFrame> disposeAction,
            params object?[] objects
            ) : base(disposeAction)
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            Objects = objects;
        }

        /// <inheritdoc />
        public override IEnumerable<object?> GetElementaries()
        {
            foreach (var o in Objects)
            {
                yield return o;
            }
        }
    }
}
