using Microsoft.Xaml.Behaviors;
using NetworkService.ViewModel;
using System.Windows.Controls;

namespace NetworkService.Behaviors
{
    public class ScrollBehavior : Behavior<ScrollViewer>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AssociatedObject.DataContext is NetworkEntitiesViewModel viewModel)
            {
                viewModel.ScrollToTopAction = () => AssociatedObject.ScrollToTop();
            }
        }
    }
}