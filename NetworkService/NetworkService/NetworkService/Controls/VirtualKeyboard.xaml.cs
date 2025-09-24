using System;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService.Controls
{
    public partial class VirtualKeyboard : UserControl
    {
        #region Events

        public event EventHandler<VirtualKeyEventArgs> KeyPressed;
        public event EventHandler CloseRequested;

        #endregion

        #region Properties

        public bool IsShiftPressed { get; private set; } = false;
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

        private void LetterKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string letter)
            {
                string keyValue = IsShiftPressed ? letter.ToUpper() : letter.ToLower();
                SendKeyToTarget(keyValue);

                if (IsShiftPressed)
                {
                    IsShiftPressed = false;
                    UpdateKeyboardMode();
                }
            }
        }

        private void NumberKey_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string number)
            {
                SendKeyToTarget(number);
            }
        }

        private void SpaceKey_Click(object sender, RoutedEventArgs e)
        {
            SendKeyToTarget(" ");
        }

        private void PeriodKey_Click(object sender, RoutedEventArgs e)
        {
            SendKeyToTarget(".");
        }

        private void EnterKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnKeyPressed(new VirtualKeyEventArgs { Key = "Enter", Action = VirtualKeyAction.Enter });
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling Enter key: {ex.Message}");
            }
        }

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
                }
            }
        }

        private void ShiftKey_Click(object sender, RoutedEventArgs e)
        {
            IsShiftPressed = !IsShiftPressed;
            UpdateKeyboardMode();
            OnKeyPressed(new VirtualKeyEventArgs { Key = "Shift", Action = VirtualKeyAction.Shift });
        }

        #endregion

        #region Private Methods

        private void SendKeyToTarget(string keyValue)
        {
            if (TargetTextBox != null)
            {
                int caretIndex = TargetTextBox.CaretIndex;
                string currentText = TargetTextBox.Text ?? "";
                string newText = currentText.Insert(caretIndex, keyValue);
                TargetTextBox.Text = newText;
                TargetTextBox.CaretIndex = caretIndex + keyValue.Length;
            }
        }

        private void UpdateKeyboardMode()
        {
            UpdateLetterButtons();
            UpdateShiftButton();
        }

        private void UpdateLetterButtons()
        {
        }

        private void UpdateShiftButton()
        {
        }

        private void OnKeyPressed(VirtualKeyEventArgs args)
        {
            KeyPressed?.Invoke(this, args);
        }

        #endregion

        #region Public Methods

        public void SetTarget(TextBox textBox)
        {
            TargetTextBox = textBox;
        }

        public void ClearTarget()
        {
            TargetTextBox = null;
        }

        public void ResetKeyboard()
        {
            IsShiftPressed = false;
            UpdateKeyboardMode();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)

        {

            CloseRequested?.Invoke(this, EventArgs.Empty);

        }

        #endregion
    }

    #region Event Args and Enums

    public class VirtualKeyEventArgs : EventArgs
    {
        public string Key { get; set; }
        public VirtualKeyAction Action { get; set; }
    }

    public enum VirtualKeyAction
    {
        Character,
        Backspace,
        Enter,
        Shift
    }

    #endregion
}