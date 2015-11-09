using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;

namespace MovieMaster.Objects
{
    public class MovieMetaData : DatabaseObject
    {
        [PrimaryKey]
        public int MovieMetaDataID { get; set; }        
        public int MovieID { get; set; }
        public string Value { get; set; }
    }
}
