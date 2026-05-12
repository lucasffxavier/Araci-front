using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels
{
    public class ViewportViewModel
        : INotifyPropertyChanged
    {
        // =========================
        // ELEMENTOS
        // =========================

        public ObservableCollection<ElementoViewModel>
            Elementos
        { get; }

        // =========================
        // CONSTRUTOR
        // =========================

        public ViewportViewModel()
        {
            Elementos =
                new ObservableCollection<
                    ElementoViewModel>();
        }

        // =========================
        // PROPERTY CHANGED
        // =========================

        public event PropertyChangedEventHandler?
            PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName]
            string? nome = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nome));
        }
    }
}