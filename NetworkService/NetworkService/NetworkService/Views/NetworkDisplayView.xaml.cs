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

            // Set DataContext to ViewModel
            this.DataContext = new NetworkService.ViewModel.NetworkDisplayViewModel();
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

                // Remove from tracking
                canvasEntityMap.Remove(canvas);
            }
        }

        private FrameworkElement CreateEntityVisual(PowerConsumptionEntity entity)
        {
            var container = new Grid
            {
                Width = 120,
                Height = 120
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var image = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(entity.Type.ImagePath, UriKind.RelativeOrAbsolute)),
                Width = 100,
                Height = 100,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 2, 0, 0), // pomera sliku gore
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Tekst
            var nameAndValue = new TextBlock
            {
                Text = entity.Name,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 120,
                Margin = new Thickness(0,5,0,0),
                Foreground = Brushes.DarkSlateGray
            };

            stack.Children.Add(image);
            stack.Children.Add(nameAndValue);

            container.Children.Add(stack);
            return container;
        }

        private void UpdateCanvasAppearance(Canvas canvas, PowerConsumptionEntity entity)
        {
            if (entity.IsValueValid)
            {
                canvas.Background = Brushes.Transparent;
            }
            else
            {
                canvas.Background = Brushes.Transparent;
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