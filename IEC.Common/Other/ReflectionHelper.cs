using System;
using System.Linq;
using System.Reflection;

namespace IEC.Common.Other
{
    public static class ReflectionHelper
    {
        public static string GetHumanReadableTypeName(
            object? o,
            string memberTypeName
            )
        {
            if (o is not null)
            {
                return GetHumanReadableTypeName(o.GetType());
            }

            if (string.IsNullOrEmpty(memberTypeName))
            {
                return "(unknown)";
            }

            return memberTypeName;
        }

        internal static string GetHumanReadableTypeName(
            Type? type
            )
        {
            if (type is null)
            {
                return "(unknown)";
            }

            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType)
            {
                return type.FullName!;
            }

            var builder = new System.Text.StringBuilder();

            var name = type.Name;
            var index = name.IndexOf("`", StringComparison.Ordinal);
            builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));

            builder.Append('<');

            var gargs = string.Join(",", type.GetGenericArguments().Select(ga => GetHumanReadableTypeName(ga)));
            builder.Append(gargs);

            builder.Append('>');

            return builder.ToString();
        }

        internal static object? DetermineValue(
            object o,
            FieldInfo? fieldInfo,
            PropertyInfo? propertyInfo
            )
        {
            object? value;
            try
            {
                value = fieldInfo != null
                        ? fieldInfo.GetValue(o)
                        : propertyInfo!.GetValue(o, null)
                    ;
            }
            catch (Exception excp)
            {
                var innerExceptionData = 
                    excp.InnerException != null
                        ? string.Format(
                            ", InnerException={{{0}: \"{1}\"}}",
                            excp.InnerException.GetType().Name,
                            excp.InnerException.Message
                            )
                        : string.Empty
                        ;

                value = $"{excp.GetType().Name}: \"{excp.Message}\"{innerExceptionData}";
            }

            return value;
        }

    }
}
