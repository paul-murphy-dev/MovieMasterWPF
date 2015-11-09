using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovieMaster.MovieDbAPI
{
    public class MovieSearchInfo
    {
        public MovieSearchInfo()
        {

        }

        public bool adult { get; set; }

        public string backdrop_path { get; set; }

        public int id { get; set; }

        public string original_title { get; set; }

        public DateTime? release_date { get; set; }

        public string poster_path { get; set; }

        public double popularity { get; set; }

        public string title { get; set; }

        public double vote_average { get; set; }

        public int vote_count { get; set; }

        public override string ToString()
        {
            if (release_date.HasValue)
                return string.Format("{0} ({1})", title, release_date.Value.Year.ToString());
            else
                return this.title;
        }
    }
}
