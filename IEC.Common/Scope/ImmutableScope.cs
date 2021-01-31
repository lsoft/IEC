using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

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
        public override IReadOnlyCollection<object?> GetElementaries()
        {
            var result = new List<object?>();

            foreach (var p in _properties)
            {
                
                try
                {
                    result.Add(p.GetValue(this));
                }
                catch
                {
                    //unknown error, we can perform nothing here
                    //probably only-setter property here (object SomeProperty { set {...} })
                    //or property code raised an exception
                    //who knows...
                    //just skip this property
                }
            }

            return result;
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
        public override IReadOnlyCollection<object?> GetElementaries()
        {
            return Objects;

            //foreach (var o in Objects)
            //{
            //    yield return o;
            //}
        }
    }
}
