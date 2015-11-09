using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MovieMaster.DataLayer
{
    public static class DatatableExtensions
    {
        public static string[] GetFields(this System.Data.DataTable dt)
        {
            string[] fields = new string[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                fields[i] = dt.Columns[i].ColumnName;
            }
            return fields;
        }        
    }
}
