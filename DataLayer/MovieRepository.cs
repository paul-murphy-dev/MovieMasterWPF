using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.Objects;
using System.Reflection;
using System.Windows.Media;
using System.Data;

namespace MovieMaster.DataLayer
{
    public static class MovieRepository
    {
        private static LocalStore _movieData = new LocalStore();

        static MovieRepository()
        {
         
        }

        public static void Initialize()
        {
            if (System.IO.File.Exists("dbdata.xml"))
            {
                _movieData.BeginInit();
                _movieData.ReadXml("dbdata.xml");
                _movieData.EndInit();
                _movieData.AcceptChanges();
            }
        }

        public static void ClearRepository()
        {
            _movieData.ImageListTable.Rows.Clear();
            _movieData.MovieFileTable.Rows.Clear();
            _movieData.MovieTable.Rows.Clear();
            _movieData.ImagesTable.Rows.Clear();
            _movieData.AcceptChanges();
        }

        public static void Shutdown()
        {
            _movieData.WriteXml("dbdata.xml");
        }

        public static Movie GetMovieByPath(string path)
        {
            var result = _movieData.MovieFileTable.Where(a => a.LocalPath == path.Trim()).FirstOrDefault();

            if (result != null)
            {
                var mov = _movieData.MovieTable.FirstOrDefault(a => a.MovieID == result.MovieID);
                Movie m = new Movie();
                m.Populate(new DataResult() { FieldNames = _movieData.MovieTable.GetFields(), Values = mov.ItemArray });
                return m;
            }

            return null;
        }

        public static Movie GetMovieByName(string name)
        {
            var result = _movieData.MovieTable.Where(a => a.Name.Contains(name)).ToList();
            if (result != null && result.Any())
            {
                Movie m = new Movie();
                m.Populate(new DataResult() { FieldNames = _movieData.MovieTable.GetFields(), Values = result.First().ItemArray });
                return m;
            }

            return null;
        }

        public static Movie GetMovieByID(int id)
        {
            var result = _movieData.MovieTable.FirstOrDefault(a => a.MovieID == id);
            if (result != null)
            {
                Movie m = new Movie();
                m.Populate(new DataResult() { FieldNames = _movieData.MovieTable.GetFields(), Values = result.ItemArray });
                return m;
            }

            return null;
        }

        public static void UpdateMovie(Movie movie)
        {
            try
            {
                var result = _movieData.MovieTable.FirstOrDefault(a => a.MovieID == movie.MovieID);
                if (result != null)
                {
                    result.Year = movie.Year;
                    result.Name = movie.Name;
                    result.GenreString = movie.GenreString;
                    result.Rating = movie.Rating;
                    result.UserRating = movie.UserRating;
                    result.TimesPlayed = movie.TimesPlayed;
                    result.ImageListID = movie.ImageListID;
                    result.Description = movie.Description;
                }
                result.AcceptChanges();
                _movieData.AcceptChanges();
            }
            catch { }
        }

        public static void CreateMovie(Movie movie)
        {
            var movieRow = (LocalStore.MovieTableRow)_movieData.MovieTable.NewRow();
            movieRow.BeginEdit();
            movieRow.GenreListID = movie.GenreListID;
            movieRow.ImageListID = movie.ImageListID;
            movieRow.Name = movie.Name;
            movieRow.Year = movie.Year;
            movieRow.EndEdit();
            _movieData.MovieTable.Rows.Add(movieRow);
            _movieData.MovieTable.AcceptChanges();

            var movieFileRow = (LocalStore.MovieFileTableRow)_movieData.MovieFileTable.NewRow();
            movieFileRow.MovieID = movieRow.MovieID;
            movieFileRow.LocalPath = movie.LocalPath;
            _movieData.MovieFileTable.Rows.Add(movieFileRow);
            _movieData.MovieFileTable.AcceptChanges();

            movie.MovieID = movieRow.MovieID;
        }

        public static int AddMovieImage(int MovieID, string imagePath)
        {
            try
            {
                var imageRow = (LocalStore.ImagesTableRow)_movieData.ImagesTable.NewRow();
                imageRow.ImageLocation = imagePath;
                _movieData.ImagesTable.Rows.Add(imageRow);
                _movieData.ImagesTable.AcceptChanges();

                var ImageListRow = (LocalStore.ImageListTableRow)_movieData.ImageListTable.NewRow();
                ImageListRow.MovieID = MovieID;
                ImageListRow.ImageID = imageRow.ImageID;
                _movieData.ImageListTable.Rows.Add(ImageListRow);
                _movieData.ImageListTable.AcceptChanges();
                return ImageListRow.ImageListID;
            }
            catch { }
            return -1;
        }

        public static void AddMovieMetaData(int movieID, List<MovieMetaData> metadata)
        {
            foreach (var item in metadata)
            {
                item.MovieID = movieID;

                var metaDataRow = (LocalStore.MovieMetadataTableRow)_movieData.MovieMetadataTable.NewRow();
                metaDataRow.MovieID = movieID;
                metaDataRow.Value = item.Value;
                _movieData.MovieMetadataTable.Rows.Add(metaDataRow);
            }
            _movieData.MovieMetadataTable.AcceptChanges();
        }

        public static List<MovieMetaData> GetMovieMetaData(int movieID)
        {
            var results = (List<LocalStore.MovieMetadataTableRow>)_movieData.MovieMetadataTable.Where(a => a.MovieID == movieID).ToList();
            if (results != null && results.Any())
            {
                var metadata = results.Select(a => new MovieMetaData() { MovieID = a.MovieID, Value = a.Value }).ToList();
                return metadata;
            }
            return null;
        }

        public static System.Windows.Media.ImageSource GetMovieImage(int movieID)
        {
            var imgReslt = (from imageList in _movieData.ImageListTable
                            join images in _movieData.ImagesTable on imageList.ImageID equals images.ImageID
                            where imageList.MovieID == movieID
                            select new MovieImage()
                            {
                                ImageID = images.ImageID,
                                ImageLocation = images.ImageLocation
                            }).FirstOrDefault();

            if (imgReslt != null)
            {
                if (System.IO.File.Exists(imgReslt.ImageLocation))
                {
                    System.Uri fileURI = new Uri(System.IO.Path.GetFullPath(imgReslt.ImageLocation));
                    System.Windows.Media.Imaging.BitmapImage bmpImg = new System.Windows.Media.Imaging.BitmapImage(fileURI);
                    return bmpImg;
                }
            }

            return null;
        }

        public static void DeleteMovie(Movie movie)
        {
            var result = _movieData.MovieTable.FirstOrDefault(a => a.MovieID == movie.MovieID);
            if (result != null)
                result.Delete();
            _movieData.AcceptChanges();
        }

        public static List<Genre> ListGenresForMovie(int MovieID)
        {
            var movie = _movieData.MovieTable.FirstOrDefault(a => a.MovieID == MovieID);
            if (movie != null)
            {
                var genres = (from gl in _movieData.GenreListTable
                              join g in _movieData.GenreTable on gl.GenreID equals g.GenreID
                              select new Genre()
                              {
                                  GenreID = g.GenreID,
                                  Name = g.Name
                              }).ToList();

                return genres;
            }

            return null;
        }

        public static IList<Movie> ListAllMovies()
        {
            var results = (from movies in _movieData.MovieTable
                           join files in _movieData.MovieFileTable on movies.MovieID equals files.MovieID
                           select new Movie()
                           {
                               MovieID = movies.MovieID,
                               GenreListID = movies.GenreListID,
                               ImageListID = movies.ImageListID,
                               Rating = movies.Rating,
                               UserRating = movies.UserRating,
                               TimesPlayed = movies.TimesPlayed,
                               LocalPath = files.LocalPath,
                               GenreString = movies.GenreString,
                               Description = movies.Description,
                               Name = movies.Name,
                               Year = movies.Year
                           }).OrderBy(a => a.Name).ToList();

            return results;
        }

        public static void UpdateMovieImage(Movie mov, string ImagePath)
        {
            mov.ImageListID = MovieRepository.AddMovieImage(mov.MovieID, ImagePath);
            MovieRepository.UpdateMovie(mov);
        }

        public static void DeleteImageForMovie(int movieID)
        {
            var imgReslt = (from imageList in _movieData.ImageListTable
                            where imageList.MovieID == movieID
                            select imageList).FirstOrDefault();
            if (imgReslt != null)
            {
                var image = (from images in _movieData.ImagesTable
                             where images.ImageID == imgReslt.ImageID
                             select images).FirstOrDefault();


                imgReslt.Delete();
                image.Delete();
                _movieData.AcceptChanges();
            }
        }
    }
}
