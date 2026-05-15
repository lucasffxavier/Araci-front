using Araci.Models.Tipos;

namespace Araci.ViewModels
{
    public class TipoCaboViewModel : TipoElementoViewModel
    {
        protected TipoCabo TipoCabo => (TipoCabo)_tipo;

        public TipoCaboViewModel(TipoCabo tipo)
            : base(tipo)
        {
        }

        public double Resistencia
        {
            get => TipoCabo.Resistencia;
            set
            {
                if (TipoCabo.Resistencia == value)
                    return;

                TipoCabo.Resistencia = value;
                OnPropertyChanged();
            }
        }

        public double Reatancia
        {
            get => TipoCabo.Reatancia;
            set
            {
                if (TipoCabo.Reatancia == value)
                    return;

                TipoCabo.Reatancia = value;
                OnPropertyChanged();
            }
        }

        public double Capacitancia
        {
            get => TipoCabo.Capacitancia;
            set
            {
                if (TipoCabo.Capacitancia == value)
                    return;

                TipoCabo.Capacitancia = value;
                OnPropertyChanged();
            }
        }

        public double Ampacidade
        {
            get => TipoCabo.Ampacidade;
            set
            {
                if (TipoCabo.Ampacidade == value)
                    return;

                TipoCabo.Ampacidade = value;
                OnPropertyChanged();
            }
        }

        public int Fases
        {
            get => TipoCabo.Fases;
            set
            {
                if (TipoCabo.Fases == value)
                    return;

                TipoCabo.Fases = value;
                OnPropertyChanged();
            }
        }

        public bool Neutro
        {
            get => TipoCabo.Neutro;
            set
            {
                if (TipoCabo.Neutro == value)
                    return;

                TipoCabo.Neutro = value;
                OnPropertyChanged();
            }
        }
    }
}