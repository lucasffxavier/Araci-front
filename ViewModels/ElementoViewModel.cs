using Araci.Models;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public abstract class ElementoViewModel : ViewModelBase
    {
        protected readonly Elemento _modelo;

        private bool _isSelecionado;

        public bool IsSelecionado
        {
            get => _isSelecionado;
            set
            {
                if (_isSelecionado != value)
                {
                    _isSelecionado = value;
                    OnPropertyChanged();
                }
            }
        }

        protected ElementoViewModel(Elemento modelo)
        {
            _modelo = modelo;
        }

        public Elemento Modelo => _modelo;

        public double X
        {
            get => _modelo.PosicaoX;
            set
            {
                if (_modelo.PosicaoX != value)
                {
                    _modelo.PosicaoX = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Y
        {
            get => _modelo.PosicaoY;
            set
            {
                if (_modelo.PosicaoY != value)
                {
                    _modelo.PosicaoY = value;
                    OnPropertyChanged();
                }
            }
        }

        // 🔥 ESSENCIAL
        public virtual double Largura => 70;
        public virtual double Altura => 70;
    }
}