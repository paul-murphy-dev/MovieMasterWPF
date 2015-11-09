using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using MovieMaster.DataLayer;

namespace MovieMaster.Objects
{
    public class Movie: DatabaseObject
    {
        public Movie()
        {
            this.Name = string.Empty;
            this.Year = -1;
            this.LocalPath = string.Empty;
            this.Description = string.Empty;
            //Genres = new List<Genre>();            
        }

        [PrimaryKey]
        public int MovieID { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        [Ignore]
        public string LocalPath { get; set; }        
        public int internetRating { get; set; }
        public string Description { get; set; }
        public int GenreListID { get; set; }
        public int ImageListID { get; set; }
        public string GenreString { get; set; }
        [Ignore]
        public BitmapImage Image { get; set; }

        public double Rating { get; set; }
        public double UserRating { get; set; }
        public int TimesPlayed { get; set; }

        public void Reset()
        {
            this.Image = null;
            this.internetRating = 0;
            this.UserRating = 0;            
            this.GenreString = string.Empty;
            this.GenreListID = -1;
            this.Description = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Year);
        }
    }
}
