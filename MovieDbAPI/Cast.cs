using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.MovieDbAPI
{
    public class MovieCast
    {
        public int id { get; set; }
        List<CastMember> cast { get; set; }
    }
}
