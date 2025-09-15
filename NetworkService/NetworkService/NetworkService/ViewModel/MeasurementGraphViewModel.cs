using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using NetworkService.MVVM;
using NetworkService.Model;
using NetworkService.Services;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        #region Fields

        private readonly DispatcherTimer refreshTimer;
        private readonly MeasurementService measurementService;

        #endregion

        #region Properties

        private ObservableCollection<PowerConsumptionEntity> availableEntities;
        public ObservableCollection<PowerConsumptionEntity> AvailableEntities
        {
            get { return availableEntities; }
            set { SetProperty(ref availableEntities, value); }
        }

        private PowerConsumptionEntity selectedEntity;
        public PowerConsumptionEntity SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                SetProperty(ref selectedEntity, value);
                if (value != null)
                {
                    LoadMeasurements();
                }
            }
        }

        private ObservableCollection<Measurement> measurements;
        public ObservableCollection<Measurement> Measurements
        {
            get { return measurements; }
            set { SetProperty(ref measurements, value); }
        }

        #endregion

        #region Constructor

        public MeasurementGraphViewModel()
        {
            measurementService = MeasurementService.Instance;
            LoadAvailableEntities();

            // Setup refresh timer for real-time updates
            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            refreshTimer.Tick += OnRefreshTimer;
            refreshTimer.Start();
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

            // Notify view to redraw chart
            OnPropertyChanged(nameof(Measurements));
        }

        private void OnRefreshTimer(object sender, EventArgs e)
        {
            // Refresh measurements if entity is selected
            if (SelectedEntity != null)
            {
                LoadMeasurements();
            }
        }

        #endregion

        #region Cleanup

        public void Cleanup()
        {
            refreshTimer?.Stop();
        }

        #endregion
    }
}