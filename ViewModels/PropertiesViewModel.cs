using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels
{
    public class PropertiesViewModel
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
                if (_elementoSelecionado != value)
                {
                    _elementoSelecionado = value;

                    OnPropertyChanged();
                }
            }
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