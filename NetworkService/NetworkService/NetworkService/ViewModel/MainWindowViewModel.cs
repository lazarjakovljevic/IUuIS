using NetworkService.Model;
using NetworkService.MVVM;
using NetworkService.Services;
using NetworkService.Views;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        #region Properties

        private BindableBase currentViewModel;
        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set { SetProperty(ref currentViewModel, value); }
        }

        // Shared entities collection for all ViewModels
        public static ObservableCollection<PowerConsumptionEntity> SharedEntities { get; set; }

        // Reference to NetworkDisplayView for connection updates
        private NetworkDisplayView networkDisplayView;

        #endregion

        #region Commands

        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand HomeCommand { get; private set; }
        public MyICommand UndoCommand { get; private set; }

        #endregion

        #region TCP Communication

        private int count = 3; // Initial number of objects in system
        private MeasurementService measurementService;

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            InitializeSharedData();
            InitializeCommands();
            InitializeServices();
            CreateListener(); // TCP connection setup

            // Set initial view to home
            CurrentViewModel = new HomeViewModel();
        }

        #endregion

        #region Initialization

        private void InitializeSharedData()
        {
            if (SharedEntities == null)
            {
                SharedEntities = new ObservableCollection<PowerConsumptionEntity>();
                LoadInitialEntities();
            }
        }

        private void LoadInitialEntities()
        {
            // Load some initial entities
            SharedEntities.Add(new PowerConsumptionEntity(0, "Main Building Meter", EntityType.SmartMeter) { CurrentValue = 1.2 }); // NORMAL
            SharedEntities.Add(new PowerConsumptionEntity(1, "Workshop Meter", EntityType.IntervalMeter) { CurrentValue = 2.1 }); // NORMAL
            SharedEntities.Add(new PowerConsumptionEntity(2, "Office Complex", EntityType.SmartMeter) { CurrentValue = 0.2 }); // ALERT
        }

        private void InitializeServices()
        {
            measurementService = MeasurementService.Instance;

            // Subscribe to measurement service events
            measurementService.MeasurementReceived += OnMeasurementReceived;
            measurementService.AlertTriggered += OnAlertTriggered;
        }

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            NavCommand = new MyICommand<string>(OnNav);
            HomeCommand = new MyICommand(OnHome);
            UndoCommand = new MyICommand(OnUndo);
        }

        private void OnNav(string destination)
        {
            switch (destination?.ToLower())
            {
                case "entities":
                    CurrentViewModel = new NetworkEntitiesViewModel();
                    break;
                case "display":
                    var displayViewModel = new NetworkDisplayViewModel();
                    CurrentViewModel = displayViewModel;

                    // Store reference to the view for connection updates
                    // This will be set when the view is actually created
                    break;
                case "graph":
                    CurrentViewModel = new MeasurementGraphViewModel();
                    break;
                case "home":
                default:
                    CurrentViewModel = new HomeViewModel();
                    break;
            }
        }

        private void OnHome()
        {
            CurrentViewModel = new HomeViewModel();
        }

        private void OnUndo()
        {
            MessageBox.Show("Undo functionality will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region TCP Server Implementation

        private void CreateListener()
        {
            try
            {
                var tcp = new TcpListener(IPAddress.Any, 25675);
                tcp.Start();

                var listeningThread = new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            var tcpClient = tcp.AcceptTcpClient();
                            ThreadPool.QueueUserWorkItem(param =>
                            {
                                try
                                {
                                    // Receive message
                                    NetworkStream stream = tcpClient.GetStream();
                                    string incoming;
                                    byte[] bytes = new byte[1024];
                                    int i = stream.Read(bytes, 0, bytes.Length);
                                    incoming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                                    // Process different message types
                                    if (incoming.Equals("Need object count"))
                                    {
                                        // Response - send count of monitored objects
                                        int currentCount = SharedEntities?.Count ?? count;
                                        byte[] data = System.Text.Encoding.ASCII.GetBytes(currentCount.ToString());
                                        stream.Write(data, 0, data.Length);

                                        Console.WriteLine($"Unknown message received: {incoming}");
                                        measurementService.LogError($"Unknown TCP message: {incoming}");
                                    }

                                    stream.Close();
                                    tcpClient.Close();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error handling TCP client: {ex.Message}");
                                    measurementService.LogError($"TCP client error: {ex.Message}");
                                }
                            }, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in TCP listener: {ex.Message}");
                        measurementService.LogError($"TCP listener error: {ex.Message}");
                    }
                });

                listeningThread.IsBackground = true;
                listeningThread.Start();

                Console.WriteLine("TCP Server started on port 25675");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting TCP server: {ex.Message}", "TCP Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Event Handlers

        private void OnMeasurementReceived(PowerConsumptionEntity entity, double value)
        {
            // This runs on background thread, so we need to dispatch to UI thread
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Update NetworkEntitiesView if it's currently active
                    if (CurrentViewModel is NetworkEntitiesViewModel entitiesViewModel)
                    {
                        var entities = entitiesViewModel.Entities;
                        var index = entities.IndexOf(entity);

                        if (index >= 0)
                        {
                            // Remove and re-add to force DataGrid update
                            entities.RemoveAt(index);
                            entities.Insert(index, entity);

                            // Also refresh the filtered view
                            entitiesViewModel.FilteredEntities?.Refresh();
                        }
                    }

                    // Update NetworkDisplayView if it's currently active
                    if (CurrentViewModel is NetworkDisplayViewModel)
                    {
                        // Find the actual NetworkDisplayView instance
                        // Since we can't directly access the view from ViewModel in pure MVVM,
                        // we'll use the measurement service to notify about value changes
                        UpdateNetworkDisplayConnections(entity);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating UI: {ex.Message}");
                }
            }));
        }

        private void OnAlertTriggered(string alertMessage)
        {
            // Show alert on UI thread
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBox.Show(alertMessage, "Measurement Alert",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }));
        }

        #endregion

        #region Network Display Support

        /// <summary>
        /// Register NetworkDisplayView for connection updates
        /// This method should be called from NetworkDisplayView constructor
        /// </summary>
        public void RegisterNetworkDisplayView(NetworkDisplayView view)
        {
            networkDisplayView = view;
        }

        /// <summary>
        /// Update connections in NetworkDisplayView when entity value changes
        /// </summary>
        private void UpdateNetworkDisplayConnections(PowerConsumptionEntity entity)
        {
            if (networkDisplayView != null)
            {
                try
                {
                    // Update the entity value and refresh connections
                    networkDisplayView.UpdateEntityValue(entity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating network display connections: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handle entity deletion from the shared collection
        /// This method should be called when entity is deleted from NetworkEntitiesView
        /// </summary>
        public static void OnEntityDeleted(PowerConsumptionEntity entity)
        {
            if (entity == null) return;

            // The entity will be automatically removed from TreeView via data binding
            // Connection cleanup will be handled by NetworkDisplayView when it detects the entity removal
            Console.WriteLine($"Entity {entity.Name} (ID: {entity.Id}) was deleted from shared collection");
        }

        /// <summary>
        /// Handle entity addition to the shared collection
        /// </summary>
        public static void OnEntityAdded(PowerConsumptionEntity entity)
        {
            if (entity == null) return;

            Console.WriteLine($"Entity {entity.Name} (ID: {entity.Id}) was added to shared collection");

            // Entity will automatically appear in TreeView via data binding
            // No connection management needed here since entity is not on network grid yet
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get current network statistics
        /// </summary>
        public string GetNetworkStatistics()
        {
            if (networkDisplayView != null)
            {
                return networkDisplayView.GetNetworkStatistics();
            }

            return $"Entities in system: {SharedEntities?.Count ?? 0}";
        }

        /// <summary>
        /// Update entity count (called when entities are added/removed)
        /// </summary>
        public static void UpdateEntityCount()
        {
            // This method can be called when entities are added/removed
            // to notify other components about count changes
            Console.WriteLine($"Entity count updated: {SharedEntities?.Count ?? 0} entities");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup resources when application shuts down
        /// </summary>
        public void Cleanup()
        {
            if (measurementService != null)
            {
                measurementService.MeasurementReceived -= OnMeasurementReceived;
                measurementService.AlertTriggered -= OnAlertTriggered;
            }

            if (networkDisplayView != null)
            {
                networkDisplayView.Cleanup();
                networkDisplayView = null;
            }
        }

        #endregion
    }

    #region Other ViewModels
    public class MeasurementGraphViewModel : BindableBase
    {
        // Bar chart showing last 5 measurements
    }

    #endregion
}