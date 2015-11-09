using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;

namespace MovieMaster.Objects
{
    public class MovieFile : DatabaseObject
    {
        [PrimaryKeyForeignKey]
        public int MovieID { get; set; }
        public string LocalPath { get; set; }
    }
}
