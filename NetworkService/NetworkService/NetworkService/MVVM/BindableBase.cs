using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetworkService.MVVM
{
    public class BindableBase : INotifyPropertyChanged
    {
        protected virtual bool SetProperty<T>(ref T member, T val,
           [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return false;

            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}