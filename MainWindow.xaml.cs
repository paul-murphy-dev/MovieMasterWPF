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
using MovieMaster.Objects;
using MovieMaster.DataLayer;
using MovieMaster.ViewModels;
using System.ServiceProcess;
using MovieMaster.Views;

namespace MovieMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string Theme { get; set; }
        private MainViewModel _viewModel = null;
        private FrameworkElement _view = null;
        
        public FrameworkElement View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
            }
        }

        public MainViewModel ViewModel
        {
            get { return _viewModel; }
        }


        public MainWindow()
        {
            Theme = "../Skins/DefaultSkin.xaml";
            InitializeComponent();

            this.DataContext = this;
            this.View = new MoviesListView();
            _viewModel = new MainViewModel();            

            View.DataContext = _viewModel;
            ((MainViewModel)_viewModel).Start();

            ((MainViewModel)_viewModel).ScrollToSelectedItem += MainWindow_ScrollToSelectedItem;

            //StartService("MySQL", 35000);            
        }

        void MainWindow_ScrollToSelectedItem(MovieViewModel item)
        {
            var scrollViewer = GetScrollViewer(((MoviesListView)this.View).lvwMovieList) as ScrollViewer;
            scrollViewer.ScrollToTop();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //StopService("MySQL", 30000);
            base.OnClosing(e);
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }

        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch
            {
                // ...
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MovieRepository.Initialize();
            (_viewModel as MainViewModel).OnRefreshMovieList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MovieRepository.Shutdown();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MaximizeRestoreLogic();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MaximizeRestoreLogic();
        }

        private void MaximizeRestoreLogic()
        {
            if (mainWindow.WindowState == System.Windows.WindowState.Normal)
            {
                mainWindow.WindowState = System.Windows.WindowState.Maximized;
                return;
            }

            if (mainWindow.WindowState == System.Windows.WindowState.Maximized)
                mainWindow.WindowState = System.Windows.WindowState.Normal;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = System.Windows.WindowState.Minimized;
        }
    }
}
