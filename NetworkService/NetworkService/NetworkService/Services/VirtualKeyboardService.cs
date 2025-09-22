using NetworkService.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NetworkService.Services
{
    /// <summary>
    /// Service for managing Virtual Keyboard display and interaction
    /// Specifikacija: Mobilni interface sa virtuelnom tastaturom
    /// </summary>
    public class VirtualKeyboardService
    {
        #region Singleton Pattern

        private static VirtualKeyboardService instance;
        public static VirtualKeyboardService Instance
        {
            get
            {
                if (instance == null)
                    instance = new VirtualKeyboardService();
                return instance;
            }
        }

        private VirtualKeyboardService() { }

        #endregion

        #region Properties

        /// <summary>
        /// Current virtual keyboard instance
        /// </summary>
        public VirtualKeyboard CurrentKeyboard { get; private set; }

        /// <summary>
        /// Parent container for keyboard (usually main Grid or Canvas)
        /// </summary>
        public Panel KeyboardContainer { get; private set; }

        /// <summary>
        /// Currently active TextBox receiving input
        /// </summary>
        public TextBox ActiveTextBox { get; private set; }

        /// <summary>
        /// Is virtual keyboard currently visible
        /// </summary>
        public bool IsKeyboardVisible { get; private set; } = false;

        // Removed Position property - fixed position above navigation

        #endregion

        #region Events

        /// <summary>
        /// Event fired when keyboard visibility changes
        /// </summary>
        public event EventHandler<KeyboardVisibilityEventArgs> KeyboardVisibilityChanged;

        /// <summary>
        /// Event fired when text input occurs via keyboard
        /// </summary>
        public event EventHandler<VirtualKeyEventArgs> TextInput;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize keyboard service with container
        /// </summary>
        public void Initialize(Panel container)
        {
            try
            {
                KeyboardContainer = container ?? throw new ArgumentNullException(nameof(container));
                Console.WriteLine($"VirtualKeyboardService initialized successfully with {container.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing VirtualKeyboardService: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Show virtual keyboard for specified TextBox - WITH DETAILED LOGGING
        /// </summary>
        public void ShowKeyboard(TextBox targetTextBox)
        {
            try
            {
                Console.WriteLine("=== ShowKeyboard called (NEW VERSION) ===");

                if (targetTextBox == null)
                {
                    Console.WriteLine("❌ Cannot show keyboard: targetTextBox is null");
                    return;
                }
                Console.WriteLine($"✅ Target TextBox: {targetTextBox.Name ?? "Unnamed"}");

                if (KeyboardContainer == null)
                {
                    Console.WriteLine("❌ Cannot show keyboard: KeyboardContainer not initialized");
                    return;
                }
                Console.WriteLine($"✅ KeyboardContainer: {KeyboardContainer.GetType().Name} with {KeyboardContainer.Children.Count} children");

                // Set active TextBox
                ActiveTextBox = targetTextBox;
                Console.WriteLine("✅ Set ActiveTextBox");

                // Create keyboard if it doesn't exist
                if (CurrentKeyboard == null)
                {
                    Console.WriteLine("🔧 Creating new keyboard...");
                    CreateKeyboard();
                    Console.WriteLine($"✅ Keyboard created: {CurrentKeyboard != null}");
                }

                if (CurrentKeyboard != null)
                {
                    // Set keyboard target
                    CurrentKeyboard.SetTarget(ActiveTextBox);
                    Console.WriteLine("✅ Set keyboard target");

                    // Add keyboard to container if not already added
                    if (!KeyboardContainer.Children.Contains(CurrentKeyboard))
                    {
                        Console.WriteLine("🔧 Adding keyboard to container...");
                        KeyboardContainer.Children.Add(CurrentKeyboard);
                        Console.WriteLine($"✅ Keyboard added. Container now has {KeyboardContainer.Children.Count} children");
                    }
                    else
                    {
                        Console.WriteLine("ℹ️ Keyboard already in container");
                    }

                    // Position keyboard
                    Console.WriteLine("🔧 Positioning keyboard...");
                    PositionKeyboard();

                    // Make sure keyboard is visible
                    CurrentKeyboard.Visibility = Visibility.Visible;
                    Console.WriteLine($"✅ Keyboard visibility set to: {CurrentKeyboard.Visibility}");

                    // Show with animation
                    Console.WriteLine("🔧 Starting show animation...");
                    ShowKeyboardWithAnimation();

                    // FORCE LAYOUT UPDATE
                    CurrentKeyboard.UpdateLayout();
                    CurrentKeyboard.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    CurrentKeyboard.Arrange(new Rect(CurrentKeyboard.DesiredSize));

                    // EXTRA DEBUG - Check if keyboard is actually visible
                    Console.WriteLine($"🔍 AFTER LAYOUT - Opacity: {CurrentKeyboard.Opacity}");
                    Console.WriteLine($"🔍 AFTER LAYOUT - Visibility: {CurrentKeyboard.Visibility}");
                    Console.WriteLine($"🔍 AFTER LAYOUT - IsVisible: {CurrentKeyboard.IsVisible}");
                    Console.WriteLine($"🔍 AFTER LAYOUT - ActualWidth: {CurrentKeyboard.ActualWidth}");
                    Console.WriteLine($"🔍 AFTER LAYOUT - ActualHeight: {CurrentKeyboard.ActualHeight}");
                    Console.WriteLine($"🔍 AFTER LAYOUT - DesiredSize: {CurrentKeyboard.DesiredSize}");

                    IsKeyboardVisible = true;
                    OnKeyboardVisibilityChanged(new KeyboardVisibilityEventArgs { IsVisible = true, TargetTextBox = targetTextBox });

                    Console.WriteLine($"🎉 Virtual keyboard shown successfully. IsVisible: {IsKeyboardVisible}");
                }
                else
                {
                    Console.WriteLine("❌ Failed to create keyboard");
                }

                Console.WriteLine("=== ShowKeyboard completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR showing virtual keyboard: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Hide virtual keyboard
        /// </summary>
        public void HideKeyboard()
        {
            try
            {
                Console.WriteLine("=== HideKeyboard called ===");

                if (CurrentKeyboard == null || !IsKeyboardVisible)
                {
                    Console.WriteLine("ℹ️ Keyboard already hidden or null");
                    return;
                }

                // Hide with animation
                HideKeyboardWithAnimation(() =>
                {
                    // Clear target
                    CurrentKeyboard.ClearTarget();
                    ActiveTextBox = null;

                    IsKeyboardVisible = false;
                    OnKeyboardVisibilityChanged(new KeyboardVisibilityEventArgs { IsVisible = false });

                    Console.WriteLine("✅ Virtual keyboard hidden");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error hiding virtual keyboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle keyboard visibility for TextBox
        /// </summary>
        public void ToggleKeyboard(TextBox targetTextBox)
        {
            if (IsKeyboardVisible && ActiveTextBox == targetTextBox)
            {
                HideKeyboard();
            }
            else
            {
                ShowKeyboard(targetTextBox);
            }
        }

        /// <summary>
        /// Setup TextBox to automatically show keyboard on focus
        /// </summary>
        public void RegisterTextBox(TextBox textBox)
        {
            if (textBox == null) return;

            try
            {
                // Remove existing handlers to avoid duplicates
                UnregisterTextBox(textBox);

                // Add event handlers
                textBox.GotFocus += TextBox_GotFocus;
                textBox.LostFocus += TextBox_LostFocus;
                textBox.MouseUp += TextBox_MouseUp;

                Console.WriteLine($"✅ TextBox registered for virtual keyboard: {textBox.Name ?? "Unnamed"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error registering TextBox: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove TextBox from automatic keyboard handling
        /// </summary>
        public void UnregisterTextBox(TextBox textBox)
        {
            if (textBox == null) return;

            try
            {
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.MouseUp -= TextBox_MouseUp;

                Console.WriteLine($"TextBox unregistered from virtual keyboard: {textBox.Name ?? "Unnamed"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unregistering TextBox: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create new virtual keyboard instance
        /// </summary>
        private void CreateKeyboard()
        {
            try
            {
                CurrentKeyboard = new VirtualKeyboard();

                // FORCE EXPLICIT DIMENSIONS FOR TESTING
                CurrentKeyboard.Width = 580;  // Close to MainWindow width (600)
                CurrentKeyboard.Height = 280; // Standard keyboard height
                CurrentKeyboard.MinWidth = 400;
                CurrentKeyboard.MinHeight = 200;

                Console.WriteLine($"🔧 Set keyboard dimensions: {CurrentKeyboard.Width}x{CurrentKeyboard.Height}");

                // Subscribe to keyboard events
                CurrentKeyboard.KeyPressed += OnKeyboardKeyPressed;
                CurrentKeyboard.CloseRequested += OnKeyboardCloseRequested;

                Console.WriteLine("✅ Virtual keyboard instance created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating virtual keyboard: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Position keyboard at bottom of content area (Row 1)
        /// </summary>
        private void PositionKeyboard()
        {
            if (CurrentKeyboard == null || KeyboardContainer == null) return;

            try
            {
                Console.WriteLine("Positioning keyboard at bottom of content area");

                // CRITICAL: Set to Row 1 (main content area) - NOT Row 0
                Grid.SetRow(CurrentKeyboard, 1);

                // Position at absolute bottom of Row 1
                CurrentKeyboard.VerticalAlignment = VerticalAlignment.Bottom;
                CurrentKeyboard.HorizontalAlignment = HorizontalAlignment.Stretch;
                CurrentKeyboard.Margin = new Thickness(0,0,5,0);

                // Ensure high Z-index so it appears above content
                Panel.SetZIndex(CurrentKeyboard, 1000);

                Console.WriteLine("Keyboard positioned in Grid Row 1 at bottom");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error positioning keyboard: {ex.Message}");
            }
        }
        /// <summary>
        /// Show keyboard FIXED - always reset opacity
        /// </summary>
        private void ShowKeyboardWithAnimation()
        {
            if (CurrentKeyboard == null) return;

            try
            {
                Console.WriteLine("✅ FORCE Setting keyboard to FULLY VISIBLE - FIXED opacity reset");

                // ALWAYS RESET TO VISIBLE STATE
                CurrentKeyboard.Opacity = 1;
                CurrentKeyboard.RenderTransform = new TranslateTransform(0, 0);
                CurrentKeyboard.Visibility = Visibility.Visible;

                // Set high Z-index to ensure it's on top
                Panel.SetZIndex(CurrentKeyboard, 9999);

                Console.WriteLine($"✅ FORCED keyboard opacity: {CurrentKeyboard.Opacity}");
                Console.WriteLine($"✅ FORCED keyboard visibility: {CurrentKeyboard.Visibility}");
                Console.WriteLine($"✅ FORCED keyboard Z-index: {Panel.GetZIndex(CurrentKeyboard)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error showing keyboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Hide keyboard - FIXED version without broken animations
        /// </summary>
        private void HideKeyboardWithAnimation(Action onCompleted = null)
        {
            if (CurrentKeyboard == null) return;

            try
            {
                Console.WriteLine("🔧 HIDING keyboard - simple version");

                // SIMPLE HIDE WITHOUT BROKEN ANIMATIONS
                CurrentKeyboard.Visibility = Visibility.Collapsed;

                // Execute callback immediately
                onCompleted?.Invoke();

                Console.WriteLine("✅ Keyboard hidden successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error hiding keyboard: {ex.Message}");
                onCompleted?.Invoke();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle TextBox focus gained - show keyboard
        /// </summary>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Console.WriteLine($"📍 TextBox got focus: {textBox.Name ?? "Unnamed"}");
                ShowKeyboard(textBox);
            }
        }

        /// <summary>
        /// Handle TextBox focus lost - hide keyboard with DEBOUNCING
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("📍 TextBox lost focus - delaying hide...");

            // Add longer delay to prevent blinking when switching between TextBoxes
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Only hide if no other TextBox has focus and keyboard isn't focused
                if (!IsAnyTextBoxFocused() && !IsKeyboardFocused())
                {
                    Console.WriteLine("📍 No TextBox focused - hiding keyboard");
                    HideKeyboard();
                }
                else
                {
                    Console.WriteLine("📍 Another TextBox or keyboard focused - keeping visible");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Check if any registered TextBox currently has focus
        /// </summary>
        private bool IsAnyTextBoxFocused()
        {
            try
            {
                var focused = Keyboard.FocusedElement;
                return focused is TextBox;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Handle TextBox mouse click - ensure keyboard is shown
        /// </summary>
        private void TextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Console.WriteLine($"🖱️ TextBox clicked: {textBox.Name ?? "Unnamed"}");
                ShowKeyboard(textBox);
            }
        }

        /// <summary>
        /// Handle key pressed on virtual keyboard
        /// </summary>
        private void OnKeyboardKeyPressed(object sender, VirtualKeyEventArgs e)
        {
            // Forward event to subscribers
            TextInput?.Invoke(this, e);

            Console.WriteLine($"⌨️ Virtual key pressed: {e.Key} ({e.Action})");
        }

        /// <summary>
        /// Handle keyboard close request
        /// </summary>
        private void OnKeyboardCloseRequested(object sender, EventArgs e)
        {
            Console.WriteLine("❌ Keyboard close requested");
            HideKeyboard();
        }

        /// <summary>
        /// Fire keyboard visibility changed event
        /// </summary>
        private void OnKeyboardVisibilityChanged(KeyboardVisibilityEventArgs args)
        {
            KeyboardVisibilityChanged?.Invoke(this, args);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if any keyboard element currently has focus
        /// </summary>
        private bool IsKeyboardFocused()
        {
            if (CurrentKeyboard == null) return false;

            try
            {
                var focusedElement = Keyboard.FocusedElement as DependencyObject;
                if (focusedElement == null) return false;

                // Check if focused element is within keyboard
                return IsChildOf(focusedElement, CurrentKeyboard);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if child element is within parent element
        /// </summary>
        private bool IsChildOf(DependencyObject child, DependencyObject parent)
        {
            if (child == null || parent == null) return false;
            if (child == parent) return true;

            var currentParent = VisualTreeHelper.GetParent(child);
            while (currentParent != null)
            {
                if (currentParent == parent) return true;
                currentParent = VisualTreeHelper.GetParent(currentParent);
            }

            return false;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                HideKeyboard();

                if (CurrentKeyboard != null)
                {
                    CurrentKeyboard.KeyPressed -= OnKeyboardKeyPressed;
                    CurrentKeyboard.CloseRequested -= OnKeyboardCloseRequested;
                }

                KeyboardContainer?.Children.Remove(CurrentKeyboard);
                CurrentKeyboard = null;
                KeyboardContainer = null;
                ActiveTextBox = null;

                Console.WriteLine("VirtualKeyboardService disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing VirtualKeyboardService: {ex.Message}");
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// Keyboard position options
    /// </summary>
    public enum KeyboardPosition
    {
        Bottom,             // Bottom of screen 
        AboveNavigation,    // Above navigation bar (mobile style)
        Center,             // Center of screen
        Top                 // Top of screen (below header)
    }

    /// <summary>
    /// Event arguments for keyboard visibility changes
    /// </summary>
    public class KeyboardVisibilityEventArgs : EventArgs
    {
        public bool IsVisible { get; set; }
        public TextBox TargetTextBox { get; set; }
    }

    #endregion
}