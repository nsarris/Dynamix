using System;

namespace Dynamix
{
    public class OrderItem
    {
        public OrderItem(string propertyName, bool isDescending)
        {
            PropertyName = propertyName;
            IsDescending = isDescending;
        }

        public OrderItem(string propertyName, string direction)
        {
            PropertyName = propertyName;
            IsDescending = (StringComparer.OrdinalIgnoreCase.Compare(direction, "DESC") == 0);
        }

        public string PropertyName { get; set; }
        public bool IsDescending { get; set; }
    }

}
