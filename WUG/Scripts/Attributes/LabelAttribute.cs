using System;

namespace Cr7Sund
{
    internal class LabelAttribute : Attribute
    {
        private string label;

        public LabelAttribute(string _label)
        {
            this.label = _label;
        }
    }
}