using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.Services
{
    public class ChartRenderingService
    {
        #region Singleton Pattern
        private static ChartRenderingService instance;
        public static ChartRenderingService Instance
        {
            get
            {
                if (instance == null)
                    instance = new ChartRenderingService();
                return instance;
            }
        }
        #endregion

        #region Constants
        private const double ChartMarginLeft = 40;
        private const double ChartMarginRight = 20;
        private const double ChartMarginTop = 20;
        private const double ChartMarginBottom = 40;
        private const double BarWidthPercent = 0.8;
        private const double BarSpacingPercent = 0.2;
        private const double MinMaxValue = 3.0;
        private const double YAxisStep = 0.5;
        private readonly double[] ValidRangeMarkers = { 0.34, 2.73 };
        #endregion

        #region Public Methods
        public void RenderChart(Canvas chartCanvas, IEnumerable<Measurement> measurements)
        {
            if (chartCanvas == null)
                return;

            chartCanvas.Children.Clear();

            if (measurements == null || !measurements.Any())
            {
                RenderNoDataMessage(chartCanvas);
                return;
            }

            var measurementList = measurements.ToList();
            RenderAxes(chartCanvas);
            RenderBars(chartCanvas, measurementList);
            RenderLabels(chartCanvas, measurementList);
        }
        #endregion

        #region Private Rendering Methods
        private void RenderNoDataMessage(Canvas canvas)
        {
            var message = new TextBlock
            {
                Text = "No measurement data available",
                FontSize = 16,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(message, canvas.Width / 2 - 100);
            Canvas.SetTop(message, canvas.Height / 2);
            canvas.Children.Add(message);
        }

        private void RenderAxes(Canvas canvas)
        {
            // Y-axis (vertical)
            var yAxis = new Line
            {
                X1 = ChartMarginLeft,
                Y1 = ChartMarginTop,
                X2 = ChartMarginLeft,
                Y2 = canvas.Height - ChartMarginBottom,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvas.Children.Add(yAxis);

            // X-axis (horizontal)
            var xAxis = new Line
            {
                X1 = ChartMarginLeft,
                Y1 = canvas.Height - ChartMarginBottom,
                X2 = canvas.Width - ChartMarginRight,
                Y2 = canvas.Height - ChartMarginBottom,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvas.Children.Add(xAxis);

            // Y-axis label
            var yLabel = new TextBlock
            {
                Text = "Value (kWh)",
                FontSize = 12,
                Foreground = Brushes.Black,
                RenderTransform = new RotateTransform(-90)
            };
            Canvas.SetLeft(yLabel, -15);
            Canvas.SetTop(yLabel, canvas.Height / 2);
            canvas.Children.Add(yLabel);

            // X-axis label
            var xLabel = new TextBlock
            {
                Text = "Time",
                FontSize = 12,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(xLabel, canvas.Width / 2 - 20);
            Canvas.SetTop(xLabel, canvas.Height - 15);
            canvas.Children.Add(xLabel);
        }

        private void RenderBars(Canvas canvas, List<Measurement> measurements)
        {
            if (!measurements.Any()) return;

            // Calculate chart dimensions
            double chartWidth = canvas.Width - 60; // Leave space for axes
            double chartHeight = canvas.Height - 60;
            double barWidth = chartWidth / measurements.Count * BarWidthPercent;
            double spacing = chartWidth / measurements.Count * BarSpacingPercent;

            double maxValue = Math.Max(measurements.Max(m => m.Value), MinMaxValue);

            for (int i = 0; i < measurements.Count; i++)
            {
                var measurement = measurements[i];

                double barHeight = (measurement.Value / maxValue) * chartHeight;

                // Calculate bar position
                double x = 50 + (i * (barWidth + spacing));
                double y = canvas.Height - ChartMarginBottom - barHeight;

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
                canvas.Children.Add(bar);

                // Value label on top of bar
                var valueLabel = new TextBlock
                {
                    Text = measurement.Value.ToString("F2"),
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Canvas.SetLeft(valueLabel, x + barWidth / 2 - 15);
                Canvas.SetTop(valueLabel, y - 20);
                canvas.Children.Add(valueLabel);
            }
        }

        private void RenderLabels(Canvas canvas, List<Measurement> measurements)
        {
            if (!measurements.Any()) return;

            double chartWidth = canvas.Width - 60;
            double barWidth = chartWidth / measurements.Count * BarWidthPercent;
            double spacing = chartWidth / measurements.Count * BarSpacingPercent;

            // Render Y-axis labels first
            RenderYAxisLabels(canvas, measurements);

            // Render time labels below bars
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
                Canvas.SetTop(timeLabel, canvas.Height - 30);
                canvas.Children.Add(timeLabel);
            }
        }

        private void RenderYAxisLabels(Canvas canvas, List<Measurement> measurements)
        {
            double maxValue = Math.Max(measurements.Max(m => m.Value), MinMaxValue);
            double chartHeight = canvas.Height - 60;

            for (double value = 0; value <= maxValue; value += YAxisStep)
            {
                double y = canvas.Height - ChartMarginBottom - (value / maxValue * chartHeight);

                // Scale line
                var scaleLine = new Line
                {
                    X1 = 35,
                    Y1 = y,
                    X2 = 45,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(scaleLine);

                // Scale label
                var scaleLabel = new TextBlock
                {
                    Text = value.ToString("F1"),
                    FontSize = 10,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(scaleLabel, 10);
                Canvas.SetTop(scaleLabel, y - 7);
                canvas.Children.Add(scaleLabel);

                // Grid line
                if (value > 0)
                {
                    var gridLine = new Line
                    {
                        X1 = 45,
                        Y1 = y,
                        X2 = canvas.Width - ChartMarginRight,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 0.5,
                        StrokeDashArray = new DoubleCollection { 1, 1 }
                    };
                    canvas.Children.Add(gridLine);
                }
            }

            // Render valid range markers
            RenderValidRangeMarkers(canvas, maxValue, chartHeight);
        }

        private void RenderValidRangeMarkers(Canvas canvas, double maxValue, double chartHeight)
        {
            foreach (double value in ValidRangeMarkers)
            {
                if (value <= maxValue)
                {
                    double y = canvas.Height - ChartMarginBottom - (value / maxValue * chartHeight);

                    var validLine = new Line
                    {
                        X1 = 35,
                        Y1 = y,
                        X2 = 45,
                        Y2 = y,
                        Stroke = Brushes.Green,
                        StrokeThickness = 3
                    };
                    canvas.Children.Add(validLine);

                    var validLabel = new TextBlock
                    {
                        Text = value.ToString("F2"),
                        FontSize = 10,
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold
                    };

                    Canvas.SetLeft(validLabel, 10);
                    Canvas.SetTop(validLabel, y - 7);
                    canvas.Children.Add(validLabel);
                }
            }
        }
        #endregion
    }
}