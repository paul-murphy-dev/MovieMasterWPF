using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MovieMaster.DataLayer
{
    interface IPopulate
    {
        void Populate(DataResult dr);

        PropertyInfo GetPrimaryKey();
    }
}
