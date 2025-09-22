using NetworkService.Controls;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VirtualKeyboardService keyboardService;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            InitializeVirtualKeyboard();
        }

        // MainWindow.xaml.cs additions for VirtualKeyboard support

        #region Virtual Keyboard Integration

        /// <summary>
        /// Initialize Virtual Keyboard Service in MainWindow constructor
        /// Add this after InitializeComponent() call
        /// </summary>
        private void InitializeVirtualKeyboard()
        {
            try
            {
                // Get keyboard service instance
                keyboardService = VirtualKeyboardService.Instance;

                // Find main content grid (assumes MainWindow has a main Grid)
                if (Content is Grid mainGrid)
                {
                    keyboardService.Initialize(mainGrid);
                    Console.WriteLine("MainWindow: Virtual keyboard service initialized with main grid");
                }
                else if (Content is Panel panel)
                {
                    keyboardService.Initialize(panel);
                    Console.WriteLine("MainWindow: Virtual keyboard service initialized with main panel");
                }
                else
                {
                    Console.WriteLine("Warning: Could not initialize virtual keyboard - no suitable container found");
                    return;
                }

                // Subscribe to global keyboard events
                keyboardService.TextInput += OnGlobalKeyboardInput;
                keyboardService.KeyboardVisibilityChanged += OnGlobalKeyboardVisibilityChanged;

                Console.WriteLine("MainWindow: Virtual keyboard integration completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing virtual keyboard in MainWindow: {ex.Message}");
                MessageBox.Show($"Virtual keyboard initialization failed: {ex.Message}",
                               "Keyboard Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handle global keyboard input events
        /// </summary>
        private void OnGlobalKeyboardInput(object sender, VirtualKeyEventArgs e)
        {
            try
            {
                Console.WriteLine($"MainWindow: Global keyboard input - {e.Key} ({e.Action})");

                // Add global input handling if needed
                switch (e.Action)
                {
                    case VirtualKeyAction.Enter:
                        // Could trigger global actions like navigation
                        break;
                    case VirtualKeyAction.Backspace:
                        // Global backspace handling
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling global keyboard input: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle global keyboard visibility changes
        /// </summary>
        private void OnGlobalKeyboardVisibilityChanged(object sender, KeyboardVisibilityEventArgs e)
        {
            try
            {
                if (e.IsVisible)
                {
                    Console.WriteLine("MainWindow: Virtual keyboard is now visible");
                    // REMOVE THIS LINE: AdjustLayoutForKeyboard(true);
                }
                else
                {
                    Console.WriteLine("MainWindow: Virtual keyboard is now hidden");
                    // REMOVE THIS LINE: AdjustLayoutForKeyboard(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling keyboard visibility change: {ex.Message}");
            }
        }

        /// <summary>
        /// Adjust main window layout when virtual keyboard is shown/hidden
        /// </summary>
        private void AdjustLayoutForKeyboard(bool keyboardVisible)
        {
            // DO NOTHING - keep navigation fixed
            // Content scrolling will be handled by individual views
        }
        /// <summary>
        /// Override Window closing to cleanup keyboard service
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Cleanup keyboard service
                if (keyboardService != null)
                {
                    keyboardService.TextInput -= OnGlobalKeyboardInput;
                    keyboardService.KeyboardVisibilityChanged -= OnGlobalKeyboardVisibilityChanged;
                    keyboardService.Dispose();
                }

                // Call existing cleanup if it exists
                if (DataContext is MainWindowViewModel mainViewModel)
                {
                    mainViewModel.Cleanup();
                }

                Console.WriteLine("MainWindow: Cleanup completed including virtual keyboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during MainWindow cleanup: {ex.Message}");
            }

            base.OnClosing(e);
        }

        #endregion

        #region Quick Access Methods (Optional)

        /// <summary>
        /// Show virtual keyboard programmatically
        /// </summary>
        public void ShowVirtualKeyboard(TextBox targetTextBox)
        {
            keyboardService?.ShowKeyboard(targetTextBox);
        }

        /// <summary>
        /// Hide virtual keyboard programmatically  
        /// </summary>
        public void HideVirtualKeyboard()
        {
            keyboardService?.HideKeyboard();
        }

        /// <summary>
        /// Toggle virtual keyboard for specific TextBox
        /// </summary>
        public void ToggleVirtualKeyboard(TextBox targetTextBox)
        {
            keyboardService?.ToggleKeyboard(targetTextBox);
        }

        /// <summary>
        /// Check if virtual keyboard is currently visible
        /// </summary>
        public bool IsVirtualKeyboardVisible => keyboardService?.IsKeyboardVisible ?? false;

        #endregion
    }
}
