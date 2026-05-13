using System.ComponentModel;
using System.Runtime.CompilerServices;

using Araci.ViewModels;

namespace Araci.Services
{
    public class EditorState
        : INotifyPropertyChanged
    {
        private ElementoViewModel?
            _elementoSelecionado;

        public ElementoViewModel?
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

        public event PropertyChangedEventHandler?
            PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName]
            string? nome = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nome));
        }
    }
}