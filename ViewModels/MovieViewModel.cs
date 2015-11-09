using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.Objects;
using System.ComponentModel;
using System.Windows.Media;
using MovieMaster.Helpers;
using MovieMaster.DataLayer;
using System.Windows.Threading;
using System.Windows.Input;

namespace MovieMaster.ViewModels
{
    public class MovieViewModel : ViewModelBase
    {
        private ICommand _saveMovieCommand;
        private ICommand _refreshMovieDataCommand;

        private string _genre = null;
        private Movie _model = null;
        private ImageSource _movieImage = null;
        private List<MovieMetaData> _metadata = null;
        private IEnumerable<Tuple<string,string>> _displayedMetadata;
        private bool _isdirty = false;

        public MovieViewModel(Movie model)
        {
            _model = model;
            _saveMovieCommand = new RelayCommand(param => this.OnSaveMovie(), param => this.CanSaveMovie);
            _refreshMovieDataCommand = new RelayCommand(param => this.OnRefreshMovieData(), param => { return true; });
        }

        public ICommand SaveMovieCommand
        {
            get
            {
                return _saveMovieCommand;
            }
        }

        public ICommand RefreshMovieDataCommand
        {
            get
            {
                return _refreshMovieDataCommand;
            }
        }


        public bool IsDirty
        {
            get
            {
                return _isdirty;
            }
            set
            {
                _isdirty = value;
                NotifyPropertyChanged("IsDirty");
            }
        }

        public Movie Model
        {
            get
            {
                return _model;
            }
        }
        
        public void OnSaveMovie()
        {
            MovieRepository.UpdateMovie(this.Model);
            this.IsDirty = false;
            EventHelper.Instance.Publish("Movie Updated");
        }

        public bool CanSaveMovie
        {
            get
            {
                return IsDirty;
            }
        }

        public int MovieID
        {
            get
            {
                return _model.MovieID;
            }
            set
            {
                if (_model.MovieID != value)
                {
                    _model.MovieID = value;
                    NotifyPropertyChanged("MovieID");
                }
            }
        }

        public string MovieName
        {
            get
            {
                return _model.Name;
            }
            set
            {
                if (_model.Name != value)
                {
                    _model.Name = value;
                    NotifyPropertyChanged("MovieName");
                    IsDirty = true;
                }
            }
        }

        public int Year
        {
            get
            {
                return _model.Year;
            }
            set
            {
                if (_model.Year != value)
                {
                    _model.Year = value;
                    NotifyPropertyChanged("Year");
                    IsDirty = true;
                }
            }
        }

        public string GenreString
        {
            get
            {
                return Model.GenreString;
            }
            set
            {
                Model.GenreString = value;
                NotifyPropertyChanged("GenreString");
                IsDirty = true;
            }
        }

        public string LocalPath
        {
            get
            {
                return _model.LocalPath;
            }
            set
            {
                if (_model.LocalPath != value)
                {
                    _model.LocalPath = value;
                    NotifyPropertyChanged("LocalPath");
                }
            }
        }

        public string Description
        {
            get
            {
                return _model.Description;
            }
            set
            {
                if (_model.Description != value)
                {
                    _model.Description = value;
                    NotifyPropertyChanged("Description");
                    NotifyPropertyChanged("TruncatedDescription");
                }
            }
        }

        public string TruncatedDescription
        {
            get
            {
                if (string.IsNullOrEmpty(_model.Description))
                    return string.Empty;

                return string.Format("{0}...", _model.Description.Substring(0, Math.Min(_model.Description.Length-1, 400)));
            }
        }

        public double Rating
        {
            get
            {
                return _model.Rating;
            }
            set
            {
                if (_model.Rating != value)
                {
                    _model.Rating = value;
                    NotifyPropertyChanged("Rating");
                }
            }
        }

        public double UserRating
        {
            get
            {
                return _model.UserRating;
            }
            set
            {
                if (_model.UserRating != value)
                {
                    if (value > 10)
                        value = 10;
                    if (value < 0)
                        value = 0;

                    _model.UserRating = value;
                    IsDirty = true;
                    NotifyPropertyChanged("UserRating");
                }
            }
        }

        public int TimesPlayed
        {
            get
            {
                return _model.TimesPlayed;
            }
            set
            {
                if (_model.TimesPlayed != value)
                {
                    _model.TimesPlayed = value;
                    NotifyPropertyChanged("TimesPlayed");
                }
            }
        }

        public ImageSource MovieImage
        {
            get
            {
                return _movieImage;
            }
            set
            {
                _movieImage = value;
                NotifyPropertyChanged("MovieImage");
            }
        }

        public IEnumerable<Tuple<string, string>> MetaDataNames
        {
            get
            {
                if (_metadata == null)
                    return null;

                if (_displayedMetadata == null)
                    _displayedMetadata = _metadata.Select(a => new Tuple<string, string>(a.Value.Substring(0, a.Value.IndexOf("|") - 1), a.Value.Substring(0, a.Value.IndexOf("|") + 1))).ToList();

                return _displayedMetadata;
            }
        }

        internal List<MovieMetaData> _MetaData
        {
            set
            {
                _metadata = value;
                NotifyPropertyChanged("MetaData");
            }
        }

        internal void Select()
        {
            if (_movieImage == null)
            {
                _movieImage = MovieRepository.GetMovieImage(MovieID);
                if (_movieImage == null)
                {
                    //we don't have the image, queue this up to get the movie data...
                    DataQueueManager.GetInstance().Queue(this);
                }
                else
                    NotifyPropertyChanged("MovieImage");
            }
            else
            {
                if (_metadata == null)
                {
                    _metadata = MovieRepository.GetMovieMetaData(MovieID);
                    if (_metadata != null)
                    {
                        NotifyPropertyChanged("MetaData");
                    }
                }
            }
        }

        internal void FireNotifyPropertyChanged(string property)
        {
            base.NotifyPropertyChanged(property);
        }

        public void OnRefreshMovieData()
        {
            this.Model.Reset();
            DataQueueManager.GetInstance().ForceQueue(this);
        }

        //public List<Genre> Genres
        //{
        //    get
        //    {
        //        return _model.Genres;
        //    }
        //}
    }
}
