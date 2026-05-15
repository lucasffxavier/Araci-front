using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels.Base
{
    public abstract class ViewModelBase
        : INotifyPropertyChanged,
          IDataErrorInfo
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Error =>
            string.Empty;

        public string this[string columnName] =>
            ValidateProperty(columnName);

        protected virtual string ValidateProperty(
            string propertyName)
        {
            return string.Empty;
        }

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
