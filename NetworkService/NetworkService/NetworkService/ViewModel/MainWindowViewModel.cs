using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using NetworkService.MVVM;
using NetworkService.Model;
using NetworkService.Services;

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
                    CurrentViewModel = new NetworkDisplayViewModel();
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

                                        Console.WriteLine($"Sent object count: {currentCount}");
                                    }
                                    else if (incoming.StartsWith("Entitet_") && incoming.Contains(":"))
                                    {
                                        // Measurement data received
                                        Console.WriteLine($"Received measurement: {incoming}");

                                        // Process through measurement service
                                        measurementService.ProcessTcpMessage(incoming, SharedEntities);
                                    }
                                    else
                                    {
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
                    // 4 SATA KASNIJE OVO JEDINO RADI, AMIN
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

        #region Public Methods

        public static void UpdateEntityCount()
        {
            // This method can be called when entities are added/removed
            // to notify other components about count changes
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