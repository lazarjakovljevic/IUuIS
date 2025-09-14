using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NetworkService.Model;

namespace NetworkService.Views
{
    public partial class NetworkDisplayView : UserControl
    {
        #region Fields

        private PowerConsumptionEntity draggedEntity;
        private bool isDragging = false;
        private Dictionary<Canvas, PowerConsumptionEntity> canvasEntityMap;

        #endregion

        #region Constructor

        public NetworkDisplayView()
        {
            InitializeComponent();
            canvasEntityMap = new Dictionary<Canvas, PowerConsumptionEntity>();
        }

        #endregion

        #region Drag & Drop Events

        private void Entity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging) return;

            var border = sender as Border;
            var entity = border?.DataContext as PowerConsumptionEntity;

            if (entity != null)
            {
                isDragging = true;
                draggedEntity = entity;

                // Start drag & drop operation
                DragDrop.DoDragDrop(border, entity, DragDropEffects.Move);

                // Reset drag state
                isDragging = false;
                draggedEntity = null;
            }
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            var canvas = sender as Canvas;

            if (e.Data.GetDataPresent(typeof(PowerConsumptionEntity)))
            {
                // Check if canvas is already occupied
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
            var canvas = sender as Canvas;

            if (e.Data.GetDataPresent(typeof(PowerConsumptionEntity)))
            {
                var entity = e.Data.GetData(typeof(PowerConsumptionEntity)) as PowerConsumptionEntity;

                if (entity != null && canvas != null && !canvasEntityMap.ContainsKey(canvas))
                {
                    // Remove entity from previous canvas if it was already placed
                    var previousCanvas = canvasEntityMap.FirstOrDefault(x => x.Value.Id == entity.Id).Key;
                    if (previousCanvas != null)
                    {
                        RemoveEntityFromCanvas(previousCanvas);
                    }

                    // Place entity on new canvas
                    PlaceEntityOnCanvas(canvas, entity);
                }
            }

            e.Handled = true;
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;

            if (canvas != null && canvasEntityMap.ContainsKey(canvas))
            {
                var result = MessageBox.Show(
                    $"Remove '{canvasEntityMap[canvas].Name}' from this position?",
                    "Remove Entity",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RemoveEntityFromCanvas(canvas);
                }
            }
        }

        #endregion

        #region Canvas Management

        private void PlaceEntityOnCanvas(Canvas canvas, PowerConsumptionEntity entity)
        {
            // Clear canvas
            canvas.Children.Clear();

            // Create entity visual representation
            var entityVisual = CreateEntityVisual(entity);

            // Add to canvas
            canvas.Children.Add(entityVisual);

            // Track entity placement
            canvasEntityMap[canvas] = entity;

            // Update canvas appearance
            UpdateCanvasAppearance(canvas, entity);

            Console.WriteLine($"Placed entity {entity.Name} on canvas {canvas.Name}");
        }

        private void RemoveEntityFromCanvas(Canvas canvas)
        {
            if (canvasEntityMap.ContainsKey(canvas))
            {
                var entity = canvasEntityMap[canvas];

                // Clear canvas
                canvas.Children.Clear();

                // Add back drop hint
                var hint = new TextBlock
                {
                    Text = "Drop Here",
                    FontSize = 10,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C566A"))
                };
                Canvas.SetLeft(hint, 20);
                Canvas.SetTop(hint, 40);
                canvas.Children.Add(hint);

                // Reset canvas appearance
                canvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECEFF4"));

                // Remove from tracking
                canvasEntityMap.Remove(canvas);

                Console.WriteLine($"Removed entity {entity.Name} from canvas {canvas.Name}");
            }
        }

        private StackPanel CreateEntityVisual(PowerConsumptionEntity entity)
        {
            var visual = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Entity icon/symbol
            var icon = new TextBlock
            {
                Text = entity.Type.Name.Contains("Smart") ? "🔌" : "⚡",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Entity name
            var name = new TextBlock
            {
                Text = entity.Name,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 80
            };

            // Entity ID
            var id = new TextBlock
            {
                Text = $"ID: {entity.Id}",
                FontSize = 8,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C566A"))
            };

            // Current value
            var value = new TextBlock
            {
                Text = $"{entity.CurrentValue:F2} kWh",
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                Foreground = entity.IsValueValid ?
                    new SolidColorBrush(Colors.Green) :
                    new SolidColorBrush(Colors.Red)
            };

            visual.Children.Add(icon);
            visual.Children.Add(name);
            visual.Children.Add(id);
            visual.Children.Add(value);

            return visual;
        }

        private void UpdateCanvasAppearance(Canvas canvas, PowerConsumptionEntity entity)
        {
            // Change background color based on entity status
            if (entity.IsValueValid)
            {
                canvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E8")); // Light green
            }
            else
            {
                canvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE8E8")); // Light red
            }
        }

        #endregion

        #region Public Methods

        public void RefreshEntityDisplays()
        {
            // Refresh all displayed entities with current values
            foreach (var kvp in canvasEntityMap.ToList())
            {
                var canvas = kvp.Key;
                var entity = kvp.Value;

                // Update visual representation
                PlaceEntityOnCanvas(canvas, entity);
            }
        }

        #endregion
    }

    #region Helper Classes

    // Helper class for TreeView grouping
    public class EntityGroup
    {
        public string TypeName { get; set; }
        public List<PowerConsumptionEntity> Entities { get; set; }
        public int Count => Entities?.Count ?? 0;

        public EntityGroup(string typeName)
        {
            TypeName = typeName;
            Entities = new List<PowerConsumptionEntity>();
        }
    }

    #endregion
}