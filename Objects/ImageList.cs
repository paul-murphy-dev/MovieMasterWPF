using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;

namespace MovieMaster.Objects
{
    public class ImageList:DatabaseObject
    {
        [PrimaryKey]
        public int ImageListID { get; set; }
        public int ImageID { get; set; }
        public int MovieID { get; set; }
    }
}
