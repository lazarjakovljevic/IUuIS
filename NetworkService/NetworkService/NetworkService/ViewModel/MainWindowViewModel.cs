using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using NetworkService.MVVM;
using NetworkService.Views;

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

        #endregion

        #region Commands

        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand HomeCommand { get; private set; }
        public MyICommand UndoCommand { get; private set; }

        #endregion

        #region TCP Communication

        private int count = 15; // Initial number of objects in system

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            InitializeCommands();
            CreateListener(); // TCP connection setup

            // Set initial view to home
            CurrentViewModel = new HomeViewModel();
        }

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            NavCommand = new MyICommand<string>(OnNav);
            HomeCommand = new MyICommand(OnHome);
            UndoCommand = new MyICommand(OnUndo); // Removed CanUndo - always enabled now
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
            // TODO: Implement undo functionality
            MessageBox.Show("Undo functionality will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region TCP Server Implementation

        private void CreateListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25675);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        // Receive message
                        NetworkStream stream = tcpClient.GetStream();
                        string incoming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        // Received message is saved in incoming string
                        incoming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        // If received message is asking how many objects are in system -> response
                        if (incoming.Equals("Need object count"))
                        {
                            // Response - send count of monitored objects
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            // Otherwise, server sent state change of some object in system
                            Console.WriteLine(incoming); // For example: "Entitet_1:272"

                            // TODO: Process message to get information about the change
                            // Update necessary things in application
                            ProcessIncomingMeasurement(incoming);
                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        private void ProcessIncomingMeasurement(string message)
        {
            // TODO: Implement processing of incoming measurements
            // Format: "Entitet_ID:Value"
            // This should:
            // 1. Parse the message
            // 2. Update entity value
            // 3. Log to file
            // 4. Update UI if entity is displayed
        }

        #endregion
    }

    #region Placeholder ViewModels (to be implemented later)

    public class NetworkDisplayViewModel : BindableBase
    {
        // Drag&Drop grid with visual entities
    }

    public class MeasurementGraphViewModel : BindableBase
    {
        // Bar chart showing last 5 measurements
    }

    #endregion
}