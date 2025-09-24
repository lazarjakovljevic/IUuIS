using NetworkService.Commands;
using NetworkService.Model;
using NetworkService.MVVM;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkService.Views
{
    public partial class NetworkDisplayView : UserControl
    {
        #region Singleton Pattern

        public static NetworkDisplayView Instance { get; private set; }

        #endregion

        #region Fields

        private PowerConsumptionEntity draggedEntity;
        private bool isDragging = false;
        private Dictionary<Canvas, PowerConsumptionEntity> canvasEntityMap;
        private ConnectionManager connectionManager;
        private Canvas lineCanvas;
        private bool isUndoOperation = false;

        #endregion

        #region Constructor

        public NetworkDisplayView()
        {
            InitializeComponent();
            InitializeComponents();

            Instance = this;

            this.DataContext = NetworkDisplayViewModel.Instance;

            RegisterForUpdates();

            RestoreViewState();
        }

        private void RegisterForUpdates()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
                {
                    mainViewModel.RegisterNetworkDisplayView(this);
                }
            }));
        }

        private void RestoreViewState()
        {
            var viewModel = this.DataContext as NetworkDisplayViewModel;
            if (viewModel != null)
            {
                var allCanvases = GetAllCanvases();
                viewModel.RestoreCanvasState(allCanvases, PlaceEntityOnCanvas);

                CreateAutomaticConnections();

            }
        }

        private Dictionary<Canvas, PowerConsumptionEntity> GetAllCanvases()
        {
            var allCanvases = new Dictionary<Canvas, PowerConsumptionEntity>();
            var networkGrid = this.FindName("NetworkGrid") as Grid;

            foreach (Canvas canvas in networkGrid.Children.OfType<Canvas>())
            {
                allCanvases[canvas] = null; 
            }

            return allCanvases;
        }

        public void SaveCurrentState()
        {
            var viewModel = this.DataContext as NetworkDisplayViewModel;
            viewModel?.SaveCanvasState(canvasEntityMap);
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            canvasEntityMap = new Dictionary<Canvas, PowerConsumptionEntity>();

            CreateLineCanvas();

            connectionManager = new ConnectionManager(lineCanvas, canvasEntityMap);

            connectionManager.ConnectionAdded += OnConnectionAdded;
            connectionManager.ConnectionRemoved += OnConnectionRemoved;
        }

        private void CreateLineCanvas()
        {
            lineCanvas = this.FindName("ConnectionLinesCanvas") as Canvas;

            if (lineCanvas == null)
            {
                lineCanvas = new Canvas { Background = Brushes.Transparent, IsHitTestVisible = false };
            }
        }
        #endregion

        #region Drag & Drop Events - TreeView to Canvas

        private void Entity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging) return;

            var border = sender as Border;
            var entity = border?.DataContext as PowerConsumptionEntity;

            if (entity != null)
            {
                isDragging = true;
                draggedEntity = entity;

                DragDrop.DoDragDrop(border, entity, DragDropEffects.Move);

                isDragging = false;
                draggedEntity = null;
            }
        }

        #endregion

        #region Canvas Events

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            var canvas = sender as Canvas;

            if (e.Data.GetDataPresent(typeof(PowerConsumptionEntity)))
            {

                if (canvasEntityMap.ContainsKey(canvas))
                {
                    e.Effects = DragDropEffects.None;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            var targetCanvas = sender as Canvas;

            if (e.Data.GetDataPresent(typeof(PowerConsumptionEntity)))
            {
                var entity = e.Data.GetData(typeof(PowerConsumptionEntity)) as PowerConsumptionEntity;

                if (entity != null && targetCanvas != null && !canvasEntityMap.ContainsKey(targetCanvas))
                {

                    var previousCanvas = canvasEntityMap.FirstOrDefault(x => x.Value.Id == entity.Id).Key;
                    if (previousCanvas != null)
                    {
                        RemoveEntityFromCanvas(previousCanvas, returnToTree: false);
                    }

                    PlaceEntityOnCanvas(targetCanvas, entity);


                    CreateAutomaticConnections();
                }
            }

            e.Handled = true;
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;

            if (canvas != null && canvasEntityMap.ContainsKey(canvas))
            {
                var entity = canvasEntityMap[canvas];
                var result = MessageBox.Show(
                    $"Remove '{entity.Name}' from this position?\nThis will also remove all connections.",
                    "Remove Entity",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RemoveEntityFromCanvas(canvas, returnToTree: true);
                }
            }
        }

        #endregion

        #region Canvas Management

        private void PlaceEntityOnCanvas(Canvas canvas, PowerConsumptionEntity entity)
        {
            var previousCanvas = canvasEntityMap.FirstOrDefault(x => x.Value?.Id == entity.Id).Key;

            canvas.Children.Clear();

            var entityVisual = CreateEntityVisual(entity);

            canvas.Children.Add(entityVisual);
            entityVisual.MouseLeftButtonDown += EntityOnCanvas_MouseLeftButtonDown;

            canvasEntityMap[canvas] = entity;

            UpdateCanvasAppearance(canvas, entity);

            var viewModel = this.DataContext as NetworkDisplayViewModel;
            if (viewModel?.SharedEntities.Contains(entity) == true)
            {
                viewModel?.RemoveEntityFromTree(entity);
            }

            if (!isUndoOperation)
            {
                var moveCommand = new MoveEntityCommand(entity, previousCanvas, canvas);
                UndoManager.Instance.ExecuteCommand(moveCommand);
            }

        }
        private void RemoveEntityFromCanvas(Canvas canvas, bool returnToTree = true)
        {
            if (canvasEntityMap.ContainsKey(canvas))
            {
                var entity = canvasEntityMap[canvas];

                connectionManager.RemoveConnectionsForEntity(entity);

                canvas.Children.Clear();
                RestoreCanvasDefaultAppearance(canvas);

                canvasEntityMap.Remove(canvas);

                if (returnToTree)
                {
                    var viewModel = this.DataContext as NetworkDisplayViewModel;
                    viewModel?.AddEntityToTree(entity);

                    if (!isUndoOperation)
                    {
                        var removeCommand = new MoveEntityCommand(entity, canvas, null); // null = TreeView
                        UndoManager.Instance.ExecuteCommand(removeCommand);
                    }
                }

                CreateAutomaticConnections();

            }
        }

        private void RestoreCanvasDefaultAppearance(Canvas canvas)
        {
            // Restore original border and background
            var border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8DEE9")),
                BorderThickness = new Thickness(2),
                Width = 110,
                Height = 130,
                CornerRadius = new CornerRadius(8)
            };

            var hint = new TextBlock
            {
                Text = "Drop Here",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C566A"))
            };

            border.Child = hint;
            canvas.Children.Add(border);

            // Reset canvas background
            canvas.Background = Brushes.Transparent;
        }

        private StackPanel CreateEntityVisual(PowerConsumptionEntity entity)
        {
            var visual = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand 
            };

            var image = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(entity.Type.ImagePath, UriKind.RelativeOrAbsolute)),
                Width = 100,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
                Stretch = Stretch.Uniform
            };

            var idAndValue = new TextBlock
            {
                Text = $"ID: {entity.Id}  [{entity.CurrentValue:F2} kWh]",
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 2),
                Foreground = entity.IsValueValid ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red)
            };

            visual.Children.Add(image);
            visual.Children.Add(idAndValue);

            return visual;
        }

        private void UpdateCanvasAppearance(Canvas canvas, PowerConsumptionEntity entity)
        {
            canvas.Background = Brushes.Transparent;   
        }

        #endregion

        #region Connection Management

        private void CreateAutomaticConnections()
        {
            connectionManager.ClearAllConnections();

            var entitiesCount = canvasEntityMap.Count;

            connectionManager.CreateAutomaticConnections();

            var connectionsCount = connectionManager.Connections.Count;

            bool linesVisible = lineCanvas?.Visibility == Visibility.Visible;
            UpdateConnectionCountDisplay(linesVisible);

            RefreshEntityVisuals();
        }


        private void RefreshEntityVisuals()
        {
            foreach (var kvp in canvasEntityMap.ToList())
            {
                var canvas = kvp.Key;
                var entity = kvp.Value;

                canvas.Children.Clear();
                var entityVisual = CreateEntityVisual(entity);
                canvas.Children.Add(entityVisual);
                entityVisual.MouseLeftButtonDown += EntityOnCanvas_MouseLeftButtonDown;

                UpdateCanvasAppearance(canvas, entity);
            }
        }

        #endregion

        #region Canvas-to-Canvas Drag & Drop

        private void EntityOnCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging) return;

            var entityVisual = sender as StackPanel;
            if (entityVisual == null) return;

            Canvas sourceCanvas = null;
            foreach (var kvp in canvasEntityMap)
            {
                if (kvp.Key.Children.Contains(entityVisual))
                {
                    sourceCanvas = kvp.Key;
                    break;
                }
            }

            if (sourceCanvas != null && canvasEntityMap.ContainsKey(sourceCanvas))
            {
                var entity = canvasEntityMap[sourceCanvas];
                isDragging = true;
                draggedEntity = entity;

                DragDrop.DoDragDrop(entityVisual, entity, DragDropEffects.Move);

                isDragging = false;
                draggedEntity = null;
            }
        }

        #endregion

        #region Connection Events

        private void OnConnectionAdded(Connection connection)
        {
            connection.UpdateLinePosition();

            RefreshEntityVisuals();
        }

        private void OnConnectionRemoved(Connection connection)
        {
            RefreshEntityVisuals();
        }

        #endregion

        #region Public Methods

        public void RefreshEntityDisplays()
        {
            foreach (var kvp in canvasEntityMap.ToList())
            {
                var canvas = kvp.Key;
                var entity = kvp.Value;

                PlaceEntityOnCanvas(canvas, entity);
            }

            connectionManager.UpdateAllLinePositions();
            connectionManager.UpdateAllLineColors();
        }

        public void UpdateEntityValue(PowerConsumptionEntity entity)
        {
            if (entity == null) return;

            var canvas = canvasEntityMap.FirstOrDefault(x => x.Value.Id == entity.Id).Key;
            if (canvas != null)
            {
                PlaceEntityOnCanvas(canvas, entity);

                connectionManager.UpdateAllLineColors();
            }
        }

        public string GetNetworkStatistics()
        {
            var entitiesCount = canvasEntityMap.Count;
            var connectionsCount = connectionManager.Connections.Count;
            var validEntities = canvasEntityMap.Values.Count(e => e.IsValueValid);
            var invalidEntities = entitiesCount - validEntities;

            return $"Entities: {entitiesCount}, Connections: {connectionsCount}, Valid: {validEntities}, Invalid: {invalidEntities}";
        }

        public void ToggleConnectionDisplay()
        {
            if (lineCanvas != null)
            {
                bool willBeVisible = lineCanvas.Visibility == Visibility.Hidden;
                lineCanvas.Visibility = willBeVisible ? Visibility.Visible : Visibility.Hidden;

                UpdateConnectionCountDisplay(willBeVisible);
            }
        }

        #endregion

        #region Cleanup

        public void Cleanup()
        {
            if (connectionManager != null)
            {
                connectionManager.ConnectionAdded -= OnConnectionAdded;
                connectionManager.ConnectionRemoved -= OnConnectionRemoved;
                connectionManager.ClearAllConnections();
            }

            canvasEntityMap?.Clear();
        }

        #endregion

        #region Button Event Handlers

        private void ToggleConnectionsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleConnectionDisplay();
        }

        private void ClearNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanClearNetwork())
                return;

            var result = MessageBox.Show(
                "Are you sure you want to remove all entities from the network?",
                "Clear Network",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClearNetwork();
            }
        }

        private bool CanClearNetwork()
        {
            return canvasEntityMap.Count > 0;
        }

        private bool CanToggleConnections()
        {
            return connectionManager.Connections.Count > 0;
        }

        #endregion

        private void UpdateButtonStates()
        {
            var toggleButton = this.FindName("ToggleConnectionsButton") as Button;
            var clearButton = this.FindName("ClearNetworkButton") as Button;

            if (toggleButton != null)
            {
                toggleButton.IsEnabled = CanToggleConnections();
            }

            if (clearButton != null)
            {
                clearButton.IsEnabled = CanClearNetwork();
            }
        }

        private void ClearNetwork()
        {

            var entitiesToReturn = canvasEntityMap.Values.ToList();

            foreach (var entity in entitiesToReturn)
            {
                var canvas = canvasEntityMap.FirstOrDefault(x => x.Value.Id == entity.Id).Key;
                if (canvas != null)
                {
                    RemoveEntityFromCanvas(canvas, returnToTree: true);
                }
            }

            connectionManager.ClearAllConnections();

            if (this.FindName("ConnectionStatusText") is TextBlock statusText)
            {
                statusText.Text = "0 connections";
            }

            UpdateButtonStates();
        }

        #region Undo Support
        public void PlaceEntityOnCanvasUndo(Canvas canvas, PowerConsumptionEntity entity)
        {
            isUndoOperation = true;
            PlaceEntityOnCanvas(canvas, entity);
            CreateAutomaticConnections(); // Restore connections
            isUndoOperation = false;
        }
        public void RemoveEntityFromCanvasUndo(Canvas canvas, bool returnToTree)
        {
            isUndoOperation = true;
            RemoveEntityFromCanvas(canvas, returnToTree);
            isUndoOperation = false;
        }

        private void UpdateConnectionCountDisplay(bool showConnections)
        {
            if (this.FindName("ConnectionStatusText") is TextBlock statusText)
            {
                int actualConnections = connectionManager.Connections.Count;

                if (showConnections)
                {
                    statusText.Text = $"{actualConnections} connections";
                }
                else
                {
                    statusText.Text = "0 connections";
                }
            }
        }

        #endregion
    }

    #region Helper Classes

    // Helper class for TreeView grouping
    public class EntityGroup : BindableBase
    {
        public string TypeName { get; set; }

        private ObservableCollection<PowerConsumptionEntity> entities;
        public ObservableCollection<PowerConsumptionEntity> Entities
        {
            get { return entities; }
            set
            {
                if (entities != null)
                    entities.CollectionChanged -= OnEntitiesChanged;

                SetProperty(ref entities, value);

                if (entities != null)
                    entities.CollectionChanged += OnEntitiesChanged;
            }
        }

        public int Count => Entities?.Count ?? 0;

        public EntityGroup(string typeName)
        {
            TypeName = typeName;
            Entities = new ObservableCollection<PowerConsumptionEntity>();
        }

        private void OnEntitiesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
        }
    }
    #endregion
}