using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MovieMaster.DataLayer;
using System.Windows.Input;
using System.Windows;
using MovieMaster.Views;
using System.Windows.Media;
using MovieMaster.Objects;
using MovieMaster.Helpers;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.Win32;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Reflection;

namespace MovieMaster.ViewModels
{
    public class MainViewModel : ViewModelBase, IListen
    {
        #region Command Declarations

        private ICommand _selectFolderCommand;
        private ICommand _deleteMovieCommand;
        private ICommand _addMovieCommand;
        private ICommand _refreshMovieListCommand;
        private ICommand _loadMoviesFromFolderCommand;
        private ICommand _playMovieCommand;
        private ICommand _showLoadMoviesCommand;
        private ICommand _showSetImagesCommand;
        private ICommand _exitCommand;
        private ICommand _setImagesCommand;
        private ICommand _assignGenreCommand;
        private ICommand _clearSearchCommand;
        private ICommand _orderByMyTopRatedCommand;
        private ICommand _orderByTopRatedCommand;
        private ICommand _orderByMostPlayedCommand;
        private ICommand _orderByNameAlphaCommand;
        private ICommand _findNewMoviesCommand;
        private ICommand _changeThemeCommand;

        #endregion

        private Theme _currentTheme;
        private BackgroundWorker _worker;
        private BitmapImage _loadingImage;
        private MovieViewModel _selectedMovie;
        private List<MovieViewModel> _movies = new List<MovieViewModel>();
        private string _searchString = string.Empty;
        private GenericDialog _dialog = null;
        private string _statusMsg;
        private System.Windows.Threading.DispatcherTimer _statusTimer;
        private System.Windows.Threading.DispatcherTimer _searchTimer;
        public delegate void ScrollToSelectedItemHandler(MovieViewModel item);
        public event ScrollToSelectedItemHandler ScrollToSelectedItem;

        public MainViewModel()
        {
            EventHelper.Instance.Subscribe(this);
            _selectFolderCommand = new RelayCommand(param => this.OnSelectFolder(), param => { return true; });
            _deleteMovieCommand = new RelayCommand(param => this.OnDeleteMovie(), param => this.CanDeleteMovie);
            _addMovieCommand = new RelayCommand(param => this.OnAddMovie(), param => this.CanAddMovie);
            _refreshMovieListCommand = new RelayCommand(param => this.OnRefreshMovieList(), param => this.CanRefreshMovieList);
            _loadMoviesFromFolderCommand = new RelayCommand(param => this.OnLoadMoviesFromFolder(), param => this.CanLoadMoviesFromFolder);
            _playMovieCommand = new RelayCommand(param => this.OnPlayMovie(), param => this.CanPlayMovie);
            _showLoadMoviesCommand = new RelayCommand(param => this.OnShowLoadMovies(), param => this.CanShowLoadMovies);
            _showSetImagesCommand = new RelayCommand(param => this.OnShowSetImages(), param => this.CanShowSetImages);
            _setImagesCommand = new RelayCommand(param => this.OnSetImages(), param => this.CanSetImages);
            _exitCommand = new RelayCommand(param => this.OnExit(), param => this.CanExit);
            _clearSearchCommand = new RelayCommand(param => this.OnClearSearch(), param => CanClearSearch);
            _orderByMyTopRatedCommand = new RelayCommand(param => this.OnOrderByMyTopRatedCommand(), param => { return Movies.Count > 0; });
            _orderByTopRatedCommand = new RelayCommand(param => this.OnOrderByTopRatedCommand(), param => { return Movies.Count > 0; });
            _orderByMostPlayedCommand = new RelayCommand(param => this.OnOrderByMostPlayedCommand(), param => { return Movies.Count > 0; });
            _orderByNameAlphaCommand = new RelayCommand(param => this.OnOrderByNameAlphaCommand(), param => { return Movies.Count > 0; });
            _findNewMoviesCommand = new RelayCommand(param => this.OnFindNewMovies(), param => { return Movies.Count > 0 && !string.IsNullOrEmpty(SettingsManager.MediaFolder); });
            _changeThemeCommand = new RelayCommand(ChangeThemCommandCalled, param => { return true; });

            DataQueueManager.GetInstance().FetchingMovieData += new DataQueueManager.FetchingNameHandler(MainViewModel_FetchingMovieData);
            DataQueueManager.GetInstance().FetchComplete += new DataQueueManager.FetchingNameHandler(MainViewModel_FetchComplete);
            NotifyPropertyChanged("Movies");           
            SettingsManager.ImagesFolder = Path.Combine(SettingsManager.MediaFolder, "Images");            
            OnChangeTheme(this.Themes.FirstOrDefault(a=> a.Name == SettingsManager.Theme));
        }

        #region Commands

        public ICommand AssignGenreCommand
        {
            get
            {
                return _assignGenreCommand;
            }
        }

        public ICommand DeleteMovieCommand
        {
            get
            {
                return _deleteMovieCommand;
            }
        }

        public ICommand AddMovieCommand
        {
            get
            {
                return _addMovieCommand;
            }
        }

        public ICommand RefreshMovieListCommand
        {
            get
            {
                return _refreshMovieListCommand;
            }
        }

        public ICommand LoadMoviesFromFolderCommand
        {
            get
            {
                return _loadMoviesFromFolderCommand;
            }
        }

        public ICommand PlayMovieCommand
        {
            get
            {
                return _playMovieCommand;
            }
        }

        public ICommand ShowLoadMoviesCommand
        {
            get
            {
                return _showLoadMoviesCommand;
            }
        }

        public ICommand ShowSetImagesCommand
        {
            get
            {
                return _showSetImagesCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                return _exitCommand;
            }
        }

        public ICommand SetImagesCommand
        {
            get
            {
                return _setImagesCommand;
            }
        }

        public ICommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand;
            }
        }

        public ICommand OrderByNameAlphaCommand
        {
            get
            {
                return _orderByNameAlphaCommand;
            }
        }

        public ICommand OrderByMostPlayedCommand
        {
            get
            {
                return _orderByMostPlayedCommand;
            }
        }

        public ICommand OrderByMyTopRatedCommand
        {
            get
            {
                return _orderByMyTopRatedCommand;
            }
        }

        public ICommand OrderByTopRatedCommand
        {
            get
            {
                return _orderByTopRatedCommand;
            }
        }

        public ICommand FindNewMoviesCommand
        {
            get
            {
                return _findNewMoviesCommand;
            }
        }

        public ICommand ChangeThemeCommand
        {
            get
            {
                return _changeThemeCommand;
            }
        }       

        #endregion

        #region Properties

        public List<Theme> Themes
        {
            get
            {
                return 
                    new List<Theme>() { 
                        new Theme() { Name = "Default", URI = "../Skins/DefaultSkin.xaml" }, 
                        new Theme() { Name = "Red", URI = "../Skins/RedSkin.xaml" },
                        new Theme() { Name = "Blue", URI = "../Skins/BlueSkin.xaml" },
                        new Theme() { Name = "Mango", URI = "../Skins/MangoSkin.xaml" }
                    };
            }            
        }

        public Theme CurrentTheme
        {
            get
            {
                return _currentTheme;
            }
            set
            {
                OnChangeTheme(value);
                _currentTheme = value;
                NotifyPropertyChanged("CurrentTheme");
            }
        }

        public void OnChangeThemeCommand(string name)
        {
            OnChangeTheme(Themes.FirstOrDefault(a => a.Name == name));
        }

        public MovieViewModel SelectedMovie
        {
            get
            {
                return _selectedMovie;
            }
            set
            {
                _selectedMovie = value;
                if (_selectedMovie != null)
                    _selectedMovie.Select();
                NotifyPropertyChanged("SelectedMovie");
            }
        }

        public string SelectedFolder { get; set; }

        public string FetchingMovie
        {
            get
            {
                if (String.IsNullOrEmpty(DataQueueManager.GetInstance().FetchingMovieName))
                    return string.Empty;

                return String.Format("Fetching data for {0}", DataQueueManager.GetInstance().FetchingMovieName);
            }            
        }

        public string StatusMessage
        {
            get
            {
                return _statusMsg;
            }
            set
            {
                _statusMsg = value;
                NotifyPropertyChanged("StatusMessage");
            }
        }

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                if (_searchTimer == null)
                {
                    _searchTimer = new DispatcherTimer();
                    _searchTimer.Interval = new TimeSpan(0,0,0,0,500);
                    _searchTimer.Tick += _searchTimer_Tick;
                    _searchTimer.IsEnabled = true;
                    _searchTimer.Start();
                }
                else
                {
                    //Reset the timer
                    _searchTimer.Stop();
                    _searchTimer.Start();
                }
                
                NotifyPropertyChanged("SearchString");                
                NotifyPropertyChanged("CanClearSearch");
            }
        }

        void _searchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Tick -= _searchTimer_Tick;
            _searchTimer = null;
            NotifyPropertyChanged("Movies");
        }

        public IList<MovieViewModel> Movies
        {
            get
            {
                if (string.IsNullOrEmpty(_searchString))
                    return _movies;

                //List<MovieViewModel> finalResults = new List<MovieViewModel>();
                //List<List<MovieViewModel>> resultSets = new List<List<MovieViewModel>>();
                //if (SearchString.Contains(","))//OR search
                //{
                //    string[] searchTokens = _searchString.Trim().ToLower().Split(new char[] { ',' });

                //    if (searchTokens.Length < 2)
                //        return _movies;

                //    foreach (string token in searchTokens)
                //    {
                //        var searchTerm = token.ToLower().Trim();
                //        //movies that start with the search string
                //        var results = _movies.Where(mov => mov.MovieName.ToLower().StartsWith(searchTerm)).ToList();

                //        //movies that contain the search string
                //        results.AddRange(_movies.Where(mov => mov.MovieName.ToLower().Contains(searchTerm)).Except(results));

                //        //genres that contain the search string
                //        results.AddRange(_movies.Where(a => a.GenreString.ToLower().Contains(searchTerm)).Except(results));

                //        resultSets.Add(results);
                //    }

                //    foreach (var set in resultSets)
                //    {
                //        finalResults.AddRange(set.Except(finalResults));
                //    }
                //}
                //else if (SearchString.Contains("+"))//AND search
                //{
                //    string[] searchTokens = _searchString.Trim().ToLower().Split(new char[] { '+' });

                //    if (searchTokens.Length < 2)
                //        return _movies;

                //    foreach (string token in searchTokens)
                //    {
                //        var searchTerm = token.ToLower().Trim();
                //        //movies that start with the search string
                //        var results = _movies.Where(mov => mov.MovieName.ToLower().StartsWith(searchTerm)).ToList();

                //        //movies that contain the search string
                //        results.AddRange(_movies.Where(mov => mov.MovieName.ToLower().Contains(searchTerm)).Except(results));

                //        //genres that contain the search string
                //        results.AddRange(_movies.Where(a => a.GenreString.ToLower().Contains(searchTerm)).Except(results));

                //        resultSets.Add(results);
                //    }

                //    for (int i = 0; i < resultSets.Count - 2; i++)
                //    {
                //        finalResults.AddRange(from movies in resultSets[i]
                //                              join otherMovies in resultSets[i + 1] on movies.MovieID equals otherMovies.MovieID
                //                              select movies);
                //    }
                //}

                //return finalResults;

                var searchTerm = _searchString.ToLower().Trim();
                //movies that start with the search string
                var results = _movies.Where(mov => mov.MovieName.ToLower().StartsWith(searchTerm)).ToList();

                //movies that contain the search string
                results.AddRange(_movies.Where(mov => mov.MovieName.ToLower().Contains(searchTerm)).Except(results));

                //genres that contain the search string
                results.AddRange(_movies.Where(a => a.GenreString.ToLower().Contains(searchTerm)).Except(results));
                 
                return results;
            }
        }

        public BitmapImage LoadingImage
        {
            get
            {
                if (_loadingImage == null)
                {
                    _loadingImage = new BitmapImage();
                    _loadingImage.BeginInit();
                    _loadingImage.UriSource = new Uri("pack://application:,,/Resources/ajaxLoad.gif");
                    _loadingImage.EndInit();
                }
                return _loadingImage;
            }
        }

        #endregion

        #region Command Methods

        public bool CanClearSearch
        {
            get
            {
                return !string.IsNullOrEmpty(_searchString);
            }
        }

        public bool CanShowSetImages
        {
            get
            {
                return (Movies != null && Movies.Any());
            }
        }

        public bool CanSetImages
        {
            get
            {
                return SelectedFolder != null;
            }
        }

        public bool CanExit
        {
            get
            {
                return true;
            }
        }

        public bool CanDeleteMovie
        {
            get
            {
                return SelectedMovie != null;
            }
        }

        public bool CanAddMovie
        {
            get
            {
                return true;
            }
        }

        public bool CanPlayMovie
        {
            get
            {
                return SelectedMovie != null;
            }
        }

        public bool CanShowLoadMovies
        {
            get
            {
                return true;
            }
        }

        public bool CanRefreshMovieList
        {
            get
            {
                return true;
            }
        }

        public bool CanAssignGenre
        {
            get
            {
                return true;
            }
        }

        public void OnExit()
        {
            MovieRepository.Shutdown();
            Application.Current.Shutdown();
        }

        public bool CanLoadMoviesFromFolder
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedFolder);
            }
        }

        public void OnFindNewMovies()
        {
            if (System.IO.Directory.Exists(SettingsManager.MediaFolder))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(SettingsManager.MediaFolder);
                System.IO.FileInfo[] files = di.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

                Dictionary<string, Movie> lookup = new Dictionary<string, Movie>();
                foreach (Movie m in MovieRepository.ListAllMovies())
                {
                    lookup.Add(m.LocalPath, m);
                }

                List<Movie> newMovies = new List<Movie>();

                foreach (System.IO.FileInfo fi in files)
                {
                    //do something
                    switch (fi.Extension)
                    {
                        case ".mkv":
                        case ".mp4":
                        case ".avi":

                            string relativePath = string.Empty;
                            try
                            {
                                relativePath = FileUtility.GetRelativePath(FileUtility.ApplicationPath, fi.FullName);
                            }
                            catch (Exception ex)
                            {
                                //something didn't work, no relative path...
                            }

                            if (lookup.ContainsKey(fi.FullName) || lookup.ContainsKey(relativePath))
                                continue;

                            Movie movieData = new Movie();
                            movieData.Name = "";

                            string fileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);

                            movieData.LocalPath = fi.FullName;
                            movieData.Name = GetMovieNameFromFileName(fileName);
                            movieData.Year = GetYearFromFileName(fileName);

                            if (movieData.Year < 1930)
                                movieData.Year = -1;

                            if (movieData.Year == -1)
                            {
                                //we couldn't figure out the data we needed from the filename
                                //so maybe this information is in actually on the folder that 
                                //contains our file
                                string[] pathTokens = fi.FullName.Split(new char[] { '\\' });
                                if (pathTokens != null && pathTokens.Length > 0)
                                {
                                    int year = GetYearFromFileName(pathTokens[pathTokens.Length - 2]);

                                    if (year < 1930)
                                        year = -1;

                                    if (year != -1)
                                    {
                                        movieData.Name = GetMovieNameFromFileName(pathTokens[pathTokens.Length - 2]);
                                        movieData.Year = year;
                                    }
                                }
                            }

                            movieData.Name.Replace("..", ":");
                            //Remove artifacts from the file name
                            string[] removeList = new string[] { "(", ")", "[", "]", "{", "}", ".", ",", "_", "-", "~" };
                            foreach (var thing in removeList)
                                movieData.Name = movieData.Name.Replace(thing, " ");

                            newMovies.Add(movieData);
                            break;
                    }
                }

                var moviesToRefresh = new List<MovieViewModel>();
                foreach (Movie movie in newMovies)
                {
                    MovieRepository.CreateMovie(movie);                                       
                }

                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {

                    OnRefreshMovieList();
                    
                }
                    ));
            }
        }

        private void ChangeThemCommandCalled(object state)
        {
            var str = state as string;
            OnChangeTheme(Themes.FirstOrDefault(a => a.Name == str));
        }       

        private void OnChangeTheme(Theme selectedTheme)
        {
            if (selectedTheme == null)
                selectedTheme = Themes.First();

            SettingsManager.Theme = selectedTheme.Name;
            try
            {
                Uri dictUri = new Uri(selectedTheme.URI, UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(dictUri) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch { }
        }

        public void OnAssignGenre()
        {

        }

        public void OnShowSetImages()
        {
            this.SelectedFolder = SettingsManager.ImagesFolder;

            _dialog = new GenericDialog(new SelectImageFolderView(), this);
            if (_dialog.ShowDialog() == true)
            {
                OnRefreshMovieList();
            }
        }

        public void OnSetImages()
        {
            if (System.IO.Directory.Exists(SelectedFolder))
            {
                SettingsManager.ImagesFolder = SelectedFolder;
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(SelectedFolder);
                System.IO.FileInfo[] files = di.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

                foreach (var movie in Movies)
                {
                    string[] nameTokens = movie.MovieName.Split(new char[] { ' ' });
                    string[] tokens = new string[nameTokens.Length + 1];
                    nameTokens.CopyTo(tokens, 0);
                    tokens[tokens.Length - 1] = movie.Year.ToString();

                    var matches = DoMatching(nameTokens, files);
                    if (matches == null || !matches.Any())
                        continue;

                    switch (matches.First().Extension)
                    {
                        case ".jpg":
                        case ".png":
                        case ".bmp":
                            movie.MovieImage = new BitmapImage(new Uri(matches.First().FullName));
                            movie.MovieImage.Freeze();
                            MovieRepository.UpdateMovieImage(movie.Model, matches.First().FullName);
                            break;
                    }
                }
                _dialog.Close();
            }
        }

        public void OnDeleteMovie()
        {
            if (MessageBox.Show(string.Format("Are you sure you want to delete the movie: {0}?", SelectedMovie.MovieName.Trim()), "Delete Movie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (!string.IsNullOrEmpty(SelectedMovie.LocalPath) && System.IO.File.Exists(SelectedMovie.LocalPath))
                {
                    if (MessageBox.Show("Do you also want to delete the movie file on disc?\n NOTE: MOVIE IS UNRECOVERABLE!!", "Delete Movie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        System.IO.File.Delete(SelectedMovie.LocalPath);
                    }
                }
                MovieRepository.DeleteMovie(SelectedMovie.Model);
                OnRefreshMovieList();
            }
        }

        public void OnAddMovie()
        {
            Movie newMovie = new Movie();
            newMovie.Name = "New Movie";
            MovieViewModel mvm = new MovieViewModel(newMovie);
            MovieRepository.CreateMovie(newMovie);
            OnRefreshMovieList();
        }

        public void OnRefreshMovieList()
        {
            var movies = MovieRepository.ListAllMovies();
            _movies = movies.Select(thing => new MovieViewModel(thing)).ToList();            
            NotifyPropertyChanged("Movies");

            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var mov in Movies)
                {
                    mov.MovieImage = MovieRepository.GetMovieImage(mov.MovieID);
                    mov.IsDirty = false;

                    if (mov.MovieImage == null)
                    {
                        DataQueueManager.GetInstance().Queue(mov);
                    }
                }
            }));


        }

        public void OnPlayMovie()
        {
            if (System.IO.File.Exists(SelectedMovie.LocalPath))
            {
                try
                {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = SelectedMovie.LocalPath };
                    p.Start();

                    SelectedMovie.TimesPlayed += 1;
                    MovieRepository.UpdateMovie(SelectedMovie.Model);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("There was an error trying to play this file.\n Message: {0}", ex.Message), "Error Playing File", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                var result = MessageBox.Show(string.Format("This video file for {0}, cannot be found anymore at \"{1}\", do you want to remove it from the list?", SelectedMovie.MovieName, SelectedMovie.LocalPath), "Movie Not Found", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    MovieRepository.DeleteMovie(this.SelectedMovie.Model);
                    _movies.Remove(Movies.FirstOrDefault(a => a.MovieID == this.SelectedMovie.MovieID));
                    NotifyPropertyChanged("Movies");
                }
            }
        }

        public void OnShowLoadMovies()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select your movies folder:";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.SelectedFolder = fbd.SelectedPath;
                OnLoadMoviesFromFolder();
                OnRefreshMovieList();
            }
            fbd.Dispose();
        }

        public void OnLoadMoviesFromFolder()
        {
            if (System.IO.Directory.Exists(SelectedFolder))
            {
                if (this.Movies.Any())
                {
                    if (MessageBox.Show("This will replace your movies list, do you want to continue?", "Replace Movies", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        return;
                }
                DataQueueManager.GetInstance().PurgeQueue();
                MovieRepository.ClearRepository();
                SettingsManager.MediaFolder = SelectedFolder;
                if (string.IsNullOrEmpty(SettingsManager.ImagesFolder))
                {
                    SettingsManager.ImagesFolder = Path.Combine(SelectedFolder, "Images");
                }

                var di = new System.IO.DirectoryInfo(SelectedFolder);
                var files = di.GetFiles("*.*", System.IO.SearchOption.AllDirectories);                

                var movies = new Dictionary<string, Movie>();
                foreach (System.IO.FileInfo fi in files)
                {
                    //do something
                    switch (fi.Extension)
                    {
                        case ".mkv":
                        case ".mp4":
                        case ".avi":
                            Movie movieData = new Movie();
                            movieData.Name = "";

                            string fileName = System.IO.Path.GetFileNameWithoutExtension(fi.Name);


                            movieData.LocalPath = fi.FullName;
                            movieData.Name = GetMovieNameFromFileName(fileName);
                            movieData.Year = GetYearFromFileName(fileName);

                            if (movieData.Year < 1930)
                                movieData.Year = -1;

                            if (movieData.Year == -1)
                            {
                                //we couldn't figure out the data we needed from the filename
                                //so maybe this information is in actually on the folder that 
                                //contains our file
                                string[] pathTokens = fi.FullName.Split(new char[] { '\\' });
                                if (pathTokens != null && pathTokens.Length > 0)
                                {
                                    int year = GetYearFromFileName(pathTokens[pathTokens.Length - 2]);

                                    if (year < 1930)
                                        year = -1;

                                    if (year != -1)
                                    {
                                        movieData.Name = GetMovieNameFromFileName(pathTokens[pathTokens.Length - 2]);
                                        movieData.Year = year;
                                    }
                                }
                            }

                            movieData.Name.Replace("..", ":");
                            //Remove artifacts from the file name
                            string[] removeList = new string[] { "(", ")", "[", "]", "{", "}", ".", ",", "_", "-", "~" };
                            foreach (var thing in removeList)
                                movieData.Name = movieData.Name.Replace(thing, " ");

                            movieData.Name = movieData.Name.Trim();

                            try
                            {
                                string relativePath = FileUtility.GetRelativePath(FileUtility.ApplicationPath, movieData.LocalPath);
                                movieData.LocalPath = relativePath;
                            }
                            catch (Exception ex)
                            {
                                //something didn't work, no relative path...
                            }

                            if (!movies.ContainsKey(movieData.ToString()))
                                movies.Add(movieData.ToString(), movieData);
                            break;
                    }
                }

                foreach (Movie movie in movies.Values)
                {
                    var found = MovieRepository.GetMovieByPath(movie.LocalPath.Trim());
                    if (found == null || !(found.Name == movie.Name && movie.Year == found.Year))
                        MovieRepository.CreateMovie(movie);                 
                }

                OnRefreshMovieList();
            }
        }

        private int GetYearFromFileName(string fileName)
        {
            if (Regex.IsMatch(fileName, "\\d{4}"))
            {
                //this string contains a year or something like it
                Match m = Regex.Match(fileName, "\\d{4}");
                string year = fileName.Substring(m.Index, 4);
                return int.Parse(year);
            }

            return -1;
        }

        private string GetMovieNameFromFileName(string fileName)
        {
            string test = fileName.ToLower();
            string[] replaceList = new 
                string[] { ".", ",", "_", 
                          "dvdscr", "dvdrip", "axxo", "jaybob", 
                          "dvd", "720p", "1080p", "1080 px", 
                          "720 px", "720 p", "1080 p", "bdrip", "brrip",
                          "ac3", "x264", "bluray", "aac", "hdrip", "subbed",
                          "xvid", "divx", "cd1", "cd2"};
            foreach (var token in replaceList)
                test = test.Replace(token, " ");

            string candidateFileName = test.Substring(0, Math.Min(256, test.Length)).Trim();
            var nameTokens = test.Split(new char[]{' '});

            if (nameTokens.Length > 5)
            {
                candidateFileName = string.Join(" ", nameTokens.Take(5));
            }

            Match m = Regex.Match(test, "\\d{4}");
            if (m.Success)
            {
                candidateFileName = test.Substring(0, m.Index);
            }

            candidateFileName = candidateFileName.TrimEnd(new char[] { '[', '{', ' ', '-' });

            return candidateFileName;
        }

        public void OnSelectFolder()
        {
            //Microsoft.WindowsAPICodePack.Dialogs.TaskDialog td = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog();
            //td.InstructionText = "Select the folder";
            //td.Show();





        }

        public void OnClearSearch()
        {
            this.SearchString = string.Empty;
        }

        public void OnOrderByMyTopRatedCommand()
        {
            _movies = _movies.OrderByDescending(a => a.UserRating).ToList();
            ScrollToFirstMovie();
            NotifyPropertyChanged("Movies");
        }

        public void OnOrderByTopRatedCommand()
        {
            _movies = _movies.OrderByDescending(a => a.Rating).ToList();
            ScrollToFirstMovie();
            NotifyPropertyChanged("Movies");
        }

        public void OnOrderByMostPlayedCommand()
        {
            _movies = _movies.OrderByDescending(a => a.TimesPlayed).ToList();
            ScrollToFirstMovie();
            NotifyPropertyChanged("Movies");
        }

        public void OnOrderByNameAlphaCommand()
        {
            _movies = _movies.OrderBy(a => a.MovieName).ToList();
            ScrollToFirstMovie();
            NotifyPropertyChanged("Movies");
        }

        #endregion

        #region Methods

        private void ScrollToFirstMovie()
        {
            this.SelectedMovie = Movies.First();
            if (ScrollToSelectedItem != null)
            {
                ScrollToSelectedItem(SelectedMovie);
            }
        }

        private FileInfo[] DoMatching(IEnumerable<string> tokens, FileInfo[] matchSet)
        {
            matchSet = matchSet.Where(a => a.Name.Contains(tokens.First())).ToArray();

            switch (matchSet.Length)
            {
                case 0:
                    return null;
                case 1:
                    return matchSet;//1 match

                default:
                    if (tokens.Count() == 1)
                    {
                        matchSet = matchSet.Take(1).ToArray();
                        return matchSet;
                    }
                    else
                        return DoMatching(tokens.Except(new string[] { tokens.First() }), matchSet);
            }
        }

        void MainViewModel_FetchComplete(MovieViewModel mvm)
        {
            NotifyPropertyChanged("FetchingMovie");
            NotifyPropertyChanged("Movies");
            mvm.FireNotifyPropertyChanged("MovieName");
            mvm.FireNotifyPropertyChanged("MovieImage");
            mvm.FireNotifyPropertyChanged("GenreString");
            mvm.FireNotifyPropertyChanged("Rating");
            mvm.FireNotifyPropertyChanged("TimesPlayed");
            mvm.FireNotifyPropertyChanged("UserRating");
            mvm.FireNotifyPropertyChanged("Year");
            mvm.FireNotifyPropertyChanged("TruncatedDescription");
            mvm.FireNotifyPropertyChanged("TimesPlayed");
        }

        void MainViewModel_FetchingMovieData(MovieViewModel mvm)
        {
            NotifyPropertyChanged("FetchingMovie");
        }

        internal void Start()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.RunWorkerAsync();
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            NotifyPropertyChanged("Movies");
            _worker.DoWork -= new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.Dispose();
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var results = MovieRepository.ListAllMovies();
            _movies = results.Select(a => new MovieViewModel(a)).ToList();
        }
        #endregion

        #region IListen Members

        public void Notify(object msg)
        {
            this.StatusMessage = msg.ToString();
            _statusTimer = new DispatcherTimer();
            _statusTimer.Interval = new TimeSpan(0, 0, 1);
            _statusTimer.Tick += new EventHandler(_statusTimer_Tick);
            _statusTimer.Start();
        }

        void _statusTimer_Tick(object sender, EventArgs e)
        {
            this.StatusMessage = string.Empty;
            _statusTimer.Stop();            
            _statusTimer.Tick -= new EventHandler(_statusTimer_Tick);
            _statusTimer = null;
        }

        #endregion
    }
}
