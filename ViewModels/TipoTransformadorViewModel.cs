using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoTransformadorViewModel : TipoElementoViewModel
    {
        public TipoTransformadorViewModel(TipoTransformador tipo)
            : base(tipo)
        {
        }

        private TipoTransformador TipoTransformador => (TipoTransformador)_tipo;

        public int Fases
        {
            get => TipoTransformador.Fases;
            set
            {
                if (TipoTransformador.Fases == value)
                    return;
                TipoTransformador.Fases = value;
                OnPropertyChanged();
            }
        }

        public int Enrolamentos
        {
            get => TipoTransformador.Enrolamentos;
            set
            {
                if (TipoTransformador.Enrolamentos == value)
                    return;
                TipoTransformador.Enrolamentos = value;
                OnPropertyChanged();
            }
        }

        public double RPercentual
        {
            get => TipoTransformador.RPercentual;
            set
            {
                if (TipoTransformador.RPercentual == value)
                    return;
                TipoTransformador.RPercentual = value;
                OnPropertyChanged();
            }
        }

        public double XPercentual
        {
            get => TipoTransformador.XPercentual;
            set
            {
                if (TipoTransformador.XPercentual == value)
                    return;
                TipoTransformador.XPercentual = value;
                OnPropertyChanged();
            }
        }

        public string LigacaoPrimario
        {
            get => TipoTransformador.LigacaoPrimario;
            set
            {
                if (TipoTransformador.LigacaoPrimario == value)
                    return;
                TipoTransformador.LigacaoPrimario = value;
                OnPropertyChanged();
            }
        }

        public string LigacaoSecundario
        {
            get => TipoTransformador.LigacaoSecundario;
            set
            {
                if (TipoTransformador.LigacaoSecundario == value)
                    return;
                TipoTransformador.LigacaoSecundario = value;
                OnPropertyChanged();
            }
        }
    }
}