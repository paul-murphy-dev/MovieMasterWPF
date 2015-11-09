using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.MovieDbAPI
{
    public class CrewMember
    {
        public int id { get; set; }
        public string name { get; set; }
        public string department { get; set; }
        public string job { get; set; }
        public string profile_path { get; set; }        
    }
}
