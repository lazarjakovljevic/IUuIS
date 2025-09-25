using NetworkService.Commands;
using NetworkService.Model;
using NetworkService.MVVM;
using NetworkService.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        #region Singleton Pattern
        private static MeasurementGraphViewModel instance;
        public static MeasurementGraphViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new MeasurementGraphViewModel();
                return instance;
            }
        }
        #endregion

        #region Fields
        private readonly DispatcherTimer refreshTimer;
        private readonly MeasurementService measurementService;
        private readonly ChartRenderingService chartRenderingService;
        private PowerConsumptionEntity selectedEntity;
        private bool isSilentSelection = false;
        private Canvas chartCanvas;
        #endregion

        #region Constructor
        public MeasurementGraphViewModel()
        {
            measurementService = MeasurementService.Instance;
            chartRenderingService = ChartRenderingService.Instance;
            LoadAvailableEntities();

            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            refreshTimer.Tick += OnRefreshTimer;
            refreshTimer.Start();
        }
        #endregion

        #region Properties
        private ObservableCollection<PowerConsumptionEntity> availableEntities;
        public ObservableCollection<PowerConsumptionEntity> AvailableEntities
        {
            get { return availableEntities; }
            set { SetProperty(ref availableEntities, value); }
        }

        public PowerConsumptionEntity SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                var previousEntity = selectedEntity;
                SetProperty(ref selectedEntity, value);

                if (value != null)
                {
                    LoadMeasurements();

                    // (ignore first selection)
                    if (!isSilentSelection && previousEntity != null && previousEntity != value)
                    {
                        var selectCommand = new SelectEntityCommand(previousEntity, value);
                        UndoManager.Instance.ExecuteCommand(selectCommand);
                    }
                }
            }
        }

        private ObservableCollection<Measurement> measurements;
        public ObservableCollection<Measurement> Measurements
        {
            get { return measurements; }
            set
            {
                SetProperty(ref measurements, value);
                RefreshChart();
            }
        }

        public Canvas ChartCanvas
        {
            get { return chartCanvas; }
            set
            {
                SetProperty(ref chartCanvas, value);
                RefreshChart();
            }
        }
        #endregion

        #region Methods
        private void LoadAvailableEntities()
        {
            AvailableEntities = MainWindowViewModel.SharedEntities;
        }

        private void LoadMeasurements()
        {
            if (SelectedEntity == null) return;

            var lastMeasurements = measurementService.GetLastMeasurementsForEntity(SelectedEntity.Id, 5);
            Measurements = new ObservableCollection<Measurement>(lastMeasurements);

            OnPropertyChanged(nameof(Measurements));
        }

        private void OnRefreshTimer(object sender, EventArgs e)
        {
            if (SelectedEntity != null)
            {
                LoadMeasurements();
            }
        }

        public void SetSelectedEntitySilently(PowerConsumptionEntity entity)
        {
            isSilentSelection = true;
            SelectedEntity = entity;
            isSilentSelection = false;
        }

        public void RefreshChart()
        {
            if (ChartCanvas != null)
            {
                chartRenderingService.RenderChart(ChartCanvas, Measurements);
            }
        }
        #endregion
    }
}