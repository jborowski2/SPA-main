using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_main.Model
{
    public class Attribute
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public Attribute(string PropertyName, string PropertyValue)
        {
            this.PropertyName = PropertyName;
            this.PropertyValue = PropertyValue;
        }
    }
}
