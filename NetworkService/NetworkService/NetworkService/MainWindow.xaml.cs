using NetworkService.Controls;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }      
    }
}
