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

        private TipoElementoViewModel?
            _tipoSelecionado;

        public ElementoViewModel?
            ElementoSelecionado
        {
            get => _elementoSelecionado;

            set
            {
                if (_elementoSelecionado == value)
                    return;

                _elementoSelecionado = value;

                TipoSelecionado =
                    value?.TipoViewModel;

                OnPropertyChanged();
            }
        }

        public TipoElementoViewModel?
            TipoSelecionado
        {
            get => _tipoSelecionado;

            set
            {
                if (_tipoSelecionado == value)
                    return;

                _tipoSelecionado = value;

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