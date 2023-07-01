using System;

namespace Cr7Sund
{
    public class ButtonAttribute : Attribute
    {
        public string labelName;

        public ButtonAttribute(string _labelName)
        {
            this.labelName = _labelName;
        }
    }
}