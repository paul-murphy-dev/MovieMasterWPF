using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.DataLayer
{
    [System.AttributeUsage(AttributeTargets.Class)]
    public class TableMapping : Attribute
    {
        public string Name { get; set; }

        public TableMapping(string name)
        {
            Name = name;
        }
    }
}
