using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.Helpers;
using System.IO;
using System.Net;
using System.Web;
using System.Reflection;

namespace MovieMaster.MovieDbAPI
{
    public static class MovieDbAPI
    {
        private const string API_KEY = "GetYourOwnKey@TheMoviedb.org";
        private const string GetMovieById = "http://api.themoviedb.org/3/movie/{0}?api_key={1}";
        private const string MovieSearchByNameURL = "http://api.themoviedb.org/3/search/movie?api_key={0}&query={1}";
        private const string GetImageURL = "http://cf2.imgobject.com/t/p/w185{0}";
        private const string GetCastURL = "http://api.themoviedb.org/3/movie/{0}/casts?api_key={1}";

        public static MovieInfo FetchMovieData(string movieName, int Year)
        {
            string searchTerm = movieName;
            string[] removeList = new string[]{",","-",":"};
            foreach (var thing in removeList)
            {
                searchTerm = searchTerm.Replace(".", " ");
            }
            searchTerm = searchTerm.Replace(" ", "%20");
            string url = string.Format(MovieSearchByNameURL, API_KEY, HttpUtility.HtmlEncode(searchTerm));
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("Accept:application/json");
            string data = wc.DownloadString(url);

            var apage = JSONConverter.Convert<page>(data);


            //Look for an exact match on the name and the year
            var exactMatch = apage.results.FirstOrDefault(mov => mov.title.ToLower() == movieName.ToLower() && mov.release_date.HasValue && mov.release_date.Value.Year == Year);
            if (exactMatch != null || apage.results.Count == 1)
            {
                if (apage.results.Count == 1)
                    exactMatch = apage.results.First();
                return FetchSpecificMovieData(wc, exactMatch);
            }
            else
            {
                //get all of the matches which have a release date
                exactMatch = apage.results.Where(a=> a.release_date.HasValue).FirstOrDefault(mov => mov.title.StartsWith(movieName));
                if (exactMatch != null)
                {
                    //does the match we found match with our expected release date?
                    if (exactMatch.release_date.HasValue && exactMatch.release_date.Value.Year == Year)
                    {
                        return FetchSpecificMovieData(wc, exactMatch);
                    }
                }

                //which movie contains all name tokens (in order)
                searchTerm = searchTerm.Replace("%20", " ");
                var nameTokens = movieName.Split(new char[] { ' ' });
                var lastIDX = -1;
                var idx = -1;

                List<MovieSearchInfo> narrowedSearch = null;
                if (Year != -1)                
                    narrowedSearch = apage.results.Where(a => a.release_date.HasValue).Where(a => a.release_date.Value.Year == Year).ToList();                
                else                
                    narrowedSearch = apage.results;

                if (narrowedSearch.Count == 1)
                    return FetchSpecificMovieData(wc, narrowedSearch.First());

                int wordsMatched = 0;
                List<Tuple<MovieSearchInfo, int>> candidates = new List<Tuple<MovieSearchInfo, int>>();

                foreach (var thing in narrowedSearch)
                {
                    var matchAll = true;
                    foreach (var token in nameTokens)
                    {
                        int nextIDX = Math.Max(lastIDX, 0);
                        if (nextIDX >= thing.title.Length)
                            continue;
                        idx = thing.title.ToLower().IndexOf(token.ToLower(), nextIDX);
                        if (idx == -1 || idx < lastIDX)
                        {
                            matchAll = false;
                        }
                        else
                        {
                            lastIDX = idx;
                            wordsMatched++;
                        }
                    }
                    candidates.Add(new Tuple<MovieSearchInfo, int>(thing, wordsMatched));
                    wordsMatched = 0;
                }
                if (!candidates.Any())
                    return null;

                candidates = candidates.Where(movieTuple => movieTuple.Item1.title.StartsWith(movieName)).ToList();

                var bestMatch = candidates.Aggregate((i1, i2) => i1.Item2 > i2.Item2 ? i1 : i2);
                if (bestMatch != null)
                {
                    return FetchSpecificMovieData(wc, bestMatch.Item1);
                }

            }
            return null;
        }

        private static MovieInfo FetchSpecificMovieData(WebClient wc, MovieSearchInfo exactMatch)
        {
            string applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", string.Empty);

            wc.Headers.Add("Accept:application/json");
            string movieData = wc.DownloadString(string.Format(GetMovieById, exactMatch.id, API_KEY));
            var result = JSONConverter.Convert<MovieInfo>(movieData);

            if (!string.IsNullOrEmpty(result.poster_path))
            {
                int idx = result.poster_path.LastIndexOf(".");
                string extension = result.poster_path.Substring(idx);
                string imgFileLocation = null;
                if (result.release_date.HasValue)
                    imgFileLocation = Path.Combine(SettingsManager.ImagesFolder, string.Format("{0} ({1}).{2}", Sanitize(result.title), result.release_date.Value.Year, extension));
                else
                    imgFileLocation = Path.Combine(SettingsManager.ImagesFolder, string.Format("{0} ({1}).{2}", Sanitize(result.title), "XXXX", extension));

                if (!System.IO.Directory.Exists(SettingsManager.ImagesFolder))
                    System.IO.Directory.CreateDirectory(SettingsManager.ImagesFolder);

                if (!System.IO.File.Exists(imgFileLocation))
                {
                    try
                    {
                        wc.DownloadFile(string.Format(GetImageURL, result.poster_path), imgFileLocation);
                    }
                    catch (System.Net.WebException ex)
                    {
                        return null;
                    }
                }

                result.poster_path = imgFileLocation;
                try
                {
                    string relativePath = FileUtility.GetRelativePath(applicationPath, imgFileLocation);
                    result.poster_path = relativePath;
                }
                catch (Exception ex)
                {
                    //something didn't work, no relative path...
                }                
            }
            return result;
        }

        public static MovieCast FetchCastInfo(int movieDbOrgMovieId)
        {
            string url = string.Format(GetCastURL, movieDbOrgMovieId, API_KEY);
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("Accept:application/json");
            string data = wc.DownloadString(url);

            var cast = JSONConverter.Convert<MovieCast>(data);

            return cast;
        }

        private static string Sanitize(string p)
        {
            return p.Replace("'", string.Empty).Replace(":", string.Empty).Replace("?",string.Empty).Replace(",",string.Empty);
        }
    }
}
