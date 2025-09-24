using NetworkService.Controls;
using NetworkService.Services;
using NetworkService.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkService.Views
{
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

            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is NetworkEntitiesViewModel viewModel)
                {
                    viewModel.ScrollToTopAction = OnScrollToTopRequested;
                }
            };
        }

        #endregion

        #region Virtual Keyboard Integration

        private void InitializeVirtualKeyboard()
        {
            try
            {
                keyboardService = VirtualKeyboardService.Instance;

                this.Loaded += OnViewLoaded;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing virtual keyboard in NetworkEntitiesView: {ex.Message}");
            }
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            if (isInitialized) return;

            try
            {

                var mainContainer = FindMainContainer();
                if (mainContainer != null)
                {
                    keyboardService.Initialize(mainContainer);
                }

                RegisterTextBoxControls();

                keyboardService.TextInput += OnVirtualKeyboardInput;
                keyboardService.KeyboardVisibilityChanged += OnKeyboardVisibilityChanged;

                isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnViewLoaded: {ex.Message}");
            }
        }

        private Panel FindMainContainer()
        {
            try
            {
                var current = this as DependencyObject;
                while (current != null)
                {
                    current = VisualTreeHelper.GetParent(current);

                    if (current is Window window)
                    {
                        if (window.Content is Grid mainGrid)
                        {
                            return mainGrid;
                        }
                        else if (window.Content is Panel panel)
                        {
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

        private void RegisterTextBoxControls()
        {
            try
            {
                var textBoxes = FindTextBoxes(this);

                foreach (var textBox in textBoxes)
                {
                    RegisterTextBox(textBox);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering TextBox controls: {ex.Message}");
            }
        }

        private void RegisterTextBox(TextBox textBox)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox.Name))
                {
                    textBox.Name = $"TextBox_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                keyboardService.RegisterTextBox(textBox);

                AddMobileIndicators(textBox);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering TextBox: {ex.Message}");
            }
        }

        private void AddMobileIndicators(TextBox textBox)
        {
            try
            {
                textBox.ToolTip = "Tap to open virtual keyboard";

                textBox.Cursor = Cursors.Hand;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding mobile indicators: {ex.Message}");
            }
        }

        private System.Collections.Generic.List<TextBox> FindTextBoxes(DependencyObject parent)
        {
            var textBoxes = new System.Collections.Generic.List<TextBox>();

            try
            {
                int childCount = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child is TextBox textBox)
                    {
                        if (!textBox.IsReadOnly)
                        {
                            textBoxes.Add(textBox);
                        }
                    }
                    else
                    {
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

        private void FilterIdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (!IsTextAllowed(e.Text))
                {
                    e.Handled = true;

                    if (DataContext is NetworkEntitiesViewModel viewModel)
                    {
                        viewModel.TriggerFilterIdError();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FilterId validation: {ex.Message}");
            }
        }

        private bool IsTextAllowed(string text)
        {
            return text.All(char.IsDigit);
        }

        private void OnKeyboardVisibilityChanged(object sender, KeyboardVisibilityEventArgs e)
        {
            try
            {
                var scrollViewer = FindScrollViewer(this);

                if (scrollViewer != null)
                {
                    if (e.IsVisible)
                    {
                        if (IsFilterTextBox(e.TargetTextBox))
                        {
                            // Filter TextBox - scroll to middle
                            var middlePosition = scrollViewer.ScrollableHeight * 0.75;
                            scrollViewer.ScrollToVerticalOffset(middlePosition);
                            Console.WriteLine("Scrolled to middle for filter TextBox");
                        }
                        else
                        {
                            scrollViewer.Padding = new Thickness(0, 0, 0, 275); // taman 280 px je tastatura, znaci malo manji padding 
                            // Entity form TextBoxes - scroll to bottom
                            scrollViewer.ScrollToBottom();
                            Console.WriteLine("Scrolled to bottom for entity form TextBox");
                        }
                    }
                    else
                    {
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

        private bool IsFilterTextBox(TextBox textBox)
        {
            if (textBox == null) return false;

            try
            {
                var binding = textBox.GetBindingExpression(TextBox.TextProperty);
                if (binding?.ParentBinding?.Path?.Path != null)
                {
                    var path = binding.ParentBinding.Path.Path;
                    return path.Contains("Filter");
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void OnVirtualKeyboardInput(object sender, VirtualKeyEventArgs e)
        {
            try
            {
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

        private void HandleEnterKey()
        {
            try
            {
                var activeTextBox = keyboardService.ActiveTextBox;
                if (activeTextBox == null) return;

                var nextControl = activeTextBox.PredictFocus(FocusNavigationDirection.Next);
                if (nextControl is Control control)
                {
                    control.Focus();
                }
                else
                {
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

        private void HandleCharacterInput(string character)
        {
            try
            {
                // karakter je handlovan od strane textboxa
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling character input: {ex.Message}");
            }
        }

        private bool IsInAddEntityForm(TextBox textBox)
        {
            try
            {
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

        private void TriggerAddEntity()
        {
            try
            {
                if (DataContext is NetworkEntitiesViewModel viewModel)
                {
                    var command = viewModel.AddEntityCommand as ICommand;
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

        private void OnScrollToTopRequested()
        {
            try
            {
                EntitiesScrollViewer.ScrollToTop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling to top: {ex.Message}");
            }
        }

        #endregion



        #region Cleanup

        public void Cleanup()
        {
            try
            {
                if (keyboardService != null)
                {
                    keyboardService.TextInput -= OnVirtualKeyboardInput;
                    VirtualKeyboardService.Instance.KeyboardVisibilityChanged -= OnKeyboardVisibilityChanged;

                    var textBoxes = FindTextBoxes(this);
                    foreach (var textBox in textBoxes)
                    {
                        keyboardService.UnregisterTextBox(textBox);
                    }
                }

                this.Loaded -= OnViewLoaded;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        #endregion
    }
}