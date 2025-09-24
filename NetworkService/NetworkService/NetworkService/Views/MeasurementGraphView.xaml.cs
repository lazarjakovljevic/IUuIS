using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class MeasurementGraphView : UserControl
    {
        private MeasurementGraphViewModel viewModel;

        public MeasurementGraphView()
        {
            InitializeComponent();

            viewModel = MeasurementGraphViewModel.Instance;
            this.DataContext = viewModel;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            DrawChart();
        }


        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.Measurements) || e.PropertyName == nameof(viewModel.SelectedEntity))
            {
                DrawChart();
            }
        }

        private void DrawChart()
        {
            ChartCanvas.Children.Clear();

            if (viewModel.Measurements == null || !viewModel.Measurements.Any())
            {
                DrawNoDataMessage();
                return;
            }

            DrawAxes();
            DrawBars();
            DrawLabels();
        }

        private void DrawNoDataMessage()
        {
            var message = new TextBlock
            {
                Text = "No measurement data available",
                FontSize = 16,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(message, ChartCanvas.Width / 2 - 100);
            Canvas.SetTop(message, ChartCanvas.Height / 2);
            ChartCanvas.Children.Add(message);
        }

        private void DrawAxes()
        {
            // Y-axis (vertical)
            var yAxis = new Line
            {
                X1 = 40,
                Y1 = 20,
                X2 = 40,
                Y2 = ChartCanvas.Height - 40,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            ChartCanvas.Children.Add(yAxis);

            // X-axis (horizontal)
            var xAxis = new Line
            {
                X1 = 40,
                Y1 = ChartCanvas.Height - 40,
                X2 = ChartCanvas.Width - 20,
                Y2 = ChartCanvas.Height - 40,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            ChartCanvas.Children.Add(xAxis);

            // Y-axis label
            var yLabel = new TextBlock
            {
                Text = "Value (kWh)",
                FontSize = 12,
                Foreground = Brushes.Black,
                RenderTransform = new RotateTransform(-90)
            };
            Canvas.SetLeft(yLabel, -15);
            Canvas.SetTop(yLabel, ChartCanvas.Height / 2);
            ChartCanvas.Children.Add(yLabel);

            // X-axis label
            var xLabel = new TextBlock
            {
                Text = "Time",
                FontSize = 12,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(xLabel, ChartCanvas.Width / 2 - 20);
            Canvas.SetTop(xLabel, ChartCanvas.Height - 15);
            ChartCanvas.Children.Add(xLabel);
        }


        private void DrawBars()
        {
            var measurements = viewModel.Measurements.ToList();
            if (!measurements.Any()) return;

            // Chart dimensions
            double chartWidth = ChartCanvas.Width - 60; // Leave space for axes
            double chartHeight = ChartCanvas.Height - 60;
            double barWidth = chartWidth / measurements.Count * 0.8; // 80% width, 20% spacing
            double spacing = chartWidth / measurements.Count * 0.2;

            double maxValue = Math.Max(measurements.Max(m => m.Value), 3.0); 

            for (int i = 0; i < measurements.Count; i++)
            {
                var measurement = measurements[i];

                double barHeight = (measurement.Value / maxValue) * chartHeight;

                // Calculate bar position
                double x = 50 + (i * (barWidth + spacing));
                double y = ChartCanvas.Height - 40 - barHeight; // Start from bottom

                Brush barColor = measurement.IsValid ? Brushes.Green : Brushes.Red;

                // Create bar
                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = barColor,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, y);
                ChartCanvas.Children.Add(bar);

                var valueLabel = new TextBlock
                {
                    Text = measurement.Value.ToString("F2"),
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Canvas.SetLeft(valueLabel, x + barWidth / 2 - 15);
                Canvas.SetTop(valueLabel, y - 20);
                ChartCanvas.Children.Add(valueLabel);
            }
        }

        private void DrawLabels()
        {
            var measurements = viewModel.Measurements.ToList();
            if (!measurements.Any()) return;

            double chartWidth = ChartCanvas.Width - 60;
            double barWidth = chartWidth / measurements.Count * 0.8;
            double spacing = chartWidth / measurements.Count * 0.2;

            DrawYAxisLabels();

            for (int i = 0; i < measurements.Count; i++)
            {
                var measurement = measurements[i];
                double x = 50 + (i * (barWidth + spacing));

                var timeLabel = new TextBlock
                {
                    Text = measurement.Timestamp.ToString("HH:mm:ss"),
                    FontSize = 9,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(timeLabel, x + barWidth / 2 - 20);
                Canvas.SetTop(timeLabel, ChartCanvas.Height - 30);
                ChartCanvas.Children.Add(timeLabel);
            }
        }

        private void DrawYAxisLabels()
        {
            var measurements = viewModel.Measurements.ToList();
            double maxValue = Math.Max(measurements.Max(m => m.Value), 3.0);
            double chartHeight = ChartCanvas.Height - 60;

            for (double value = 0; value <= maxValue; value += 0.5)
            {
                double y = ChartCanvas.Height - 40 - (value / maxValue * chartHeight);

                var scaleLine = new Line
                {
                    X1 = 35,
                    Y1 = y,
                    X2 = 45,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                ChartCanvas.Children.Add(scaleLine);

                var scaleLabel = new TextBlock
                {
                    Text = value.ToString("F1"),
                    FontSize = 10,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(scaleLabel, 10);
                Canvas.SetTop(scaleLabel, y - 7);
                ChartCanvas.Children.Add(scaleLabel);

                // Grid line 
                if (value > 0)
                {
                    var gridLine = new Line
                    {
                        X1 = 45,
                        Y1 = y,
                        X2 = ChartCanvas.Width - 20,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 0.5,
                        StrokeDashArray = new DoubleCollection { 1, 1 }
                    };
                    ChartCanvas.Children.Add(gridLine);
                }
            }

            DrawValidRangeMarkers(maxValue, chartHeight);
        }

        private void DrawValidRangeMarkers(double maxValue, double chartHeight)
        {
            double[] validValues = { 0.34, 2.73 };

            foreach (double value in validValues)
            {
                if (value <= maxValue)
                {
                    double y = ChartCanvas.Height - 40 - (value / maxValue * chartHeight);

                    var validLine = new Line
                    {
                        X1 = 35,
                        Y1 = y,
                        X2 = 45,
                        Y2 = y,
                        Stroke = Brushes.Green,
                        StrokeThickness = 3
                    };
                    ChartCanvas.Children.Add(validLine);

                    var validLabel = new TextBlock
                    {
                        Text = value.ToString("F2"),
                        FontSize = 10,
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold
                    };

                    Canvas.SetLeft(validLabel, 10);
                    Canvas.SetTop(validLabel, y - 7);
                    ChartCanvas.Children.Add(validLabel);
                }
            }
        }
    }
}