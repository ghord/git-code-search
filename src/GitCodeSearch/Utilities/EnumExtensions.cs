using System;
using System.ComponentModel;
using System.Linq;

namespace GitCodeSearch.Utilities
{
    internal static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            return enumValue.GetAttribute<DescriptionAttribute>().Description;
        }

        private static T GetAttribute<T>(this Enum enumValue) where T : Attribute
        {
            Type enumType = enumValue.GetType();
            var fieldInfo = enumType.GetField(enumValue.ToString());
            if (fieldInfo == null)
            {
                throw new Exception($"Can not get FieldInfo for {enumType.Name}.{enumValue}");
            }

            var attributes = (T[])fieldInfo.GetCustomAttributes(typeof(T), false);
            if (!attributes.Any())
            {
                throw new Exception($"{enumType.Name}.{enumValue} do not have {typeof(T).Name} attribute");
            }

            return attributes.First();
        }
    }
}
