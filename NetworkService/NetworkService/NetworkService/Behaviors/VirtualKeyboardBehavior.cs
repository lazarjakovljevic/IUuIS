using Microsoft.Xaml.Behaviors;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkService.Behaviors
{
    public class VirtualKeyboardBehavior : Behavior<FrameworkElement>
    {
        private bool isInitialized = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return;

            try
            {
                var mainContainer = FindMainContainer(AssociatedObject);
                if (mainContainer != null)
                {
                    VirtualKeyboardService.Instance.Initialize(mainContainer);
                }

                RegisterAllTextBoxes(AssociatedObject);
                SetupScrollViewerActions();
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VirtualKeyboardBehavior: {ex.Message}");
            }
        }

        private void SetupScrollViewerActions()
        {
            if (AssociatedObject.DataContext is NetworkEntitiesViewModel viewModel)
            {
                var scrollViewer = FindScrollViewer(AssociatedObject);
                if (scrollViewer != null)
                {
                    viewModel.ScrollToTopAction = () => scrollViewer.ScrollToTop();
                    viewModel.ScrollToBottomAction = () => scrollViewer.ScrollToBottom();
                    viewModel.ScrollToVerticalOffsetAction = (offset) =>
                    {
                        var position = scrollViewer.ScrollableHeight * offset;
                        scrollViewer.ScrollToVerticalOffset(position);
                    };
                    viewModel.SetScrollViewerPaddingAction = (padding) =>
                    {
                        scrollViewer.Padding = padding;
                    };
                }
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent is ScrollViewer scrollViewer)
                return scrollViewer;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private Panel FindMainContainer(DependencyObject element)
        {
            var current = element;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is Window window)
                {
                    if (window.Content is Grid mainGrid)
                        return mainGrid;
                    else if (window.Content is Panel panel)
                        return panel;
                }
            }
            return null;
        }

        private void RegisterAllTextBoxes(DependencyObject parent)
        {
            var textBoxes = FindAllTextBoxes(parent);
            foreach (var textBox in textBoxes)
            {
                if (string.IsNullOrEmpty(textBox.Name))
                    textBox.Name = $"TextBox_{Guid.NewGuid().ToString().Substring(0, 8)}";

                VirtualKeyboardService.Instance.RegisterTextBox(textBox);
                textBox.ToolTip = "Tap to open virtual keyboard";
                textBox.Cursor = Cursors.Hand;
            }
        }

        private List<TextBox> FindAllTextBoxes(DependencyObject parent)
        {
            var textBoxes = new List<TextBox>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBox textBox && !textBox.IsReadOnly)
                    textBoxes.Add(textBox);
                else
                    textBoxes.AddRange(FindAllTextBoxes(child));
            }
            return textBoxes;
        }
    }
}