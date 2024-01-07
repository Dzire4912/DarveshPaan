using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class EnumHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static string FormatEnumDescription<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            string enumDescription = EnumHelper.GetEnumDescription(enumValue);

            // Ensure the description is not empty
            if (!string.IsNullOrEmpty(enumDescription))
            {
                // Capitalize the first character and convert the rest to lowercase
                return char.ToUpper(enumDescription[0]) + enumDescription.Substring(1).ToLower();
            }

            return enumDescription;
        }
    }
}
