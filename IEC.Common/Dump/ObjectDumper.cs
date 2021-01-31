using System;
using System.Collections;
using System.Reflection;
using System.Text;
using IEC.Common.Other;

namespace IEC.Common.Dump
{
    public class ObjectDumper
    {
        private readonly int _maxDepth;
        private readonly int _maxCollectionCount;

        public ObjectDumper(
            int maxDepth,
            int maxCollectionCount
            )
        {
            _maxDepth = maxDepth;
            _maxCollectionCount = maxCollectionCount;
        }

        public void Dump(
            StringBuilder sb,
            object? o
            )
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            DumpAnyObject(sb, o, string.Empty, string.Empty, 1);
        }

        private void DumpAnyObject(
            StringBuilder sb,
            object? o,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (o is null)
            {
                DumpNull(sb, memberName, memberTypeName, level);

                return;
            }

            if (o is string)
            {
                DumpRaw(sb, o, memberName, memberTypeName, level);

                return;
            }

            if (o is DateTime odt)
            {
                DumpDateTime(sb, odt, memberName, memberTypeName, level);

                return;
            }

            if (o is ValueType)
            {
                DumpRaw(sb, o, memberName, memberTypeName, level);

                return;
            }

            if (level < _maxDepth)
            {
                if (o is IEnumerable e)
                {
                    DumpEnumerable(sb, e, memberName, memberTypeName, level);

                    return;
                }

                DumpComplexObject(sb, o, memberName, memberTypeName, level);
            }
            else
            {

                DumpStop(sb, o, memberName, memberTypeName, level);

                //AppendMessage(
                //    sb,
                //    $"{{...}}",
                //    level
                //    );

                //DumpRaw(sb, "{...}", string.Empty, string.Empty, level);
            }
        }

        private void DumpEnumerable(
            StringBuilder sb,
            IEnumerable o,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (string.IsNullOrEmpty(memberName))
            {
                AppendMessage(
                    sb,
                    $"{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)}:{{",
                    level
                    );
            }
            else
            {
                AppendMessage(
                    sb,
                    $"{memberName}:{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)}:{{",
                    level
                    );
            }

            var index = 0;
            foreach (var ee in o)
            {
                if (index >= _maxCollectionCount)
                {
                    break;
                }

                DumpAnyObject(
                    sb,
                    ee,
                    string.Empty,
                    string.Empty,
                    level + 1
                    );
            }
            
            AppendMessage(
                sb,
                $"}}",
                level
                );
        }

        private void DumpComplexObject(
            StringBuilder sb,
            object o,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (string.IsNullOrEmpty(memberName))
            {
                AppendMessage(
                    sb,
                    $"{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)}:{{",
                    level
                    );
            }
            else
            {
                AppendMessage(
                    sb,
                    $"{memberName}:{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)}:{{",
                    level
                    );
            }

            var members = o.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in members)
            {
                var fieldInfo = m as FieldInfo;
                var propertyInfo = m as PropertyInfo;

                if (fieldInfo == null && propertyInfo == null)
                {
                    continue;
                }

                string memberType = ReflectionHelper.GetHumanReadableTypeName(
                    fieldInfo != null
                        ? fieldInfo.FieldType
                        : propertyInfo!.PropertyType
                    );

                var value = ReflectionHelper.DetermineValue(o, fieldInfo, propertyInfo);

                DumpAnyObject(
                    sb, 
                    value, 
                    m.Name,
                    ReflectionHelper.GetHumanReadableTypeName(value, memberType),
                    level + 1
                    );
            }

            AppendMessage(
                sb,
                $"}}",
                level
                );
        }

        private void DumpDateTime(
            StringBuilder sb,
            DateTime o,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (string.IsNullOrEmpty(memberName))
            {
                AppendMessage(
                    sb,
                    $"{ReflectionHelper.GetHumanReadableTypeName(o.GetType())} = \"{o:dd.MM.yyyy HH:mm:ss.fff}\"",
                    level
                    );
                return;
            }

            AppendMessage(
                sb,
                $"{memberName}:{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)} = \"{o:dd.MM.yyyy HH:mm:ss.fff}\"",
                level
                );
        }

        private void DumpStop(
            StringBuilder sb,
            object? o,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (string.IsNullOrEmpty(memberName))
            {
                AppendMessage(
                    sb,
                    $"{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)} = \"{{...}}\"",
                    level
                    );
                return;
            }

            AppendMessage(
                sb,
                $"{memberName}:{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)} = \"{{...}}\"",
                level
                );
        }

        private void DumpRaw(
            StringBuilder sb,
            object o,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (string.IsNullOrEmpty(memberName))
            {
                AppendMessage(
                    sb,
                    $"{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)} = \"{o}\"",
                    level
                    );
                return;
            }

            AppendMessage(
                sb,
                $"{memberName}:{ReflectionHelper.GetHumanReadableTypeName(o, memberTypeName)} = \"{o}\"",
                level
                );
        }

        private void DumpNull(
            StringBuilder sb,
            string memberName,
            string memberTypeName,
            int level
            )
        {
            if (string.IsNullOrEmpty(memberName))
            {
                AppendMessage(sb, $"{ReflectionHelper.GetHumanReadableTypeName(null, memberTypeName)} = null", level);
                return;
            }

            AppendMessage(sb, $"{memberName}:{ReflectionHelper.GetHumanReadableTypeName(null, memberTypeName)} = null", level);
        }


        private void AppendMessage(
            StringBuilder sb,
            string message,
            int level
            )
        {
            sb.Append(new string(' ', level * 2));
            sb.AppendLine(message);
        }

    }
}
