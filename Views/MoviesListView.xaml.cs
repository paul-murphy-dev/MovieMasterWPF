using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MovieMaster.ViewModels;
using System.Collections.ObjectModel;

namespace MovieMaster.Views
{
    /// <summary>
    /// Interaction logic for MovieListIView.xaml
    /// </summary>
    public partial class MoviesListView : UserControl
    {
        public List<MovieViewModel> Movies
        {
            get;
            set;
        }        

        public MoviesListView()
        {
            InitializeComponent();
        }

        private void lvwMovieList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LaunchMovieLogic();
        }

        private void LaunchMovieLogic()
        {
            MainViewModel model = (this.DataContext as MainViewModel);
            if (model.CanPlayMovie)
                model.OnPlayMovie();
        }

        private void lvwMovieList_KeyDown(object sender, KeyEventArgs e)
        {
            MainViewModel model = (this.DataContext as MainViewModel);
            switch (e.Key)
            {
                case Key.Enter:
                    LaunchMovieLogic();
                    break;
                case Key.Delete:                    
                    if (model.CanDeleteMovie)
                        model.OnDeleteMovie();
                    break;
            }                
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    txtSearch.Text = string.Empty;
                    break;
                case Key.Down:
                    this.lvwMovieList.Focus();
                    break;
            }
        }

        private void lvwMovieList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (lvwMovieList.Items.Count > 0 && lvwMovieList.SelectedIndex == 0)
            {
                if (e.Key == Key.Up)
                {
                    this.txtSearch.Focus();
                }
            }
        }

        private void TopPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }        
    }
}
