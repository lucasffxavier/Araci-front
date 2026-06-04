using Araci.Models.Tipos;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public abstract class TipoElementoViewModel : ViewModelBase
    {
        protected TipoElemento _tipo;

        protected TipoElementoViewModel(TipoElemento tipo)
        {
            _tipo = tipo;
        }

        public TipoElemento Tipo => _tipo;

        public string NomeTipo
        {
            get => _tipo.NomeTipo;
            set
            {
                if (_tipo.NomeTipo == value)
                    return;

                _tipo.NomeTipo = string.IsNullOrWhiteSpace(value) ? "Tipo sem nome" : value.Trim();
                OnPropertyChanged();
            }
        }

        public string Familia
        {
            get => _tipo.Familia;
            set
            {
                if (_tipo.Familia == value)
                    return;

                _tipo.Familia = value;
                OnPropertyChanged();
            }
        }

        public string Categoria
        {
            get => _tipo.Categoria;
            set
            {
                if (_tipo.Categoria == value)
                    return;

                _tipo.Categoria = value;
                OnPropertyChanged();
            }
        }

        protected void AtualizarTipoBase(TipoElemento tipo)
        {
            if (ReferenceEquals(_tipo, tipo))
                return;

            _tipo = tipo;
            OnPropertyChanged(nameof(Tipo));
            OnPropertyChanged(nameof(NomeTipo));
            OnPropertyChanged(nameof(Familia));
            OnPropertyChanged(nameof(Categoria));
        }
    }
}