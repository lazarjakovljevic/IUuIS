using NetworkService.Controls;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkService.Views
{
    /// <summary>
    /// NetworkEntitiesView with VirtualKeyboard integration
    /// Specifikacija: Mobilni interface sa virtuelnom tastaturom
    /// </summary>
    public partial class NetworkEntitiesView : UserControl
    {
        #region Fields

        private VirtualKeyboardService keyboardService;
        private bool isInitialized = false;

        #endregion

        #region Constructor

        public NetworkEntitiesView()
        {
            InitializeComponent();
            InitializeVirtualKeyboard();

            VirtualKeyboardService.Instance.KeyboardVisibilityChanged += OnKeyboardVisibilityChanged;
        }

        #endregion

        #region Virtual Keyboard Integration

        /// <summary>
        /// Initialize virtual keyboard service and register TextBox controls
        /// </summary>
        private void InitializeVirtualKeyboard()
        {
            try
            {
                // Get keyboard service instance
                keyboardService = VirtualKeyboardService.Instance;

                // Wait for UI to load before registering TextBoxes
                this.Loaded += OnViewLoaded;

                Console.WriteLine("NetworkEntitiesView: Virtual keyboard initialization started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing virtual keyboard in NetworkEntitiesView: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle view loaded event to register TextBoxes
        /// </summary>
        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return;

            try
            {
                // Initialize keyboard service with main container
                var mainContainer = FindMainContainer();
                if (mainContainer != null)
                {
                    keyboardService.Initialize(mainContainer);
                }

                // Find and register TextBox controls
                RegisterTextBoxControls();

                // Subscribe to keyboard events
                keyboardService.TextInput += OnVirtualKeyboardInput;
                keyboardService.KeyboardVisibilityChanged += OnKeyboardVisibilityChanged;

                isInitialized = true;
                Console.WriteLine("NetworkEntitiesView: Virtual keyboard integration completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnViewLoaded: {ex.Message}");
            }
        }

        /// <summary>
        /// Find main container for keyboard display (usually MainWindow Grid)
        /// </summary>
        private Panel FindMainContainer()
        {
            try
            {
                // Traverse up the visual tree to find MainWindow
                var current = this as DependencyObject;
                while (current != null)
                {
                    current = System.Windows.Media.VisualTreeHelper.GetParent(current);

                    if (current is Window window)
                    {
                        // Find main Grid in MainWindow
                        if (window.Content is Grid mainGrid)
                        {
                            Console.WriteLine("Found MainWindow Grid for keyboard container");
                            return mainGrid;
                        }
                        else if (window.Content is Panel panel)
                        {
                            Console.WriteLine("Found MainWindow Panel for keyboard container");
                            return panel;
                        }
                    }
                }

                Console.WriteLine("Warning: Could not find main container for virtual keyboard");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding main container: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find and register all TextBox controls for virtual keyboard
        /// </summary>
        private void RegisterTextBoxControls()
        {
            try
            {
                // Find TextBox controls by name or by traversing visual tree
                var textBoxes = FindTextBoxes(this);

                foreach (var textBox in textBoxes)
                {
                    RegisterTextBox(textBox);
                }

                Console.WriteLine($"Registered {textBoxes.Count} TextBox controls for virtual keyboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering TextBox controls: {ex.Message}");
            }
        }

        /// <summary>
        /// Register individual TextBox with keyboard service
        /// </summary>
        private void RegisterTextBox(TextBox textBox)
        {
            try
            {
                // Set name for easier identification
                if (string.IsNullOrEmpty(textBox.Name))
                {
                    textBox.Name = $"TextBox_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                // Register with keyboard service
                keyboardService.RegisterTextBox(textBox);

                // Add visual indicators for mobile interface
                AddMobileIndicators(textBox);

                Console.WriteLine($"TextBox registered: {textBox.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering TextBox: {ex.Message}");
            }
        }

        /// <summary>
        /// Add mobile-style visual indicators to TextBox
        /// </summary>
        private void AddMobileIndicators(TextBox textBox)
        {
            try
            {
                // Add tooltip indicating virtual keyboard usage
                textBox.ToolTip = "Tap to open virtual keyboard";

                // Add cursor change to indicate mobile interaction
                textBox.Cursor = System.Windows.Input.Cursors.Hand;

                // You could also add an icon overlay here if needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding mobile indicators: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively find all TextBox controls in visual tree
        /// </summary>
        private System.Collections.Generic.List<TextBox> FindTextBoxes(DependencyObject parent)
        {
            var textBoxes = new System.Collections.Generic.List<TextBox>();

            try
            {
                int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < childCount; i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                    if (child is TextBox textBox)
                    {
                        // Skip read-only TextBoxes (they don't need virtual keyboard)
                        if (!textBox.IsReadOnly)
                        {
                            textBoxes.Add(textBox);
                        }
                    }
                    else
                    {
                        // Recursively search children
                        textBoxes.AddRange(FindTextBoxes(child));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding TextBoxes: {ex.Message}");
            }

            return textBoxes;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle keyboard visibility changes - add bottom padding for ALL TextBoxes
        /// </summary>
        private void OnKeyboardVisibilityChanged(object sender, KeyboardVisibilityEventArgs e)
        {
            try
            {
                var scrollViewer = FindScrollViewer(this);

                if (scrollViewer != null)
                {
                    if (e.IsVisible)
                    {
                        // Always add padding when keyboard visible
                        scrollViewer.Padding = new Thickness(0, 0, 0, 275);

                        // Smart positioning based on TextBox type
                        if (IsFilterTextBox(e.TargetTextBox))
                        {
                            // Filter TextBox - scroll to middle
                            var middlePosition = scrollViewer.ScrollableHeight * 0.75;
                            scrollViewer.ScrollToVerticalOffset(middlePosition);
                            Console.WriteLine("Scrolled to middle for filter TextBox");
                        }
                        else
                        {
                            // Entity form TextBoxes - scroll to bottom
                            scrollViewer.ScrollToBottom();
                            Console.WriteLine("Scrolled to bottom for entity form TextBox");
                        }
                    }
                    else
                    {
                        // Remove padding and scroll to top when keyboard hidden
                        scrollViewer.Padding = new Thickness(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling keyboard visibility change: {ex.Message}");
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

        /// <summary>
        /// Check if TextBox is filter TextBox (top of page)
        /// </summary>
        private bool IsFilterTextBox(TextBox textBox)
        {
            if (textBox == null) return false;

            try
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                if (binding?.ParentBinding?.Path?.Path != null)
                {
                    var path = binding.ParentBinding.Path.Path;
                    return path.Contains("Filter"); // Filter TextBox uses FilterIdValue binding
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Handle virtual keyboard input
        /// </summary>
        private void OnVirtualKeyboardInput(object sender, VirtualKeyEventArgs e)
        {
            try
            {
                Console.WriteLine($"Virtual keyboard input: {e.Key} ({e.Action})");

                // You can add custom handling for specific keys here
                switch (e.Action)
                {
                    case VirtualKeyAction.Enter:
                        HandleEnterKey();
                        break;
                    case VirtualKeyAction.Character:
                        HandleCharacterInput(e.Key);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling virtual keyboard input: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle Enter key from virtual keyboard
        /// </summary>
        private void HandleEnterKey()
        {
            try
            {
                var activeTextBox = keyboardService.ActiveTextBox;
                if (activeTextBox == null) return;

                // Move focus to next control or trigger action
                var nextControl = activeTextBox.PredictFocus(System.Windows.Input.FocusNavigationDirection.Next);
                if (nextControl is Control control)
                {
                    control.Focus();
                }
                else
                {
                    // If no next control, try to trigger Add button if we're in add form
                    if (IsInAddEntityForm(activeTextBox))
                    {
                        TriggerAddEntity();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling Enter key: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle character input from virtual keyboard
        /// </summary>
        private void HandleCharacterInput(string character)
        {
            try
            {
                // Custom validation or formatting can be added here
                // For now, the character is already handled by the TextBox
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling character input: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if TextBox is in Add Entity form
        /// </summary>
        private bool IsInAddEntityForm(TextBox textBox)
        {
            try
            {
                // Check if TextBox is bound to add entity properties
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                if (binding?.ParentBinding?.Path?.Path != null)
                {
                    var path = binding.ParentBinding.Path.Path;
                    return path.Contains("NewEntity");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking add entity form: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Trigger Add Entity action from keyboard
        /// </summary>
        private void TriggerAddEntity()
        {
            try
            {
                // Get ViewModel and trigger add command
                if (DataContext is NetworkEntitiesViewModel viewModel)
                {
                    var command = viewModel.AddEntityCommand as System.Windows.Input.ICommand;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error triggering add entity: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup virtual keyboard integration
        /// </summary>
        public void Cleanup()
        {
            try
            {
                if (keyboardService != null)
                {
                    keyboardService.TextInput -= OnVirtualKeyboardInput;
                    VirtualKeyboardService.Instance.KeyboardVisibilityChanged -= OnKeyboardVisibilityChanged;

                    // Unregister all TextBoxes
                    var textBoxes = FindTextBoxes(this);
                    foreach (var textBox in textBoxes)
                    {
                        keyboardService.UnregisterTextBox(textBox);
                    }
                }

                this.Loaded -= OnViewLoaded;

                Console.WriteLine("NetworkEntitiesView: Virtual keyboard cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        #endregion
    }
}