using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels
{
    public class EditorState
        : INotifyPropertyChanged
    {
        // =========================
        // ELEMENTO SELECIONADO
        // =========================

        private object?
            _elementoSelecionado;

        public object?
            ElementoSelecionado
        {
            get => _elementoSelecionado;

            set
            {
                if (_elementoSelecionado == value)
                    return;

                _elementoSelecionado = value;

                OnPropertyChanged();
            }
        }

        // =========================
        // PROPERTY CHANGED
        // =========================

        public event PropertyChangedEventHandler?
            PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName]
            string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}