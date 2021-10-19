
using System;
using System.Reflection;
using Gedoc.Helpers.Enum;

namespace Gedoc.Helpers
{
    public static class ExtentionClass
    {
        public static string GetDescription(this System.Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            // Get the stringvalue attributes  
            EnumDescriptionAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(EnumDescriptionAttribute), false) as EnumDescriptionAttribute[];
            // Return the first if there was a match.  
            return attribs.Length > 0 ? attribs[0].StringValue : value.ToString();
        }
    }
}
