using System.ComponentModel;
using System.Runtime.CompilerServices;

using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public class ViewportViewModel
        : INotifyPropertyChanged
    {
        // =========================
        // DOCUMENTO
        // =========================

        public AraciDocument
            Document
        { get; }

        // =========================
        // CONSTRUTOR
        // =========================

        public ViewportViewModel(
            AraciDocument document)
        {
            Document = document;
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