using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.ViewModels
{
    public enum EditorSurfaceKind
    {
        Diagram,
        ProjectTable,
        ProjectSheet,
        ProjectSheetType
    }

    public class EditorState
        : INotifyPropertyChanged
    {
        // =========================
        // ELEMENTO SELECIONADO
        // =========================

        private object?
            _elementoSelecionado;

        private bool
            _navegadorProjetoVisivel;

        private EditorSurfaceKind
            _superficieAtiva = EditorSurfaceKind.Diagram;

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

        public bool
            NavegadorProjetoVisivel
        {
            get => _navegadorProjetoVisivel;

            set
            {
                if (_navegadorProjetoVisivel == value)
                    return;

                _navegadorProjetoVisivel = value;

                OnPropertyChanged();
            }
        }

        public EditorSurfaceKind
            SuperficieAtiva
        {
            get => _superficieAtiva;

            set
            {
                if (_superficieAtiva == value)
                    return;

                _superficieAtiva = value;

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
