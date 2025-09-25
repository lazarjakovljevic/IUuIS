using NetworkService.Controls;
using NetworkService.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NetworkService.Services
{
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

        public VirtualKeyboard CurrentKeyboard { get; private set; }

        public Panel KeyboardContainer { get; private set; }

        public TextBox ActiveTextBox { get; private set; }

        public bool IsKeyboardVisible { get; private set; } = false;

        #endregion

        #region Events

        public event EventHandler<KeyboardVisibilityEventArgs> KeyboardVisibilityChanged;

        public event EventHandler<VirtualKeyEventArgs> TextInput;

        #endregion

        #region Public Methods

        public void Initialize(Panel container)
        {
            try
            {
                KeyboardContainer = container ?? throw new ArgumentNullException(nameof(container));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing VirtualKeyboardService: {ex.Message}");
                throw;
            }
        }

        public void ShowKeyboard(TextBox targetTextBox)
        {
            try
            {
                if (targetTextBox == null)
                {
                    return;
                }

                if (KeyboardContainer == null)
                {
                    return;
                }

                ActiveTextBox = targetTextBox;

                if (CurrentKeyboard == null)
                {
                    CreateKeyboard();
                }

                if (CurrentKeyboard != null)
                {
                    CurrentKeyboard.SetTarget(ActiveTextBox);

                    if (!KeyboardContainer.Children.Contains(CurrentKeyboard))
                    {
                        KeyboardContainer.Children.Add(CurrentKeyboard);
                    }                
                 
                    PositionKeyboard();

                    CurrentKeyboard.Visibility = Visibility.Visible;
 
                    ShowKeyboardWithAnimation();

                    CurrentKeyboard.UpdateLayout();
                    CurrentKeyboard.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    CurrentKeyboard.Arrange(new Rect(CurrentKeyboard.DesiredSize));

                    IsKeyboardVisible = true;
                    OnKeyboardVisibilityChanged(new KeyboardVisibilityEventArgs { IsVisible = true, TargetTextBox = targetTextBox });

                }
                else
                {
                    Console.WriteLine("Error creating keyboard!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void HideKeyboard()
        {
            try
            {
                if (CurrentKeyboard == null || !IsKeyboardVisible)
                {
                    return;
                }

                HideKeyboardWithAnimation(() =>
                {
                    CurrentKeyboard.ClearTarget();
                    ActiveTextBox = null;

                    IsKeyboardVisible = false;
                    OnKeyboardVisibilityChanged(new KeyboardVisibilityEventArgs { IsVisible = false });

                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding virtual keyboard: {ex.Message}");
            }
        }

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

        public void RegisterTextBox(TextBox textBox)
        {
            if (textBox == null) return;

            try
            {
                UnregisterTextBox(textBox);

                textBox.GotFocus += TextBox_GotFocus;
                textBox.LostFocus += TextBox_LostFocus;
                textBox.MouseUp += TextBox_MouseUp;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering TextBox: {ex.Message}");
            }
        }

        public void UnregisterTextBox(TextBox textBox)
        {
            if (textBox == null) return;

            try
            {
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.MouseUp -= TextBox_MouseUp;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unregistering TextBox: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        private void CreateKeyboard()
        {
            try
            {
                CurrentKeyboard = new VirtualKeyboard();

                CurrentKeyboard.Width = 600;  // MainWidonw height
                CurrentKeyboard.Height = 280; // Keyboard height
                CurrentKeyboard.MinWidth = 400;
                CurrentKeyboard.MinHeight = 200;

                CurrentKeyboard.KeyPressed += OnKeyboardKeyPressed;
                CurrentKeyboard.CloseRequested += OnKeyboardCloseRequested;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating virtual keyboard: {ex.Message}");
                throw;
            }
        }

        private void PositionKeyboard()
        {
            if (CurrentKeyboard == null || KeyboardContainer == null) return;

            try
            {
                // (main content area je row 1)
                Grid.SetRow(CurrentKeyboard, 1);

                CurrentKeyboard.VerticalAlignment = VerticalAlignment.Bottom;
                CurrentKeyboard.HorizontalAlignment = HorizontalAlignment.Stretch;
                CurrentKeyboard.Margin = new Thickness(0, 0, 0, 0);

                CurrentKeyboard.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                CurrentKeyboard.Arrange(new Rect(CurrentKeyboard.DesiredSize));

                Panel.SetZIndex(CurrentKeyboard, 1000);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error positioning keyboard: {ex.Message}");
            }
        }

        private void ShowKeyboardWithAnimation()
        {
            if (CurrentKeyboard == null) return;

            try
            {

                CurrentKeyboard.Visibility = Visibility.Visible;
                CurrentKeyboard.Opacity = 0;

                var slideTransform = new TranslateTransform(0, CurrentKeyboard.Height + 50);
                CurrentKeyboard.RenderTransform = slideTransform;

                Panel.SetZIndex(CurrentKeyboard, 9999);

                var duration = TimeSpan.FromMilliseconds(250);

                // 1. SLIDE UP Animation (Y position)
                var slideAnimation = new DoubleAnimation
                {
                    From = CurrentKeyboard.Height + 50,  // Start below screen
                    To = 0,                              // End at normal position
                    Duration = new Duration(duration),
                    EasingFunction = new CubicEase       // Smooth cubic ease out
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };

                // 2. FADE IN Animation (Opacity)
                var fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(duration),
                    EasingFunction = new QuadraticEase     // Gentle fade
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };

                slideTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
                CurrentKeyboard.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);

            }
            catch (Exception)
            {
                CurrentKeyboard.Opacity = 1;
                CurrentKeyboard.RenderTransform = new TranslateTransform(0, 0);
                CurrentKeyboard.Visibility = Visibility.Visible;
            }
        }

        private void HideKeyboardWithAnimation(Action onCompleted = null)
        {
            if (CurrentKeyboard == null)
            {
                onCompleted?.Invoke();
                return;
            }

            try
            {

                var duration = TimeSpan.FromMilliseconds(150); // Slightly faster hide

                var slideTransform = CurrentKeyboard.RenderTransform as TranslateTransform
                                   ?? new TranslateTransform(0, 0);
                CurrentKeyboard.RenderTransform = slideTransform;

                // 1. SLIDE DOWN Animation (Y position)
                var slideAnimation = new DoubleAnimation
                {
                    From = 0,                                // Start at current position
                    To = CurrentKeyboard.ActualHeight + 30,  // Slide below screen
                    Duration = new Duration(duration), 
                    EasingFunction = new CubicEase           // Smooth cubic ease in
                    {
                        EasingMode = EasingMode.EaseIn
                    }
                };

                // 2. FADE OUT Animation (Opacity) 
                var fadeAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(duration),
                    EasingFunction = new QuadraticEase       // Quick fade out
                    {
                        EasingMode = EasingMode.EaseIn
                    }
                };

                slideAnimation.Completed += (s, e) =>
                {
                    try
                    {
                        CurrentKeyboard.Visibility = Visibility.Collapsed;
                        CurrentKeyboard.Opacity = 1; // Reset for next show
                        CurrentKeyboard.RenderTransform = new TranslateTransform(0, 0);

                        onCompleted?.Invoke();
                    }
                    catch (Exception)
                    {
                        onCompleted?.Invoke();
                    }
                };

                slideTransform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
                CurrentKeyboard.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);

            }
            catch (Exception)
            {
                CurrentKeyboard.Visibility = Visibility.Collapsed;
                onCompleted?.Invoke();
            }
        }
        #endregion

        #region Event Handlers

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ShowKeyboard(textBox);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsAnyTextBoxFocused() && !IsKeyboardFocused())
                {
                    HideKeyboard();
                }
               
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

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

        private void TextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ShowKeyboard(textBox);
            }
        }

        private void OnKeyboardKeyPressed(object sender, VirtualKeyEventArgs e)
        {
            TextInput?.Invoke(this, e);

        }

        private void OnKeyboardCloseRequested(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void OnKeyboardVisibilityChanged(KeyboardVisibilityEventArgs args)
        {
            KeyboardVisibilityChanged?.Invoke(this, args);
        }

        #endregion

        #region Utility Methods

        private bool IsKeyboardFocused()
        {
            if (CurrentKeyboard == null) return false;

            try
            {
                var focusedElement = Keyboard.FocusedElement as DependencyObject;
                if (focusedElement == null) return false;

                return IsChildOf(focusedElement, CurrentKeyboard);
            }
            catch
            {
                return false;
            }
        }

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

    }

    #region Supporting Classes

    public class KeyboardVisibilityEventArgs : EventArgs
    {
        public bool IsVisible { get; set; }
        public TextBox TargetTextBox { get; set; }
    }

    #endregion
}