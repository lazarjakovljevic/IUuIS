#region Using directives
using NetworkService.MVVM;
using System;
using System.Windows.Controls;
using System.Windows.Input;
#endregion

namespace NetworkService.ViewModel
{
    public class VirtualKeyboardViewModel : BindableBase
    {
        #region Private Fields
        private bool _isShiftPressed;
        private TextBox _targetTextBox;
        #endregion

        #region Public Properties
        public bool IsShiftPressed
        {
            get => _isShiftPressed;
            set => SetProperty(ref _isShiftPressed, value);
        }

        public TextBox TargetTextBox
        {
            get => _targetTextBox;
            set => SetProperty(ref _targetTextBox, value);
        }
        #endregion

        #region Commands
        public ICommand KeyPressCommand { get; }
        public ICommand EnterKeyCommand { get; }
        public ICommand BackspaceKeyCommand { get; }
        public ICommand ShiftKeyCommand { get; }
        public ICommand CloseKeyboardCommand { get; }
        #endregion

        #region Events
        public event EventHandler<VirtualKeyEventArgs> KeyPressed;
        public event EventHandler CloseRequested;
        #endregion

        #region Constructor
        public VirtualKeyboardViewModel()
        {
            KeyPressCommand = new MyICommand<string>(ExecuteKeyPress);
            EnterKeyCommand = new MyICommand(ExecuteEnterKey);
            BackspaceKeyCommand = new MyICommand(ExecuteBackspaceKey);
            ShiftKeyCommand = new MyICommand(ExecuteShiftKey);
            CloseKeyboardCommand = new MyICommand(ExecuteCloseKeyboard);
        }
        #endregion

        #region Command Implementations
        private void ExecuteKeyPress(string keyValue)
        {
            try
            {
                if (!string.IsNullOrEmpty(keyValue))
                {
                    SendKeyToTarget(keyValue);
                    OnKeyPressed(new VirtualKeyEventArgs { Key = keyValue, Action = VirtualKeyAction.Character });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling key press: {ex.Message}");
            }
        }

        private void ExecuteEnterKey()
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

        private void ExecuteBackspaceKey()
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

        private void ExecuteShiftKey()
        {
            IsShiftPressed = !IsShiftPressed;
            UpdateKeyboardMode();
            OnKeyPressed(new VirtualKeyEventArgs { Key = "Shift", Action = VirtualKeyAction.Shift });
        }

        private void ExecuteCloseKeyboard()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
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
            // Implementacija za ažuriranje dugmića slova
        }

        private void UpdateShiftButton()
        {
            // Implementacija za ažuriranje Shift dugmeta
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