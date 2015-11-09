using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.Objects;
using MovieMaster.ViewModels;
using System.ComponentModel;
using System.Windows.Threading;
using MovieMaster.DataLayer;
using System.Collections.Specialized;
using System.Configuration;

namespace MovieMaster.Helpers
{
    public class DataQueueManager
    {
        private Dictionary<MovieViewModel, bool> _ignore = new Dictionary<MovieViewModel, bool>();
        private static DataQueueManager _instance;
        private BackgroundWorker _worker = null;
        private static Queue<MovieViewModel> _queue = new Queue<MovieViewModel>();
        private bool _force = false;
        public delegate void FetchingNameHandler(MovieViewModel mvm);
        public event FetchingNameHandler FetchingMovieData;
        public event FetchingNameHandler FetchComplete;

        private DataQueueManager()
        {
            
            
        }

        public string FetchingMovieName
        {
            get
            {
                if (!_queue.Any())
                    return string.Empty;

                return _queue.Peek().MovieName;
            }
        }

        public static DataQueueManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DataQueueManager();
            }
            return _instance;
        }

        public void ForceQueue(MovieViewModel movie)
        {
            _force = true;
            Queue(movie);
        }

        public void PurgeQueue()
        {
            if (_worker != null && _worker.IsBusy)
                _worker.CancelAsync();

            _queue.Clear();
        }

        public void Queue(MovieViewModel movie)
        {
            if (_force)
                _ignore.Remove(movie);

            if (!_queue.Contains(movie) && !_ignore.ContainsKey(movie))
                _queue.Enqueue(movie);
            DoNext();
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _worker.DoWork -= new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.Dispose();
            _worker = null;

            if (e.Cancelled || _queue.Count == 0)
                return;

            MovieViewModel movie = _queue.Dequeue();

            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            {
                DoNext();
                if (FetchComplete != null)
                {
                    FetchComplete(movie);
                }
            }));
        }

        private void DoNext()
        {
            if (!_queue.Any())
                return;

            if (FetchingMovieData != null)
            {
                FetchingMovieData(_queue.Peek());
            }                        

            if (_worker == null)
            {
                _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;                
                _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
                _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
                _worker.RunWorkerAsync();
            }
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
            if (e.Cancel)
                return;

            var movie = _queue.Peek();
            var result = MovieRepository.GetMovieImage(movie.MovieID);
            if (result == null || _force)
            {
                _force = false;
                var movieInfo = MovieDbAPI.MovieDbAPI.FetchMovieData(movie.MovieName, movie.Year);

                if (e.Cancel)
                    return;

                if (movieInfo != null)
                {
                    if (!string.IsNullOrEmpty(movieInfo.poster_path))
                    {
                        try
                        {
                            var img = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetFullPath(movieInfo.poster_path)));
                            img.Freeze();
                            movie.MovieImage = img;
                        }
                        catch (Exception ex)
                        {
                            //problem with the image
                            if (System.IO.File.Exists(movieInfo.poster_path))
                                System.IO.File.Delete(movieInfo.poster_path);
                        }
                    }

                    if (e.Cancel)
                        return;

                    movie.MovieName = movieInfo.title.Replace("Â", string.Empty);
                    movie.Rating = movieInfo.vote_average;
                    movie.Description = movieInfo.overview;
                    if (movie.Year == -1 && movieInfo.release_date.HasValue)
                        movie.Year = movieInfo.release_date.Value.Year;

                    if (!string.IsNullOrEmpty(movieInfo.poster_path) && System.IO.File.Exists(movieInfo.poster_path))
                    {
                        MovieRepository.DeleteImageForMovie(movie.MovieID);
                        movie.Model.ImageListID = MovieRepository.AddMovieImage(movie.MovieID, movieInfo.poster_path);
                    }
                    movie.GenreString = string.Join(", ",movieInfo.genres.Select(g=> g.name).ToArray());
                    MovieRepository.UpdateMovie(movie.Model);
                    movie.IsDirty = false;                  
                }
                else
                {
                    _ignore.Add(movie, true);
                }
            }
            else
            {
                result.Freeze();                
                movie.MovieImage = result;
                movie._MetaData = MovieRepository.GetMovieMetaData(movie.MovieID);
            }
        }

          
    
    }
}
