using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;

namespace MovieMaster.Objects
{
    [TableMapping("Images")]
    public class MovieImage:DatabaseObject
    {
        [PrimaryKey]
        public int ImageID { get; set; }
        public string ImageLocation { get; set; }
    }
}
