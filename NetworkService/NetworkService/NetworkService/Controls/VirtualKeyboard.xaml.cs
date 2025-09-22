using System;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService.Controls
{
    /// <summary>
    /// Virtual Keyboard UserControl for mobile-style text input
    /// Specifikacija: CG3-Korisnici mobilnih telefona interface
    /// </summary>
    public partial class VirtualKeyboard : UserControl
    {
        #region Events

        /// <summary>
        /// Event fired when a key is pressed on virtual keyboard
        /// </summary>
        public event EventHandler<VirtualKeyEventArgs> KeyPressed;

        /// <summary>
        /// Event fired when keyboard close is requested
        /// </summary>
        public event EventHandler CloseRequested;

        #endregion

        #region Properties

        /// <summary>
        /// Current shift state (uppercase/lowercase)
        /// </summary>
        public bool IsShiftPressed { get; private set; } = false;

        /// <summary>
        /// Current number mode state
        /// </summary>
        public bool IsNumberMode { get; private set; } = false;

        /// <summary>
        /// Target TextBox that receives input
        /// </summary>
        public TextBox TargetTextBox { get; set; }

        #endregion

        #region Constructor

        public VirtualKeyboard()
        {
            InitializeComponent();
            UpdateKeyboardMode();
        }

        #endregion

        #region Key Event Handlers

        /// <summary>
        /// Handle letter key clicks (A-Z)
        /// </summary>
        private void LetterKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string letter)
            {
                string keyValue = IsShiftPressed ? letter.ToUpper() : letter.ToLower();
                SendKeyToTarget(keyValue);

                // Reset shift after letter input (like mobile keyboards)
                if (IsShiftPressed)
                {
                    IsShiftPressed = false;
                    UpdateKeyboardMode();
                }
            }
        }

        /// <summary>
        /// Handle number key clicks (0-9)
        /// </summary>
        private void NumberKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string number)
            {
                SendKeyToTarget(number);
            }
        }

        /// <summary>
        /// Handle space key click
        /// </summary>
        private void SpaceKey_Click(object sender, RoutedEventArgs e)
        {
            SendKeyToTarget(" ");
        }

        /// <summary>
        /// Handle period key click
        /// </summary>
        private void PeriodKey_Click(object sender, RoutedEventArgs e)
        {
            SendKeyToTarget(".");
        }

        /// <summary>
        /// Handle enter key click - close keyboard
        /// </summary>
        private void EnterKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fire key pressed event for Enter
                OnKeyPressed(new VirtualKeyEventArgs { Key = "Enter", Action = VirtualKeyAction.Enter });

                // Close keyboard after Enter (mobile behavior)
                CloseRequested?.Invoke(this, EventArgs.Empty);

                Console.WriteLine("Enter pressed - keyboard closing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling Enter key: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle backspace key click
        /// </summary>
        private void BackspaceKey_Click(object sender, RoutedEventArgs e)
        {
            if (TargetTextBox != null && !string.IsNullOrEmpty(TargetTextBox.Text))
            {
                int caretIndex = TargetTextBox.CaretIndex;
                if (caretIndex > 0)
                {
                    string currentText = TargetTextBox.Text;
                    string newText = currentText.Remove(caretIndex - 1, 1);
                    TargetTextBox.Text = newText;
                    TargetTextBox.CaretIndex = caretIndex - 1;

                    OnKeyPressed(new VirtualKeyEventArgs { Key = "Backspace", Action = VirtualKeyAction.Backspace });
                }
            }
        }

        /// <summary>
        /// Handle shift key click (toggle uppercase/lowercase)
        /// </summary>
        private void ShiftKey_Click(object sender, RoutedEventArgs e)
        {
            IsShiftPressed = !IsShiftPressed;
            UpdateKeyboardMode();

            OnKeyPressed(new VirtualKeyEventArgs { Key = "Shift", Action = VirtualKeyAction.Shift });
        }

        /// <summary>
        /// Handle number mode toggle key click
        /// </summary>
        private void NumberModeKey_Click(object sender, RoutedEventArgs e)
        {
            IsNumberMode = !IsNumberMode;
            UpdateKeyboardMode();

            OnKeyPressed(new VirtualKeyEventArgs { Key = "NumberMode", Action = VirtualKeyAction.ModeSwitch });
        }

        /// <summary>
        /// Handle close button click
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Send key value to target TextBox
        /// </summary>
        private void SendKeyToTarget(string keyValue)
        {
            if (TargetTextBox != null)
            {
                int caretIndex = TargetTextBox.CaretIndex;
                string currentText = TargetTextBox.Text ?? "";

                // Insert character at caret position
                string newText = currentText.Insert(caretIndex, keyValue);
                TargetTextBox.Text = newText;
                TargetTextBox.CaretIndex = caretIndex + keyValue.Length;

                // Keep focus on target TextBox
                TargetTextBox.Focus();

                // Fire key pressed event
                OnKeyPressed(new VirtualKeyEventArgs { Key = keyValue, Action = VirtualKeyAction.Character });
            }
        }

        /// <summary>
        /// Update keyboard visual state based on current mode
        /// </summary>
        private void UpdateKeyboardMode()
        {
            // Find all letter buttons and update their content
            UpdateLetterButtons();

            // Update shift button appearance
            UpdateShiftButton();

            // Update number mode button
            UpdateNumberModeButton();
        }

        /// <summary>
        /// Update letter button content based on shift state
        /// </summary>
        private void UpdateLetterButtons()
        {
            // This would ideally traverse all letter buttons and update case
            // For now, we'll rely on the click handler to determine case
            Console.WriteLine($"Keyboard mode updated: Shift={IsShiftPressed}, NumberMode={IsNumberMode}");
        }

        /// <summary>
        /// Update shift button visual state
        /// </summary>
        private void UpdateShiftButton()
        {
            // Find shift button and update its appearance
            // In a more complete implementation, we'd change the button style
        }

        /// <summary>
        /// Update number mode button visual state
        /// </summary>
        private void UpdateNumberModeButton()
        {
            // Find number mode button and update its content
            // Could show "ABC" when in number mode, "123" when in letter mode
        }

        /// <summary>
        /// Fire KeyPressed event
        /// </summary>
        private void OnKeyPressed(VirtualKeyEventArgs args)
        {
            KeyPressed?.Invoke(this, args);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set target TextBox for keyboard input
        /// </summary>
        public void SetTarget(TextBox textBox)
        {
            TargetTextBox = textBox;
        }

        /// <summary>
        /// Clear target TextBox
        /// </summary>
        public void ClearTarget()
        {
            TargetTextBox = null;
        }

        /// <summary>
        /// Reset keyboard to default state
        /// </summary>
        public void ResetKeyboard()
        {
            IsShiftPressed = false;
            IsNumberMode = false;
            UpdateKeyboardMode();
        }

        #endregion
    }

    #region Event Args and Enums

    /// <summary>
    /// Event arguments for virtual keyboard key press
    /// </summary>
    public class VirtualKeyEventArgs : EventArgs
    {
        public string Key { get; set; }
        public VirtualKeyAction Action { get; set; }
    }

    /// <summary>
    /// Types of virtual keyboard actions
    /// </summary>
    public enum VirtualKeyAction
    {
        Character,      // Regular character input
        Backspace,      // Delete previous character
        Enter,          // Enter/return key
        Shift,          // Shift toggle
        ModeSwitch      // Switch between letters/numbers
    }

    #endregion
}