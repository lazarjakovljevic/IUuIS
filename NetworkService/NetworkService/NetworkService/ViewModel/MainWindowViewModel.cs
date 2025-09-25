using NetworkService.Commands;
using NetworkService.Model;
using NetworkService.MVVM;
using NetworkService.Services;
using NetworkService.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public static ObservableCollection<PowerConsumptionEntity> SharedEntities { get; set; }

        private NetworkDisplayView networkDisplayView;

        #endregion

        #region Commands

        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand HomeCommand { get; private set; }
        public MyICommand UndoCommand { get; private set; }

        #endregion

        #region TCP Communication

        //private int count = 6; 
        private MeasurementService measurementService;

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            InitializeServices();
            InitializeSharedData();
            InitializeCommands();
            CreateListener(); 

            UndoManager.UndoStackChanged += () =>
            {
                UndoCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanUndoActions)); 
            };

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

                measurementService.LoadLastMeasurementsFromFile(SharedEntities);
            }
        }

        private void LoadInitialEntities()
        {
            // Load initial entities
            SharedEntities.Add(new PowerConsumptionEntity(0, "Main Building Meter", EntityType.SmartMeter) { CurrentValue = 1.2 });     // NORMAL
            SharedEntities.Add(new PowerConsumptionEntity(1, "Workshop Meter", EntityType.IntervalMeter) { CurrentValue = 2.1 });       // NORMAL
            SharedEntities.Add(new PowerConsumptionEntity(2, "Office Complex", EntityType.SmartMeter) { CurrentValue = 0.2 });          // ALERT
            SharedEntities.Add(new PowerConsumptionEntity(3, "Factory Line A", EntityType.IntervalMeter) { CurrentValue = 1.8 });       // NORMAL
            SharedEntities.Add(new PowerConsumptionEntity(4, "Warehouse East", EntityType.SmartMeter) { CurrentValue = 3.1 });          // ALERT  
            SharedEntities.Add(new PowerConsumptionEntity(5, "Server Room", EntityType.IntervalMeter) { CurrentValue = 0.9 });          // NORMAL
        }

        private void InitializeServices()
        {
            measurementService = MeasurementService.Instance;

            measurementService.MeasurementReceived += OnMeasurementReceived;
            measurementService.AlertTriggered += OnAlertTriggered;
        }

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            HomeCommand = new MyICommand(OnHome);
            NavCommand = new MyICommand<string>(OnNav);
            UndoCommand = new MyICommand(OnUndo, CanUndo);
        }

        private void OnNav(string destination)
        {
            if (CurrentViewModel is NetworkDisplayViewModel && networkDisplayView != null)
            {
                networkDisplayView.SaveCurrentState();
            }

            switch (destination?.ToLower())
            {
                case "entities":
                    CurrentViewModel = NetworkEntitiesViewModel.Instance; 
                    break;
                case "display":
                    CurrentViewModel = NetworkDisplayViewModel.Instance;
                    break;
                case "graph":
                    CurrentViewModel = MeasurementGraphViewModel.Instance;
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
                                    NetworkStream stream = tcpClient.GetStream();
                                    string incoming;
                                    byte[] bytes = new byte[1024];
                                    int i = stream.Read(bytes, 0, bytes.Length);
                                    incoming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                                    if (incoming.Equals("Need object count"))
                                    {
                                        int maxIdPlusOne = 0;
                                        if (SharedEntities != null && SharedEntities.Count > 0)
                                        {
                                            maxIdPlusOne = SharedEntities.Max(e => e.Id) + 1;
                                        }

                                        byte[] data = System.Text.Encoding.ASCII.GetBytes(maxIdPlusOne.ToString());
                                        stream.Write(data, 0, data.Length);

                                        Console.WriteLine($"Sent: {maxIdPlusOne}");
                                    }
                                    else if (incoming.StartsWith("Entitet_") && incoming.Contains(":"))
                                    {
                                        Console.WriteLine($"Received measurement: {incoming}");

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
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (CurrentViewModel is NetworkEntitiesViewModel entitiesViewModel)
                    {
                        var entities = entitiesViewModel.Entities;
                        var index = entities.IndexOf(entity);

                        if (index >= 0)
                        {
                            entities.RemoveAt(index);
                            entities.Insert(index, entity);

                            entitiesViewModel.FilteredEntities?.Refresh();
                        }
                    }

                    if (CurrentViewModel is NetworkDisplayViewModel)
                    {
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
            // UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    alertMessage,                           
                    "Alert",         
                    MessageBoxButton.OK,                    
                    MessageBoxImage.Warning                
                );
            });
        }
        #endregion

        #region Network Display Support

        public void RegisterNetworkDisplayView(NetworkDisplayView view)
        {
            networkDisplayView = view;
        }

        private void UpdateNetworkDisplayConnections(PowerConsumptionEntity entity)
        {
            if (networkDisplayView != null)
            {
                try
                {
                    networkDisplayView.UpdateEntityValue(entity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating network display connections: {ex.Message}");
                }
            }
        }

        public static void OnEntityDeleted(PowerConsumptionEntity entity)
        {
            if (entity == null) return;

            Console.WriteLine($"Entity {entity.Name} (ID: {entity.Id}) was deleted from shared collection");
        }

        public static void OnEntityAdded(PowerConsumptionEntity entity)
        {
            if (entity == null) return;

            Console.WriteLine($"Entity {entity.Name} (ID: {entity.Id}) was added to shared collection");

            // amin, databinding ce konacno bindovati entitete u treeview 
        }

        #endregion

        #region Public Methods

        public string GetNetworkStatistics()
        {
            if (networkDisplayView != null)
            {
                return networkDisplayView.GetNetworkStatistics();
            }

            return $"Entities in system: {SharedEntities?.Count ?? 0}";
        }

        #endregion

        #region Undo Functionality

        private UndoManager undoManager;

        public UndoManager UndoManager => undoManager ?? (undoManager = UndoManager.Instance);

        public bool CanUndoActions => UndoManager.CanUndo;

        private void OnUndo()
        {
            if (!UndoManager.CanUndo)
            {
                Console.WriteLine("No actions to undo");
                return;
            }

            bool success = UndoManager.Undo();
            if (!success)
            {
                Console.WriteLine("Undo failed");
            }
        }

        private bool CanUndo()
        {
            return true; 
        }

        #endregion

    }

}