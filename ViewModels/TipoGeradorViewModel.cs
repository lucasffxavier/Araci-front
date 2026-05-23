using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoGeradorViewModel : TipoElementoViewModel
    {
        public TipoGeradorViewModel(TipoGerador tipo)
            : base(tipo)
        {
        }

        protected TipoGerador TipoGerador => (TipoGerador)_tipo;

        public int Fases
        {
            get => TipoGerador.Fases;
            set
            {
                if (TipoGerador.Fases == value)
                    return;

                TipoGerador.Fases = value;
                OnPropertyChanged();
            }
        }

        public int ModeloFonte
        {
            get => TipoGerador.ModeloFonte;
            set
            {
                if (TipoGerador.ModeloFonte == value)
                    return;

                TipoGerador.ModeloFonte = value;
                OnPropertyChanged();
            }
        }

        public double FatorPotencia
        {
            get => TipoGerador.FatorPotencia;
            set
            {
                if (TipoGerador.FatorPotencia == value)
                    return;

                TipoGerador.FatorPotencia = value;
                OnPropertyChanged();
            }
        }
    }
}
