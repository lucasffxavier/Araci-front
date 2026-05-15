using Araci.Models.Tipos;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public abstract class TipoElementoViewModel : ViewModelBase
    {
        protected readonly TipoElemento _tipo;

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

                _tipo.NomeTipo = value;
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
    }
}