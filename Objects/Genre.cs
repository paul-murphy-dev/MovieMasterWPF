using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;

namespace MovieMaster.Objects
{
    public class Genre : DatabaseObject
    {
        [PrimaryKey]
        public int GenreID { get; set; }
        public virtual string Name { get; set; }
    }
}
