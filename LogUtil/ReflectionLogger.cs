using System;
using System.Collections;
using System.Reflection;

namespace Noroma.LogUtil
{
	public class ReflectionLogger
	{
		/// <summary>
		/// 最大出力回数
		/// </summary>
		public static int MAX_LOG_COUNT = 100;

		public static void LogProperties(object obj, Action<string> logger, bool logStruct = false, string indent = "")
		{
			if (obj == null) return;
			int count = 0;
			bool result = _LogProperties(obj, logger, logStruct, indent, ref count);
			if (false == result)
			{
				logger("再帰上限回数に到達したため、出力を終了します。");
			}
		}

		private static bool _LogProperties(object obj, Action<string> logger, bool logStruct, string indent, ref int currentRecursive)
		{
			if (obj == null) return true;
			if (currentRecursive >= MAX_LOG_COUNT)
			{
				return false;
			}
			currentRecursive++;

			if (obj == null)
			{
				logger($"{indent}null");
				return true;
			}

			Type type = obj.GetType();

			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object value = null;
				try
				{
					value = field.GetValue(obj);
				}
				catch
				{
					currentRecursive++;
					if (currentRecursive >= MAX_LOG_COUNT) return false;
					continue;
				}
				if (value == null) continue;
				if (value is IEnumerable enumerable && !(value is string))
				{
					if (false == _LogEnumerable(field.Name, enumerable, logger, logStruct, indent + "  ", ref currentRecursive)) return false;
				}
				else
				{
					logger($"{indent}{field.Name}: {value}");
					currentRecursive++;
					if (value != null && !field.FieldType.IsPrimitive && field.FieldType != typeof(string))
					{
						if (false == _LogProperties(value, logger, logStruct, indent + "  ", ref currentRecursive)) return false;
					}
				}
			}

			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				object value = null;
				try
				{
					value = property.GetValue(obj);
				}
				catch
				{
					currentRecursive++;
					if (currentRecursive >= MAX_LOG_COUNT) return false;
					continue;
				}
				if (value == null) continue;
				if (value is IEnumerable enumerable && !(value is string))
				{
					if (false == _LogEnumerable(property.Name, enumerable, logger, logStruct, indent + "  ", ref currentRecursive)) return false;
				}
				else
				{
					logger($"{indent}{property.Name}: {value}");
					currentRecursive++;
					if (value != null && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
					{
						if (false == _LogProperties(value, logger, logStruct, indent + "  ", ref currentRecursive)) return false;
					}
				}
			}
			return true;
		}

		private static bool _LogEnumerable(string propertyName, IEnumerable enumerable, Action<string> logger, bool logStruct, string indent, ref int currentRecursive)
		{
			if (enumerable == null) return true;
			if (currentRecursive >= MAX_LOG_COUNT)
			{
				return false;
			}
			currentRecursive++;

			logger($"{indent}{propertyName}:");

			int index = 0;
			foreach (var item in enumerable)
			{
				if (item == null)
				{
					logger($"{indent}  Index {index}: (NULL)");
				}
				if (item is IEnumerable subEnumerable && !(item is string))
				{
					if (_LogEnumerable($"Index {index}", subEnumerable, logger, logStruct, indent + "  ", ref currentRecursive) == false) return false;
				}
				else
				{
					logger($"{indent}  Index {index}: {item}");
					if (item != null && !item.GetType().IsPrimitive && item.GetType() != typeof(string))
					{
						if (_LogProperties(item, logger, logStruct, indent + "    ", ref currentRecursive) == false) return false;
					}
				}

				index++;
			}
			return true;
		}
	}
}