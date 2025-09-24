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

        #region Virtual Keyboard Integration

        private void InitializeVirtualKeyboard()
        {
            try
            {
                keyboardService = VirtualKeyboardService.Instance;

                if (Content is Grid mainGrid)
                {
                    keyboardService.Initialize(mainGrid);
                }
                else if (Content is Panel panel)
                {
                    keyboardService.Initialize(panel);
                }
                else
                {
                    Console.WriteLine("Warning: Could not initialize virtual keyboard - no suitable container found");
                    return;
                }

                keyboardService.TextInput += OnGlobalKeyboardInput;
                keyboardService.KeyboardVisibilityChanged += OnGlobalKeyboardVisibilityChanged;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing virtual keyboard in MainWindow: {ex.Message}");
                MessageBox.Show($"Virtual keyboard initialization failed: {ex.Message}",
                               "Keyboard Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnGlobalKeyboardInput(object sender, VirtualKeyEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case VirtualKeyAction.Enter:
                        break;
                    case VirtualKeyAction.Backspace:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling global keyboard input: {ex.Message}");
            }
        }

        private void OnGlobalKeyboardVisibilityChanged(object sender, KeyboardVisibilityEventArgs e)
        {
            try
            {
                if (e.IsVisible)
                {
                    //Console.WriteLine("MainWindow: Virtual keyboard is now visible");
                }
                else
                {
                    //Console.WriteLine("MainWindow: Virtual keyboard is now hidden");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling keyboard visibility change: {ex.Message}");
            }
        }

        #endregion
       
    }
}
