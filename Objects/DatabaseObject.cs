using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;
using System.Reflection;

namespace MovieMaster.Objects
{
    public class DatabaseObject: IPopulate
    {
        #region IPopulate Members

        public virtual void Populate(DataResult dr)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            List<string> fields = dr.FieldNames.ToList();
            foreach (PropertyInfo p in props)
            {
                //does a name of any of the properties match?
                if (dr.FieldNames.Any(name => name.Equals(p.Name)))
                {
                    int idx = fields.IndexOf(p.Name);
                    object value = dr.Values[idx];
                    if (value is DBNull)
                        value = null;

                    if (value is string)
                        value = value.ToString();
                    
                    p.SetValue(this, value, null);
                    continue;
                }

                object[] customAttributes = p.GetCustomAttributes(false);
                if (customAttributes != null && customAttributes.Any())
                {
                    object columnAttribute = customAttributes.Where(att => att is ColumnMapping).FirstOrDefault();
                    if (columnAttribute != null && columnAttribute is ColumnMapping)
                    {
                        int idx = fields.IndexOf(((ColumnMapping)columnAttribute).Name);
                        object value = dr.Values[idx];
                        if (value is string)
                            value = value.ToString().Trim();
                        p.SetValue(this, dr.Values[idx], null);
                    }
                }
            }
        }
        
        public PropertyInfo GetPrimaryKey()
        {
            PropertyInfo[] props = this.GetType().GetProperties();

            foreach (var p in props)
            {
                var customAtts = p.GetCustomAttributes(false);
                if (customAtts.Any(att => att is PrimaryKey))
                {
                    return p;
                }
                if (customAtts.Any(att => att is PrimaryKeyForeignKey))
                {
                    return p;
                }
            }

            return null;
        }

        #endregion
    }
}
