using NetworkService.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService.Controls
{
    public partial class VirtualKeyboard : UserControl
    {
        #region Properties
        public VirtualKeyboardViewModel ViewModel => DataContext as VirtualKeyboardViewModel;
        #endregion

        #region Constructor
        public VirtualKeyboard()
        {
            InitializeComponent();
            DataContext = new VirtualKeyboardViewModel(); 
            Loaded += OnLoaded;
        }
        #endregion

        #region Event Handlers
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.KeyPressed += (s, args) => KeyPressed?.Invoke(this, args);
                ViewModel.CloseRequested += (s, args) => CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Public Events (for backward compatibility)
        public event EventHandler<VirtualKeyEventArgs> KeyPressed;
        public event EventHandler CloseRequested;
        #endregion

        #region Public Methods (delegated to ViewModel)
        public void SetTarget(TextBox textBox)
        {
            ViewModel?.SetTarget(textBox);
        }

        public void ClearTarget()
        {
            ViewModel?.ClearTarget();
        }

        #endregion
    }

}