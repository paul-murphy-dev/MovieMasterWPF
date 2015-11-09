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
using System.Windows.Shapes;
using MovieMaster.ViewModels;

namespace MovieMaster.Views
{
    
    /// <summary>
    /// Interaction logic for GenericDialog.xaml
    /// </summary>
    public partial class GenericDialog : Window
    {
        private ViewModelBase _viewModel;

        private ViewModelBase ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;
            }
        }

        public FrameworkElement View
        {
            get;
            set;
        }

        public GenericDialog(FrameworkElement view, ViewModelBase viewModel)
        {
            InitializeComponent();
            this.DataContext = this;
            this.View = view;
            this.ViewModel = viewModel;
            view.DataContext = ViewModel;
            
        }
    }
}
