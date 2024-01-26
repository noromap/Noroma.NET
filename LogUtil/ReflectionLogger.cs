using System;
using System.Collections;
using System.Reflection;

namespace Noroma.LogUtil
{
    public class ReflectionLogger
    {
        public static void LogProperties(object obj, Action<string> logger, string indent = "")
        {
            if (obj == null)
            {
                logger($"{indent}null");
                return;
            }

            Type type = obj.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                object value = property.GetValue(obj);

                if (value is IEnumerable enumerable && !(value is string))
                {
                    LogEnumerable(property.Name, enumerable, logger, indent + "  ");
                }
                else
                {
                    logger($"{indent}{property.Name}: {value}");
                    if (value != null && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
                    {
                        LogProperties(value, logger, indent + "  ");
                    }
                }
            }
        }

        private static void LogEnumerable(string propertyName, IEnumerable enumerable, Action<string> logger, string indent)
        {
            logger($"{indent}{propertyName}:");

            int index = 0;
            foreach (var item in enumerable)
            {
                if (item is IEnumerable subEnumerable && !(item is string))
                {
                    LogEnumerable($"Index {index}", subEnumerable, logger, indent + "  ");
                }
                else
                {
                    logger($"{indent}  Index {index}: {item}");
                    if (item != null && !item.GetType().IsPrimitive && item.GetType() != typeof(string))
                    {
                        LogProperties(item, logger, indent + "    ");
                    }
                }

                index++;
            }
        }
    }
}