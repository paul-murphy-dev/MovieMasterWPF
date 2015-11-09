using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.DataLayer
{
    [System.AttributeUsage(AttributeTargets.Property)]
    public class ColumnMapping : Attribute
    {
        public string Name { get; set; }

        public ColumnMapping(string name)
        {
            Name = name;
        }
    }
}
