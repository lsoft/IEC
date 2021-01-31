using System;
using System.Collections.Concurrent;
using System.Linq;
using IEC.Common.Other;

namespace IEC.Common.Scope
{
    internal class ScopeFactory : IScopeFactory
    {
        private readonly Action<IThreadFrame> _disposeAction;
        private readonly Type _taitf = typeof(Action<IThreadFrame>);

        private readonly ConcurrentDictionary<Type, bool> _constructorCache = new();

        public ScopeFactory(
            Action<IThreadFrame> disposeAction
            )
        {
            if (disposeAction == null)
            {
                throw new ArgumentNullException(nameof(disposeAction));
            }

            _disposeAction = disposeAction;
        }


        public T CreateMutableScope<T>(
            )
            where T : MutableScope
        {
            if (!CheckForConstructors<T>())
            {
                throw new InvalidOperationException($"Type {ReflectionHelper.GetHumanReadableTypeName(typeof(T))} should contains only one constructor with only one parameter of type {ReflectionHelper.GetHumanReadableTypeName(_taitf)}");
            }

            return (T)Activator.CreateInstance(
                typeof(T),
                _disposeAction
                )!;
        }

        private bool CheckForConstructors<T>()
            where T : MutableScope
        {
            var result = _constructorCache.GetOrAdd(
                typeof(T),
                (typeoft) =>
                {
                    var constructors = typeoft.GetConstructors();

                    if (constructors.Length != 1)
                    {
                        return false;
                    }

                    var constructor = constructors[0];

                    var parameters = constructor.GetParameters();

                    if (parameters.Length != 1)
                    {
                        return false;
                    }

                    var parameter = parameters[0];

                    if (
                        parameter.ParameterType != _taitf
                        && !parameter.ParameterType.IsSubclassOf(_taitf)
                        )
                    {
                        return false;
                    }

                    return true;
                }
                );

            return result;
        }

        public ImmutableScope CreateImmutableScope(
            params object?[] objects
            )
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            return new ImmutableScope(
                _disposeAction,
                objects
                );
        }
    }
}