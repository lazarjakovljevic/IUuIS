using System.Windows.Controls;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class MeasurementGraphView : UserControl
    {
        #region Fields
        private MeasurementGraphViewModel viewModel;
        #endregion

        #region Constructor
        public MeasurementGraphView()
        {
            InitializeComponent();

            viewModel = MeasurementGraphViewModel.Instance;
            this.DataContext = viewModel;

            viewModel.ChartCanvas = ChartCanvas;
        }
        #endregion
    }
}