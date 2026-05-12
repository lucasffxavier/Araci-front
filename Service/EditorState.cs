using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Araci.ViewModels;

namespace Araci.Services
{
    public class EditorState
        : INotifyPropertyChanged
    {
        // =========================
        // SELEÇÃO
        // =========================

        public ObservableCollection<ElementoViewModel>
            ElementosSelecionados
        { get; }
            = new();

        // =========================
        // ELEMENTO PRINCIPAL
        // =========================

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

        // =========================
        // CONTAGEM
        // =========================

        public int QuantidadeSelecionados =>
            ElementosSelecionados.Count;

        // =========================
        // HELPERS
        // =========================

        public bool PossuiSelecao =>
            ElementosSelecionados.Any();

        // =========================
        // PROPERTY CHANGED
        // =========================

        public event PropertyChangedEventHandler?
            PropertyChanged;

        public void NotifySelecaoAlterada()
        {
            OnPropertyChanged(nameof(QuantidadeSelecionados));
            OnPropertyChanged(nameof(PossuiSelecao));
        }

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