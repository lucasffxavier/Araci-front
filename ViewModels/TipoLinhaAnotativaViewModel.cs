using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoLinhaAnotativaViewModel : TipoElementoViewModel
    {
        public TipoLinhaAnotativaViewModel(TipoLinhaAnotativa tipo)
            : base(tipo)
        {
        }

        protected TipoLinhaAnotativa TipoLinha => (TipoLinhaAnotativa)_tipo;

        public string EstiloLinha
        {
            get => TipoLinha.EstiloLinha;
            set
            {
                if (TipoLinha.EstiloLinha == value)
                    return;

                TipoLinha.EstiloLinha = value;
                OnPropertyChanged();
            }
        }
    }
}
