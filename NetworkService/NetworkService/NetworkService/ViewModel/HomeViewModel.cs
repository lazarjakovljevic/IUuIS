using NetworkService.MVVM;

namespace NetworkService.ViewModel
{
    public class HomeViewModel : BindableBase
    {
        private string welcomeMessage;
        public string WelcomeMessage
        {
            get { return welcomeMessage; }
            set { SetProperty(ref welcomeMessage, value); }
        }

        public HomeViewModel()
        {
            WelcomeMessage = "Welcome to Network Infrastructure System";
        }
    }
}