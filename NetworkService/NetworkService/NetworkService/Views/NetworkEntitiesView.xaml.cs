using NetworkService.Controls;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace NetworkService.Views
{
    public partial class NetworkEntitiesView : UserControl
    {
        public NetworkEntitiesView()
        {
            InitializeComponent();
            DataContext = NetworkEntitiesViewModel.Instance;
        }
    }
}