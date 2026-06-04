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

        public virtual string NomeTipo
        {
            get => _tipo.NomeTipo;
            set
            {
                string nomeNormalizado = string.IsNullOrWhiteSpace(value) ? "Tipo sem nome" : value.Trim();

                if (_tipo.NomeTipo == nomeNormalizado)
                    return;

                _tipo.NomeTipo = nomeNormalizado;
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

        public virtual void CommitChanges()
        {
        }

        public virtual void CancelChanges()
        {
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
