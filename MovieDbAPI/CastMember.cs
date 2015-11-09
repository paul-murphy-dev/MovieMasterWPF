using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.MovieDbAPI
{
    public class CastMember
    {
        public int id { get; set; }
        public string name { get; set; }
        public string character { get; set; }
        public int order { get; set; }
        public int cast_id { get; set; }
        public string profile_path { get; set; }
        public string department { get; set; }
        public string job { get; set; }
    }
}
