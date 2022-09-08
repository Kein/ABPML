using ABPMLManager.Providers.INI;
using ABPMLManager.Model;
using System.Reflection;

namespace ABPMLManager.Extensions
{
    public static class ReflectionHelper
    {
        public static bool IniSectionToObject<T>(List<IniKeyValue>? kvProps, out T instance)
        {
            bool result = false;
            instance = Activator.CreateInstance<T>();
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields != null && kvProps != null && fields.Length > 0 && kvProps.Count > 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    string fName = fields[i].Name;
                    var fieldType = fields[i].FieldType;
                    for (int j = 0; j < kvProps.Count; j++)
                    {
                        string propName = kvProps[j].Key;
                        string propVal = kvProps[j].Value;
                        if (string.Equals(fName, propName, StringComparison.OrdinalIgnoreCase))
                        {
                            result = true;
                            var targetVal = StringToTypeValue(propVal, fieldType);
                            if (targetVal != null)
                            {
                                if (fieldType.IsArray)
                                    fields[i].SetValue(instance, IniPropToArrayObject(fName, kvProps, fieldType));
                                else
                                    fields[i].SetValue(instance, targetVal);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static object? StringToTypeValue(string value, Type type) => type switch
        {
            _ when type == typeof(string) => value,
            _ when type == typeof(char) => value[0],
            _ when type == typeof(byte) => byte.TryParse(value, out byte result) ? result: null,
            _ when type == typeof(int) => int.TryParse(value, out int result) ? result: null,
            _ when type == typeof(float) => float.TryParse(value, out float result) ? result: null,
            _ when type == typeof(bool) => bool.TryParse(value, out bool result) ? result: null,
            _ when type.BaseType == typeof(Enum) => Enum.TryParse(type, value, true, out object? result) ? result: null,
            _ => null
        };

        private static object? IniPropToArrayObject(string fieldName, List<IniKeyValue> kvProps, Type arrayType)
        {
            var array = Array.CreateInstance(typeof(object), 0);
            array = null;
            Type? elemType = arrayType.GetElementType();
            if (elemType != null)
            {
                List<object> temp = new List<object>();
                foreach (var entry in kvProps)
                {
                    if (!string.Equals(entry.Key, fieldName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var val = StringToTypeValue(entry.Value, elemType);
                    if (val != null)
                        temp.Add(entry.Value);
                }
                if (temp.Count > 0)
                {
                    array = Array.CreateInstance(elemType, temp.Count);
                    for (int i = 0; i < array.Length; i++)
                        array.SetValue(temp[i], i);
                }
            }

            return array;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Type used as default when no instance supplied</typeparam>
        /// <param name="instance">Can be null, default will be created and ini section populated with defaults</param>
        /// <param name="sectionName">Name of the section associated with T</param>
        /// <returns></returns>
        public static List<IniKeyValue>? ObjectToIniSection<T>(object? instance, out string sectionName)
        {
            sectionName = string.Empty;
            List<IniKeyValue>? result = null;
            instance = instance ?? Activator.CreateInstance(typeof(T));
            IniSectionAttribute? attr = typeof(T).GetCustomAttribute<IniSectionAttribute>();
            if (instance != null && !string.IsNullOrEmpty(attr?.SectionName))
            {
                sectionName = attr.SectionName;
                var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
                if (fields != null && fields.Length > 0)
                {
                    result = new List<IniKeyValue>(fields.Length);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        string propName = fields[i].Name;
                        Type fieldType = fields[i].FieldType;
                        object? val = fields[i].GetValue(instance);
                        if (!fieldType.IsArray && val != null)
                            result.Add(new IniKeyValue(propName, val.ToString()!));
                        else if (fieldType.IsArray)
                            result.AddRange(ArrayToIniSection(val, propName, fieldType));
                    }
                

                }
            }
            return result;
        }

        private static List<IniKeyValue> ArrayToIniSection(object? arrayObject, string propName, Type fieldType)
        {
            List<IniKeyValue> result = new();
            Type? elemType = fieldType?.GetElementType();
            if (elemType != null && arrayObject is Array ar && ar.Length > 0)
            {
                foreach (var item in ar)
                {
                    string? str = item.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(str) && (item.GetType().IsPrimitive || item is string))
                        result.Add(new IniKeyValue(propName, str));
                }
            }
            return result;
        }
    }
}
