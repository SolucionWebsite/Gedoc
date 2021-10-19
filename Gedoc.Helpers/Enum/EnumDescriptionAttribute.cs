using System;

namespace Gedoc.Helpers.Enum
{
    public class EnumDescriptionAttribute : Attribute
    {
        public EnumDescriptionAttribute(string stringValue)
        {
            this.stringValue = stringValue;
        }

        private string stringValue;
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}